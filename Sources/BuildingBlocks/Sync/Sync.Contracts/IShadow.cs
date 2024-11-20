using Pulsar.BuildingBlocks.DDD.Abstractions;

namespace Pulsar.BuildingBlocks.Sync.Contracts;

public interface IShadow : IAggregateRoot
{
    DateTime TimeStamp { get; set; }
    void CopyId(IAggregateRoot root);
}