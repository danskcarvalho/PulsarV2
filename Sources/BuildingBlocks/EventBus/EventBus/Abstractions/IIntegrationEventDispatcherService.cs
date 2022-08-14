namespace Pulsar.BuildingBlocks.EventBus.Abstractions;

public interface IIntegrationEventDispatcherService
{
    Task Run(CancellationToken ct);
}
