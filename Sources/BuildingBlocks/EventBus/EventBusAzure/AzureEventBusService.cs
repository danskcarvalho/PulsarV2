namespace Pulsar.BuildingBlocks.EventBusAzure;

public class AzureEventBusService : IEventBus
{
    private readonly string _namespace;
    private readonly string _topicName;
    private ILogger<AzureEventBusService> _logger;
    private ServiceBusClient? _client;

    public AzureEventBusService(ILogger<AzureEventBusService> logger, string @namespace, string topicName)
    {
        _logger = logger;
        _namespace = @namespace;
        _topicName = topicName;
        _client = new ServiceBusClient(
            _namespace,
            new DefaultAzureCredential());
    }

    public async ValueTask DisposeAsync()
    {
        if (_client != null)
        {
            await _client.DisposeAsync();
            _client = null;
        }
    }

    public async Task<List<(HashSet<Guid> Ids, Exception? Exception)>> Publish(IEnumerable<(string EventName, IntegrationEvent Event)> eventBatch)
    {
        var retryPolicy = Policy
                  .Handle<Exception>(e => true)
                  .WaitAndRetryAsync(3,
                      retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 0.5 + Math.Pow(2, retryAttempt) * Random.Shared.NextDouble()),
                      (e, ts) =>
                      {
                          _logger.LogError(e, $"error while trying to publish events");
                          _logger.LogInformation($"will retry in {(int)ts.TotalSeconds} seconds");
                      });

        List<(HashSet<Guid> Ids, Exception? Exception)> result = new();
        var list = eventBatch.ToList();
        var sent = new List<(string EventName, IntegrationEvent Event)>();
        var next = new List<(string EventName, IntegrationEvent Event)>();

        if (list.Count == 0)
            return new List<(HashSet<Guid> Ids, Exception? Exception)>();

        await using var sender = _client!.CreateSender(_topicName);

        do
        {
            // create a batch 
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            for (int i = 1; i <= list.Count; i++)
            {
                // try adding a message to the batch
                var body = JsonSerializer.Serialize(list[i].Event, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                var msg = new ServiceBusMessage(body);
                msg.ContentType = "application/json";
                msg.Subject = list[i].EventName;

                if (!messageBatch.TryAddMessage(msg))
                {
                    // if it is too large for the batch
                    for (int j = i; j < list.Count; j++)
                    {
                        next.Add(list[j]);
                    }
                    break;
                }
                else
                {
                    sent.Add(list[i]);
                }
            }

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    await sender.SendMessagesAsync(messageBatch);
                });
                result.Add((new HashSet<Guid>(sent.Select(s => s.Event.Id)), null));
            }
            catch (Exception e)
            {
                result.Add((new HashSet<Guid>(sent.Select(s => s.Event.Id)), e));
            }

            var temp = list;
            list = next;
            sent.Clear();
            next = temp;
            next.Clear();
        } while (list.Count != 0);

        return result;
    }
}
