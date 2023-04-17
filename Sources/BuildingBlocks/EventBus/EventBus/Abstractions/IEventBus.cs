using Pulsar.BuildingBlocks.EventBus.Events;

namespace Pulsar.BuildingBlocks.EventBus.Abstractions;

public interface IEventBus
{
    Task<List<(HashSet<Guid> Ids, Exception? Exception)>> Publish(IEnumerable<IntegrationEvent> eventBatch);
}
