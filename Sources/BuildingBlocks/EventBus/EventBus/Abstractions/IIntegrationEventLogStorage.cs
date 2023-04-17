namespace Pulsar.BuildingBlocks.EventBus.Abstractions;

public interface IIntegrationEventLogStorage
{
    Task<(Guid? ProducerId, int ProducerSeq, int ProducerCount)> CheckInProducer(Guid? producerId, TimeSpan producerTimeout, CancellationToken ct = default);
    Task<IEnumerable<IntegrationEventLogEntry>> RetrieveRelevantEventLogsAsync(int maxEvents, int producerSeq, int producersCount, CancellationToken ct = default);
    Task WatchRelevantEventLogsAsync(Func<IntegrationEventLogEntry, CancellationToken, Task> onNewEvent, int producerSeq, int producersCount, CancellationToken ct = default);
    Task MarkEventAsFailedAsync(Guid eventId, long version, CancellationToken ct = default);
    Task MarkEventAsFailedAsync(Guid eventId, List<IntegrationEventLogEntrySendAttempt> attempts, CancellationToken ct = default);
    Task RescheduleEventAsync(Guid eventId, List<IntegrationEventLogEntrySendAttempt> attempts, DateTime scheduledOn, CancellationToken ct = default);
    Task MarkEventAsPublishedAsync(Guid eventId, List<IntegrationEventLogEntrySendAttempt> attempts, CancellationToken ct = default);
    Task<bool> MarkEventAsInProgressAsync(Guid eventId, DateTime restorationDate, DateTime expirationDate, CancellationToken ct = default);
    Task RestoreEventAsync(Guid eventId, long version, CancellationToken ct = default);
}
