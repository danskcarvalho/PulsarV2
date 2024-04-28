using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Entities;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface IBatchDbContextFactory
{
    Task<TResult> Execute<TEntity, TResult>(Func<ISyncBatchRepository, IRepositoryBase<TEntity>, Task<TResult>> createFn) where TEntity : class, IAggregateRoot;

    Task<TResult> Execute<TResult>(
        Func<ISyncBatchRepository, Task<TResult>> createFn);
}