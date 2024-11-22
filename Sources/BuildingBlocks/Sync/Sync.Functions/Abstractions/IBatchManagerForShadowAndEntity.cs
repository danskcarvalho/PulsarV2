using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface IBatchManagerForShadowAndEntity<out TShadow> : IBatchManagerForEvent where TShadow : class, IShadow
{
    bool AppliesTo(object? currentShadow, object? previousShadow, ChangedEventKey eventKey);
    IBatchManagerForShadowAndEntity<TShadow> FromEvent(EntityChangedIE evt);
}

public interface IBatchManagerForShadowAndEntity<out TShadow, TEntity> : IBatchManagerForShadowAndEntity<TShadow> where TShadow : class, IShadow where TEntity : class, IAggregateRoot
{
}