using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;
using Pulsar.BuildingBlocks.EventBus.Events;

namespace Pulsar.BuildingBlocks.DDD.Mongo.EventBus;

public class MongoSaveIntegrationEventLog : ISaveIntegrationEventLog
{
    private readonly MongoDbSession _session;
    private readonly IMongoCollection<IntegrationEventLogEntry> _collection;

    public MongoSaveIntegrationEventLog(MongoDbSession session)
    {
        _session = session;
        _collection = session.Database.GetCollection<IntegrationEventLogEntry>(Constants.EVENT_LOG_COLLECTION_NAME)
            .WithWriteConcern(WriteConcern.WMajority);
    }

    public async Task SaveEventAsync(IntegrationEvent @event, CancellationToken ct = default)
    {
        var entry = new IntegrationEventLogEntry(@event);
        await _collection.InsertOneAsync(_session.CurrentHandle, entry, cancellationToken: ct);
        if (@event is IPushNotificationEvent)
        {
            var data = ((IPushNotificationEvent)@event).GetPushNotificationData();
            if (data != null)
			{
				var pn = new PushNotificationEvent(data);
                var pnEntry = new IntegrationEventLogEntry(pn);
				await _collection.InsertOneAsync(_session.CurrentHandle, pnEntry, cancellationToken: ct);
			}
        }
    }
}
