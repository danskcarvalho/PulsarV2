using Pulsar.BuildingBlocks.EventBus.Events;

namespace Pulsar.BuildingBlocks.EventBus.Abstractions;

public interface IEventBus
{
    Task Publish(IntegrationEvent @event);
}
