namespace Pulsar.BuildingBlocks.EventBus;

public partial class GenericIntegrationEventDispatcherService : IIntegrationEventDispatcherService
{
    private readonly IIntegrationEventLogStorage _Storage;
    private readonly ILogger<GenericIntegrationEventDispatcherService> _Logger;
    private readonly IEventBus _EventBus;
    public GenericIntegrationEventDispatcherServiceOptions Options { get; }

    public GenericIntegrationEventDispatcherService(
        IIntegrationEventLogStorage storage, 
        ILogger<GenericIntegrationEventDispatcherService> logger, 
        IEventBus eventBus, 
        GenericIntegrationEventDispatcherServiceOptions options)
    {
        _Storage = storage;
        _Logger = logger;
        _EventBus = eventBus;
        Options = options;
    }

    public async Task Run(CancellationToken ct)
    {
        var producer = new Producer(_Logger, _Storage, Options);
        var consumers = new List<Consumer>();
        for (int i = 0; i < Options.MaxConsumers; i++)
        {
            consumers.Add(new Consumer(producer, _EventBus, _Storage, Options, _Logger));
        }

        List<Task> tasks = new List<Task>();
        tasks.Add(producer.Run(ct));
        foreach (var c in consumers)
        {
            tasks.Add(c.Run(ct));
        }
        await Task.WhenAll(tasks);
    }
}

public class GenericIntegrationEventDispatcherServiceOptions
{
    /// <summary>
    /// Polling timeout in milliseconds.
    /// </summary>
    public int PollingTimeout { get; set; }
    public int MaxConsumers { get; set; }
}
