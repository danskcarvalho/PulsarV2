using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Entities;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface ISyncDbContextFactory
{
    Task<TResult> Execute<TEntity, TResult>(Func<SyncDbContext<TEntity>, Task<TResult>> createFn) where TEntity : class, IAggregateRoot;

    Task<TResult> Execute<TResult>(
        Func<SyncDbContext, Task<TResult>> createFn);
}