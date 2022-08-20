namespace Pulsar.BuildingBlocks.Migrations.EventBus;

public static class MigrationsExtensions
{
    public static async Task UpEventLogs(this Migration mig)
    {
        if (mig.Database == null)
            return;

        if (!(await mig.Database.CollectionExists(DDD.Mongo.Constants.EVENT_LOG_COLLECTION_NAME)))
            await mig.Database.CreateCollectionAsync(DDD.Mongo.Constants.EVENT_LOG_COLLECTION_NAME);

        var col = mig.Database.GetCollection<IntegrationEventLogEntry>(DDD.Mongo.Constants.EVENT_LOG_COLLECTION_NAME);


        var ix_pending_scheduled_on = Builders<IntegrationEventLogEntry>.IndexKeys
            .Ascending(e => e.Status)
            .Ascending(e => e.ScheduledOn);
        var ix_pending_expiration_date = Builders<IntegrationEventLogEntry>.IndexKeys
            .Ascending(e => e.Status)
            .Ascending(e => e.InProgressExpirationDate);
        var ix_pending_restore = Builders<IntegrationEventLogEntry>.IndexKeys
            .Ascending(e => e.Status)
            .Ascending(e => e.InProgressRestore);

        await col.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<IntegrationEventLogEntry>(ix_pending_scheduled_on, new CreateIndexOptions()
        {
            Name = "ix_pending_scheduled_on"
        }));
        await col.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<IntegrationEventLogEntry>(ix_pending_expiration_date, new CreateIndexOptions()
        {
            Name = "ix_pending_expiration_date"
        }));
        await col.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<IntegrationEventLogEntry>(ix_pending_restore, new CreateIndexOptions()
        {
            Name = "ix_pending_restore"
        }));
    }
}
