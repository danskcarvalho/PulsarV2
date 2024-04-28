using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface IBatchManagerFactory
{
    IEnumerable<IBatchManagerForEvent> GetManagersFromEvent(EntityChangedIE @event, object? originalShadow);
}