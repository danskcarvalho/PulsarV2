using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface IBatchManagerForShadow
{
    IEnumerable<IBatchManagerForEvent> GetManagersForEntity(Type trackedEntityType, EntityChangedIE evt, object? originalShadow);
}