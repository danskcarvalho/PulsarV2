using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Pulsar.BuildingBlocks.DDD.Mongo;
using Pulsar.BuildingBlocks.EventBus.Abstractions;
using Pulsar.BuildingBlocks.EventBus.Events;

namespace Pulsar.BuildingBlocks.Sync.Service;

internal class MyMongoSaveIntegrationEventLog : ISaveIntegrationEventLog
{
    private readonly IMongoCollection<IntegrationEventLogEntry> _collection;

    public MyMongoSaveIntegrationEventLog(IMongoDatabase database)
    {
        _collection = database.GetCollection<IntegrationEventLogEntry>(Constants.EVENT_LOG_COLLECTION_NAME)
            .WithWriteConcern(WriteConcern.WMajority);
    }

    public async Task SaveEventAsync(IntegrationEvent @event, CancellationToken ct = default)
    {
        var entry = new IntegrationEventLogEntry(@event);
        await _collection.InsertOneAsync(entry, cancellationToken: ct);
    }
}
