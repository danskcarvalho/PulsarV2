using Microsoft.Extensions.DependencyInjection;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Implementations;

namespace Pulsar.BuildingBlocks.Sync.Functions;

public static class DIExtensions
{
    public static IServiceCollection AddSyncFunctionServices(IServiceCollection col)
    {
        col.AddTransient<IBatchActivity, BatchActivity>();
        col.AddSingleton<IBatchManagerFactory, BatchManagerFactory>();
        col.AddTransient<ISyncActivity, SyncActivity>();
        col.AddTransient(typeof(ISyncOrchestrator<>), typeof(SyncOrchestrator<>));
        col.AddTransient<ISyncDbContextFactory, SyncDbContextFactory>();

        return col;
    }
}
