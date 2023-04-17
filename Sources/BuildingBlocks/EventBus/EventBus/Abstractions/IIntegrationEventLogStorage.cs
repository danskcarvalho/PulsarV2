namespace Pulsar.BuildingBlocks.EventBus.Abstractions;

public interface IIntegrationEventLogStorage
{
    Task<(Guid? ProducerId, int ProducerSeq, int ProducerCount)> CheckInProducer(Guid? producerId, TimeSpan producerTimeout, CancellationToken ct = default);
    Task CheckOutProducer(Guid producerId, CancellationToken ct = default);
    Task<IEnumerable<IntegrationEventLogEntry>> RetrieveRelevantEventLogsAsync(int maxEvents, int producerSeq, int producersCount, CancellationToken ct = default);
    Task WatchRelevantEventLogsAsync(Func<IntegrationEventLogEntry, CancellationToken, Task> onNewEvent, int producerSeq, int producersCount, CancellationToken ct = default);
    Task MarkEventsAsFailedAsync(List<(Guid EventId, long Version)> events, CancellationToken ct = default);
    Task MarkEventsAsFailedAsync(List<(Guid EventId, List<IntegrationEventLogEntrySendAttempt> Attempts)> events, CancellationToken ct = default);
    Task RescheduleEventsAsync(List<(Guid EventId, List<IntegrationEventLogEntrySendAttempt> Attempts, DateTime ScheduledOn)> events, CancellationToken ct = default);
    Task MarkEventsAsPublishedAsync(List<(Guid EventId, List<IntegrationEventLogEntrySendAttempt> Attempts)> events, CancellationToken ct = default);
    Task<HashSet<Guid>> MarkEventsAsInProgressAsync(List<Guid> eventIds, DateTime restorationDate, DateTime expirationDate, CancellationToken ct = default);
    Task RestoreEventsAsync(List<(Guid EventId, long Version)> events, CancellationToken ct = default);
}
