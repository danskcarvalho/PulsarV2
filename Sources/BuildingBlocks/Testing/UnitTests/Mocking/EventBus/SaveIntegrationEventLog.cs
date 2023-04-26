using Pulsar.BuildingBlocks.EventBus.Abstractions;
using Pulsar.BuildingBlocks.EventBus.Events;
using System.Collections.ObjectModel;

namespace Pulsar.BuildingBlocks.UnitTests.Mocking.EventBus;

public class SaveIntegrationEventLog : ISaveIntegrationEventLog
{
    private readonly List<IntegrationEvent> _events = new List<IntegrationEvent>();
    private readonly ReadOnlyCollection<IntegrationEvent> _roList;

    public SaveIntegrationEventLog()
    {
        _roList = _events.AsReadOnly();
    }

    public IReadOnlyList<IntegrationEvent> Events => _roList;

    public Task SaveEventAsync(IntegrationEvent @event, CancellationToken ct = default)
    {
        _events.Add(@event);
        return Task.CompletedTask;
    }
}
