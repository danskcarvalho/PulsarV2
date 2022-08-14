namespace Pulsar.BuildingBlocks.EventBus.Abstractions;

public interface IIntegrationEventLogStorage
{
    Task<IEnumerable<IntegrationEventLogEntry>> RetrieveRelevantEventLogsAsync(int maxEvents, CancellationToken ct);
    Task WatchRelevantEventLogsAsync(Func<IntegrationEventLogEntry, CancellationToken, Task> onNewEvent, CancellationToken ct);
    Task MarkEventAsFailedAsync(Guid eventId, long version, CancellationToken ct);
    Task MarkEventAsFailedAsync(Guid eventId, List<IntegrationEventLogEntrySendAttempt> attempts, CancellationToken ct);
    Task RescheduleEventAsync(Guid eventId, List<IntegrationEventLogEntrySendAttempt> attempts, DateTime scheduledOn, CancellationToken ct);
    Task MarkEventAsPublishedAsync(Guid eventId, List<IntegrationEventLogEntrySendAttempt> attempts, CancellationToken ct);
    Task<bool> MarkEventAsInProgressAsync(Guid eventId, DateTime restorationDate, DateTime expirationDate, CancellationToken ct);
    Task RestoreEventAsync(Guid eventId, long version, CancellationToken ct);
}
