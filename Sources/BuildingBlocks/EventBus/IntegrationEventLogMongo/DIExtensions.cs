using Pulsar.BuildingBlocks.Utils;

namespace Pulsar.BuildingBlocks.IntegrationEventLogMongo;

public static class DIExtensions
{
    public static void AddMongoEventBus(this IServiceCollection col)
    {
        col.AddSingleton<IIntegrationEventLogStorage, MongoIntegrationEventLogStorage>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var settings = MongoClientSettings.FromConnectionString(config.GetOrThrow("EventBusDispatcher:MongoDB:ConnectionString")) ?? throw new InvalidOperationException("invalid connection string");

            settings.RetryReads = true;
            settings.RetryWrites = true;
            settings.ReadConcern = ReadConcern.Majority;
            settings.WriteConcern = WriteConcern.WMajority;
            settings.ReadPreference = ReadPreference.Primary;
            settings.LinqProvider = MongoDB.Driver.Linq.LinqProvider.V3;

            var client = new MongoClient(settings);
            return new MongoIntegrationEventLogStorage(
                client.GetDatabase(config.GetOrThrow("EventBusDispatcher:MongoDB:Database")),
                sp.GetRequiredService<ILogger<MongoIntegrationEventLogStorage>>());
        });
    }
}
