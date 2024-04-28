using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface IBatchManagerForShadowAndEntity<out TShadow> : IBatchManagerForEvent where TShadow : class
{
    bool AppliesTo(object shadow, object? previous, ChangedEventKey eventKey);
    IBatchManagerForShadowAndEntity<TShadow> GetBatchManagerForShadowAndEntity(EntityChangedIE evt);
}

public interface IBatchManagerForShadowAndEntity<out TShadow, TEntity> : IBatchManagerForShadowAndEntity<TShadow> where TShadow : class where TEntity : class, IAggregateRoot
{
}