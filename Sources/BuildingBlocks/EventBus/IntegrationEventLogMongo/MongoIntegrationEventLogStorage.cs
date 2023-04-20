using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using System;

namespace Pulsar.BuildingBlocks.IntegrationEventLogMongo;

public class MongoIntegrationEventLogStorage : IIntegrationEventLogStorage
{
    private readonly IMongoDatabase _Database;
    private readonly IMongoCollection<IntegrationEventLogEntry> _Collection;
    private readonly IMongoCollection<EventProducer> _Producers;
    private readonly ILogger<MongoIntegrationEventLogStorage> _Logger;

    public MongoIntegrationEventLogStorage(IMongoDatabase database, ILogger<MongoIntegrationEventLogStorage> logger)
    {
        _Database = database ?? throw new ArgumentNullException(nameof(database));
        _Collection = _Database.GetCollection<IntegrationEventLogEntry>(Constants.EVENT_LOG_COLLECTION_NAME)
            .WithWriteConcern(WriteConcern.WMajority)
            .WithReadConcern(ReadConcern.Majority)
            .WithReadPreference(ReadPreference.PrimaryPreferred);
        _Producers = _Database.GetCollection<EventProducer>(Constants.EVENT_PRODUCERS_COLLECTION_NAME)
            .WithWriteConcern(WriteConcern.WMajority)
            .WithReadConcern(ReadConcern.Majority)
            .WithReadPreference(ReadPreference.PrimaryPreferred);
        _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(Guid? ProducerId, int ProducerSeq, int ProducerCount)> CheckInProducer(Guid? producerId, TimeSpan producerTimeout, CancellationToken ct = default)
    {
        while (true)
        {
            using var session = await _Producers.Database.Client.StartSessionAsync(new ClientSessionOptions()
            {
                CausalConsistency = true
            }, ct);
            if (producerId == null)
            {
                producerId = Guid.NewGuid();
                await _Producers.InsertOneAsync(session, new EventProducer()
                {
                    Id = producerId.Value,
                    CheckedOn = DateTime.UtcNow,
                    CreatedOn = DateTime.UtcNow
                });
            }
            else
            {
                var upd = Builders<EventProducer>.Update.Set(x => x.CheckedOn, DateTime.UtcNow);
                // -- check myself
                await _Producers.UpdateOneAsync(session, x => x.Id == producerId.Value, upd);
            }

            // -- delete unhealthy producers 
            var timeoutLimit = DateTime.UtcNow.Add(-producerTimeout);
            await _Producers.DeleteManyAsync(session, x => x.CheckedOn <= timeoutLimit);

            // -- read producers
            var allProducers = await (await _Producers.FindAsync(session, x => true)).ToListAsync();
            allProducers = allProducers.OrderBy(x => x.CreatedOn).ToList();

            for (int i = 0; i < allProducers.Count; i++)
            {
                var prod = allProducers[i];
                if (prod.Id == producerId.Value)
                {
                    return (producerId.Value, i, allProducers.Count);
                }
            }

            producerId = null;
        }
    }

    public async Task CheckOutProducer(Guid producerId, CancellationToken ct = default)
    {
        await _Producers.DeleteOneAsync(x => x.Id == producerId, ct);
    }

    public async Task MarkEventsAsFailedAsync(List<(Guid EventId, long Version)> events, CancellationToken ct)
    {
        var listWrites = new List<WriteModel<IntegrationEventLogEntry>>();
        foreach (var (eventId, version) in events)
        {
            var filter = Builders<IntegrationEventLogEntry>.Filter.Where(evt => evt.Id == eventId && evt.Version == version);
            var update = Builders<IntegrationEventLogEntry>.Update
                .Inc(evt => evt.Version, 1)
                .Set(evt => evt.Status, IntegrationEventStatus.Failed);

            listWrites.Add(new UpdateOneModel<IntegrationEventLogEntry>(filter, update));
        }
        
        await _Collection.BulkWriteAsync(listWrites, cancellationToken: ct);
    }

    public async Task MarkEventsAsFailedAsync(List<(Guid EventId, List<IntegrationEventLogEntrySendAttempt> Attempts)> events, CancellationToken ct)
    {
        var listWrites = new List<WriteModel<IntegrationEventLogEntry>>();
        foreach (var (eventId, attempts) in events)
        {
            var filter = Builders<IntegrationEventLogEntry>.Filter.Where(evt => evt.Id == eventId);
            var update = Builders<IntegrationEventLogEntry>.Update
                .Inc(evt => evt.Version, 1)
                .Set(evt => evt.Status, IntegrationEventStatus.Failed)
                .Set(evt => evt.Attempts, attempts);

            listWrites.Add(new UpdateOneModel<IntegrationEventLogEntry>(filter, update));
        }

        await _Collection.BulkWriteAsync(listWrites, cancellationToken: ct);
    }

    public async Task<HashSet<Guid>> MarkEventsAsInProgressAsync(List<Guid> eventIds, DateTime restorationDate, DateTime expirationDate, CancellationToken ct)
    {
        using var session = await _Producers.Database.Client.StartSessionAsync(new ClientSessionOptions()
        {
            CausalConsistency = true
        }, ct);

        var filter = Builders<IntegrationEventLogEntry>.Filter.Where(evt => eventIds.Contains(evt.Id) && evt.Status == IntegrationEventStatus.Pending);
        var update = Builders<IntegrationEventLogEntry>.Update
            .Inc(evt => evt.Version, 1)
            .Set(evt => evt.Status, IntegrationEventStatus.InProgress)
            .Set(evt => evt.InProgressRestore, restorationDate)
            .Set(evt => evt.InProgressExpirationDate, expirationDate);
        await _Collection.UpdateManyAsync(session, filter, update, cancellationToken: ct);
        var entries = await (await _Collection.FindAsync<IntegrationEventLogEntry>(session, evt => eventIds.Contains(evt.Id) && evt.Status == IntegrationEventStatus.InProgress)).ToListAsync();
        return new HashSet<Guid>(entries.Select(e => e.Id));
    }

    public async Task MarkEventsAsPublishedAsync(List<(Guid EventId, List<IntegrationEventLogEntrySendAttempt> Attempts)> events, CancellationToken ct)
    {
        var listWrites = new List<WriteModel<IntegrationEventLogEntry>>();
        foreach (var (eventId, attempts) in events)
        {
            var filter = Builders<IntegrationEventLogEntry>.Filter.Where(evt => evt.Id == eventId);
            var update = Builders<IntegrationEventLogEntry>.Update
                .Inc(evt => evt.Version, 1)
                .Set(evt => evt.Status, IntegrationEventStatus.Published)
                .Set(evt => evt.Attempts, attempts);

            listWrites.Add(new UpdateOneModel<IntegrationEventLogEntry>(filter, update));
        }

        await _Collection.BulkWriteAsync(listWrites, cancellationToken: ct);
    }

    public async Task RescheduleEventsAsync(List<(Guid EventId, List<IntegrationEventLogEntrySendAttempt> Attempts, DateTime ScheduledOn)> events, CancellationToken ct)
    {
        var listWrites = new List<WriteModel<IntegrationEventLogEntry>>();
        foreach (var (eventId, attempts, scheduledOn) in events)
        {
            var filter = Builders<IntegrationEventLogEntry>.Filter.Where(evt => evt.Id == eventId);
            var update = Builders<IntegrationEventLogEntry>.Update
                .Inc(evt => evt.Version, 1)
                .Set(evt => evt.Status, IntegrationEventStatus.Pending)
                .Set(evt => evt.InProgressExpirationDate, null)
                .Set(evt => evt.InProgressRestore, null)
                .Set(evt => evt.ScheduledOn, scheduledOn)
                .Set(evt => evt.Attempts, attempts);

            listWrites.Add(new UpdateOneModel<IntegrationEventLogEntry>(filter, update));
        }

        await _Collection.BulkWriteAsync(listWrites, cancellationToken: ct);
    }

    public async Task RestoreEventsAsync(List<(Guid EventId, long Version)> events, CancellationToken ct)
    {
        var listWrites = new List<WriteModel<IntegrationEventLogEntry>>();
        foreach (var (eventId, version) in events)
        {
            var filter = Builders<IntegrationEventLogEntry>.Filter.Where(evt => evt.Id == eventId && evt.Version == version);
            var update = Builders<IntegrationEventLogEntry>.Update
                .Inc(evt => evt.Version, 1)
                .Set(evt => evt.Status, IntegrationEventStatus.Pending)
                .Set(evt => evt.InProgressExpirationDate, null)
                .Set(evt => evt.InProgressRestore, null)
                .Set(evt => evt.ScheduledOn, DateTime.UtcNow);

            listWrites.Add(new UpdateOneModel<IntegrationEventLogEntry>(filter, update));
        }

        await _Collection.BulkWriteAsync(listWrites, cancellationToken: ct);
    }

    public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveRelevantEventLogsAsync(int maxEvents, int producerSeq, int producersCount, CancellationToken ct)
    {
        var list = await (await _Collection.FindAsync(
            e =>
                 e.TimeStamp % producersCount == producerSeq &&
                 ((e.Status == IntegrationEventStatus.Pending && DateTime.UtcNow > e.ScheduledOn) ||
                 (e.Status == IntegrationEventStatus.Pending && e.ScheduledOn == null) ||
                 (e.Status == IntegrationEventStatus.InProgress && DateTime.UtcNow > e.InProgressExpirationDate) ||
                 (e.Status == IntegrationEventStatus.InProgress && DateTime.UtcNow > e.InProgressRestore)),
            new FindOptions<IntegrationEventLogEntry, IntegrationEventLogEntry>()
            {
                Limit = maxEvents,
                Sort = Builders<IntegrationEventLogEntry>.Sort.Ascending(evt => evt.ScheduledOn)
            })).ToListAsync();

        return list;
    }

    public async Task WatchRelevantEventLogsAsync(Func<IntegrationEventLogEntry, CancellationToken, Task> onNewEvent, int producerSeq, int producersCount, CancellationToken ct)
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
                    .Match(x =>
                        x.OperationType == ChangeStreamOperationType.Insert &&
                        x.FullDocument.TimeStamp % producersCount == producerSeq &&
                        ((x.FullDocument.Status == IntegrationEventStatus.Pending && DateTime.UtcNow > x.FullDocument.ScheduledOn) ||
                         (x.FullDocument.Status == IntegrationEventStatus.Pending && x.FullDocument.ScheduledOn == null) ||
                         (x.FullDocument.Status == IntegrationEventStatus.InProgress && DateTime.UtcNow > x.FullDocument.InProgressExpirationDate) ||
                         (x.FullDocument.Status == IntegrationEventStatus.InProgress && DateTime.UtcNow > x.FullDocument.InProgressRestore)));
                using (var cursor = await _Collection.WatchAsync(pipeline))
                {
                    await cursor.ForEachAsync(async change =>
                    {
                        timeout = 1000;
                        if (change.FullDocument.MayNeedProcessing())
                            await onNewEvent(change.FullDocument, ct);
                    }, ct);
                }
            }
            catch (TaskCanceledException)
            {
                if (ct.IsCancellationRequested)
                    break;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "error while trying to watch over collection");
                await Task.Delay(timeout);
                timeout *= 2;
                timeout = Math.Min(timeout, 5 * 60 * 1000); // --> max of 5 minutes
            }
        }
    }
}
