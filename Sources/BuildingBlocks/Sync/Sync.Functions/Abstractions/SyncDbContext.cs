using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Entities;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public record SyncDbContext(ISyncBatchRepository SyncBatchRepository);
public record SyncDbContext<TEntity>(ISyncBatchRepository SyncBatchRepository, IRepositoryBase<TEntity> EntityRepository) : SyncDbContext(SyncBatchRepository) where TEntity : class, IAggregateRoot;
