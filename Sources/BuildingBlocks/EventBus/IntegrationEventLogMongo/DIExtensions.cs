//#define NO_COMMAND_OUTPUT
using MongoDB.Driver.Core.Events;
using Pulsar.BuildingBlocks.DDD.Mongo.Mapping;
using Pulsar.BuildingBlocks.Utils;
using System.Diagnostics;

namespace Pulsar.BuildingBlocks.IntegrationEventLogMongo;

public static class DIExtensions
{
    public static void AddMongoEventBus(this IServiceCollection col)
    {
        AutoMappingConventions.Register();
        col.AddSingleton<IIntegrationEventLogStorage, MongoIntegrationEventLogStorage>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var settings = MongoClientSettings.FromConnectionString(GetConnectionString(config)) ?? throw new InvalidOperationException("invalid connection string");

            settings.RetryReads = true;
            settings.RetryWrites = true;
            settings.ReadConcern = ReadConcern.Majority;
            settings.WriteConcern = WriteConcern.WMajority;
            settings.ReadPreference = ReadPreference.Primary;
            settings.LinqProvider = MongoDB.Driver.Linq.LinqProvider.V3;
#if DEBUG && !NO_COMMAND_OUTPUT
            settings.ClusterConfigurator = cb => {
                cb.Subscribe<CommandStartedEvent>(e => {
                    Debug.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
                });
            };
#endif

            var client = new MongoClient(settings);
            return new MongoIntegrationEventLogStorage(
                client.GetDatabase(config.GetOrThrow("EventBusDispatcher:MongoDB:Database")),
                sp.GetRequiredService<ILogger<MongoIntegrationEventLogStorage>>());
        });
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        var connectionStringName = configuration.GetOrThrow("MongoDB:ConnectionStringName");
        var connectionString = configuration.GetOrThrow("ConnectionStrings:" + connectionStringName);
        return connectionString;
    }
}
