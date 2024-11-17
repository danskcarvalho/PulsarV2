using Microsoft.Extensions.DependencyInjection;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Entities;
using Pulsar.BuildingBlocks.Sync.Functions.Implementations;

namespace Pulsar.BuildingBlocks.Sync.Functions;

public static class DIExtensions
{
    public static IServiceCollection AddSyncFunctionServices(this IServiceCollection col, params Type[] shadowTypes)
    {
        col.AddTransient<IBatchActivity, BatchActivity>();
        col.AddSingleton<IBatchManagerFactory, BatchManagerFactory>(_ => new BatchManagerFactory(shadowTypes));
		col.AddSingleton<ISyncBatchRepository, SyncBatchMongoRepository>();
		col.AddTransient<ISyncActivity, SyncActivity>();
        col.AddTransient(typeof(ISyncOrchestrator<>), typeof(SyncOrchestrator<>));
        col.AddTransient(typeof(ISyncOrchestratorStarter<>), typeof(SyncOrchestratorStarter<>));
        col.AddTransient<ISyncDbContextFactory, SyncDbContextFactory>();

        return col;
    }
}
