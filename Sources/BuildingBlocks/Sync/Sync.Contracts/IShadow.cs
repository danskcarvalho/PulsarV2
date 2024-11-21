using Pulsar.BuildingBlocks.DDD.Abstractions;

namespace Pulsar.BuildingBlocks.Sync.Contracts;

public interface IShadow : IAggregateRoot
{
    void InitializeShadowFromRoot(IAggregateRoot root);
}