namespace Pulsar.BuildingBlocks.EventBus.Abstractions;

public interface ISaveIntegrationEventLog
{
    Task SaveEventAsync(IntegrationEvent @event, CancellationToken ct = default);
}
