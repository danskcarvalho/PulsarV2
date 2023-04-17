using Pulsar.BuildingBlocks.EventBus.Events;

namespace Pulsar.BuildingBlocks.EventBus.Abstractions;

public interface IEventBus : IAsyncDisposable
{
    Task<List<(HashSet<Guid> Ids, Exception? Exception)>> Publish(IEnumerable<(string EventName, IntegrationEvent Event)> eventBatch);
}
