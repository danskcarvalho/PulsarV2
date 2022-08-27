namespace Pulsar.BuildingBlocks.IntegrationEventLogMongo;

public class MongoIntegrationEventLogStorage : IIntegrationEventLogStorage
{
    private readonly IMongoDatabase _Database;
    private readonly IMongoCollection<IntegrationEventLogEntry> _Collection;
    private readonly ILogger<MongoIntegrationEventLogStorage> _Logger;

    public MongoIntegrationEventLogStorage(IMongoDatabase database, ILogger<MongoIntegrationEventLogStorage> logger)
    {
        _Database = database ?? throw new ArgumentNullException(nameof(database));
        _Collection = _Database.GetCollection<IntegrationEventLogEntry>(Constants.EVENT_LOG_COLLECTION_NAME)
            .WithWriteConcern(WriteConcern.WMajority)
            .WithReadConcern(ReadConcern.Majority)
            .WithReadPreference(ReadPreference.PrimaryPreferred);
        _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task MarkEventAsFailedAsync(Guid eventId, long version, CancellationToken ct)
    {
        var filter = Builders<IntegrationEventLogEntry>.Filter.Where(evt => evt.Id == eventId && evt.Version == version);
        var update = Builders<IntegrationEventLogEntry>.Update
            .Inc(evt => evt.Version, 1)
            .Set(evt => evt.Status, IntegrationEventStatus.Failed);
        await _Collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task MarkEventAsFailedAsync(Guid eventId, List<IntegrationEventLogEntrySendAttempt> attempts, CancellationToken ct)
    {
        var filter = Builders<IntegrationEventLogEntry>.Filter.Where(evt => evt.Id == eventId);
        var update = Builders<IntegrationEventLogEntry>.Update
            .Inc(evt => evt.Version, 1)
            .Set(evt => evt.Status, IntegrationEventStatus.Failed)
            .Set(evt => evt.Attempts, attempts);
        await _Collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task<bool> MarkEventAsInProgressAsync(Guid eventId, DateTime restorationDate, DateTime expirationDate, CancellationToken ct)
    {
        var filter = Builders<IntegrationEventLogEntry>.Filter.Where(evt => evt.Id == eventId && evt.Status == IntegrationEventStatus.Pending);
        var update = Builders<IntegrationEventLogEntry>.Update
            .Inc(evt => evt.Version, 1)
            .Set(evt => evt.Status, IntegrationEventStatus.InProgress)
            .Set(evt => evt.InProgressRestore, restorationDate)
            .Set(evt => evt.InProgressExpirationDate, expirationDate);
        var r = await _Collection.UpdateOneAsync(filter, update, cancellationToken: ct);
        return r.IsAcknowledged && r.ModifiedCount > 0;
    }

    public async Task MarkEventAsPublishedAsync(Guid eventId, List<IntegrationEventLogEntrySendAttempt> attempts, CancellationToken ct)
    {
        var filter = Builders<IntegrationEventLogEntry>.Filter.Where(evt => evt.Id == eventId);
        var update = Builders<IntegrationEventLogEntry>.Update
            .Inc(evt => evt.Version, 1)
            .Set(evt => evt.Status, IntegrationEventStatus.Published)
            .Set(evt => evt.Attempts, attempts);
        await _Collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task RescheduleEventAsync(Guid eventId, List<IntegrationEventLogEntrySendAttempt> attempts, DateTime scheduledOn, CancellationToken ct)
    {
        var filter = Builders<IntegrationEventLogEntry>.Filter.Where(evt => evt.Id == eventId);
        var update = Builders<IntegrationEventLogEntry>.Update
            .Inc(evt => evt.Version, 1)
            .Set(evt => evt.Status, IntegrationEventStatus.Pending)
            .Set(evt => evt.InProgressExpirationDate, null)
            .Set(evt => evt.InProgressRestore, null)
            .Set(evt => evt.ScheduledOn, scheduledOn)
            .Set(evt => evt.Attempts, attempts);
        await _Collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task RestoreEventAsync(Guid eventId, long version, CancellationToken ct)
    {
        var filter = Builders<IntegrationEventLogEntry>.Filter.Where(evt => evt.Id == eventId && evt.Version == version);
        var update = Builders<IntegrationEventLogEntry>.Update
            .Inc(evt => evt.Version, 1)
            .Set(evt => evt.Status, IntegrationEventStatus.Pending)
            .Set(evt => evt.InProgressExpirationDate, null)
            .Set(evt => evt.InProgressRestore, null)
            .Set(evt => evt.ScheduledOn, DateTime.UtcNow);
        await _Collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveRelevantEventLogsAsync(int maxEvents, CancellationToken ct)
    {
        var list = await (await _Collection.FindAsync(
            e => (e.Status == IntegrationEventStatus.Pending && DateTime.UtcNow > e.ScheduledOn) ||
                 (e.Status == IntegrationEventStatus.InProgress && DateTime.UtcNow > e.InProgressExpirationDate) || 
                 (e.Status == IntegrationEventStatus.InProgress && DateTime.UtcNow > e.InProgressRestore),
            new FindOptions<IntegrationEventLogEntry, IntegrationEventLogEntry>()
            {
                Limit = maxEvents,
                Sort = Builders<IntegrationEventLogEntry>.Sort.Ascending(evt => evt.ScheduledOn)
            })).ToListAsync();

        return list;
    }

    public async Task WatchRelevantEventLogsAsync(Func<IntegrationEventLogEntry, CancellationToken, Task> onNewEvent, CancellationToken ct)
    {
        int timeout = 1000;
        while (true)
        {
            if (ct.IsCancellationRequested)
                break;
            try
            {
                var pipeline =
                    new EmptyPipelineDefinition<ChangeStreamDocument<IntegrationEventLogEntry>>()
                    .Match(x => x.OperationType == ChangeStreamOperationType.Insert);
                using (var cursor = await _Collection.WatchAsync(pipeline))
                {
                    await cursor.ForEachAsync(change =>
                    {
                        timeout = 1000;
                        if (change.FullDocument.MayNeedProcessing())
                            onNewEvent(change.FullDocument, ct);
                    }, ct);
                }
            }
            catch (TaskCanceledException)
            {
                if (ct.IsCancellationRequested)
                    break;
            }
            catch(Exception ex)
            {
                _Logger.LogError(ex, "error while trying to watch over collection");
                await Task.Delay(timeout);
                timeout *= 2;
                timeout = Math.Min(timeout, 5 * 60 * 1000); // --> max of 5 minutes
            }
        }
    }
}
