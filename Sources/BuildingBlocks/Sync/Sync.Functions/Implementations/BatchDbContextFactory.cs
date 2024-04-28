using Microsoft.Extensions.DependencyInjection;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Entities;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations;

public class BatchDbContextFactory(IServiceProvider serviceProvider) : IBatchDbContextFactory
{
    public Task<TResult> Execute<TEntity, TResult>(Func<ISyncBatchRepository, IRepositoryBase<TEntity>, Task<TResult>> createFn) where TEntity : class, IAggregateRoot
    {
        return createFn(
            serviceProvider.GetRequiredService<ISyncBatchRepository>(),
            serviceProvider.GetRequiredService<IRepositoryBase<TEntity>>());
    }

    public Task<TResult> Execute<TResult>(Func<ISyncBatchRepository, Task<TResult>> createFn)
    {
        return createFn(
            serviceProvider.GetRequiredService<ISyncBatchRepository>()
            );
    }
}