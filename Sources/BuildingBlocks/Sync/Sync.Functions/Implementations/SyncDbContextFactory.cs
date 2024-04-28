using Microsoft.Extensions.DependencyInjection;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Entities;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations;

public class SyncDbContextFactory(IServiceProvider serviceProvider) : ISyncDbContextFactory
{
    public Task<TResult> Execute<TResult>(Func<SyncDbContext, Task<TResult>> createFn)
    {
        return createFn(
            new SyncDbContext(serviceProvider.GetRequiredService<ISyncBatchRepository>())
            );
    }

    Task<TResult> ISyncDbContextFactory.Execute<TEntity, TResult>(Func<SyncDbContext<TEntity>, Task<TResult>> createFn)
    {
        return createFn(
            new SyncDbContext<TEntity>(
                serviceProvider.GetRequiredService<ISyncBatchRepository>(),
                serviceProvider.GetRequiredService<IRepositoryBase<TEntity>>()));
    }
}