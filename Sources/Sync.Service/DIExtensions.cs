using Microsoft.AspNetCore.Builder;
using System.Reflection;
using Pulsar.BuildingBlocks.Utils;
using Microsoft.Extensions.DependencyInjection;
using Pulsar.BuildingBlocks.Sync.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System.Diagnostics;
using MongoDB.Bson;

namespace Pulsar.BuildingBlocks.EventBus;

public static class DIExtensions
{
    public static void AddSyncService(this IServiceCollection col, params Assembly[] scanForAggregateRoots)
    {
        col.AddSingleton<ISyncIntegrationEventDispatcher, SyncIntegrationEventDispatcher>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var settings = MongoClientSettings.FromConnectionString(config.GetOrThrow("SyncService:MongoDB:ConnectionString")) ?? throw new InvalidOperationException("invalid connection string");

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
            var database = client.GetDatabase(config.GetOrThrow("SyncService:MongoDB:Database"));

            return new SyncIntegrationEventDispatcher(
                sp.GetRequiredService<ILogger<SyncIntegrationEventDispatcher>>(),
                database,
                new MyMongoSaveIntegrationEventLog(database),
                scanForAggregateRoots,
                new SyncIntegrationEventDispatcherOptions()
                {
                    MaxConsumers = IsPositive(int.Parse(config.GetOrThrow("SyncService:MaxConsumers")), "MaxConsumers"),
                });
        });
    }

    private static int IsPositive(int num, string name)
    {
        if (num <= 0)
            throw new ArgumentOutOfRangeException($"{name} must be positive");

        return num;
    }
}
