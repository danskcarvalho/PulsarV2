namespace Pulsar.BuildingBlocks.EventBus.Abstractions;

public class IntegrationEventLogEntry
{
    [BsonId]
    public Guid Id { get; private set; }
    public string EventName { get; private set; }
    [BsonIgnore]
    public IntegrationEvent IntegrationEvent { get; private set; }

    public string SerializedEvent { get; private set; }
    public IntegrationEventStatus Status { get; set; }
    public DateTime? InProgressExpirationDate { get; set; }
    public DateTime? InProgressRestore { get; set; }
    public DateTime? ScheduledOn { get; set; }
    public bool NoRetryOnFailure { get; set; }
    public DateTime CreatedOn { get; private set; }
    public long Version { get; private set; }
    public List<IntegrationEventLogEntrySendAttempt> Attempts { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [BsonConstructor]
    private IntegrationEventLogEntry(string eventName, string serializedEvent)
    {
        SerializedEvent = serializedEvent;
        EventName = eventName;
        var eventType = EventNameAttribute.GetTypeByEventName(eventName);
        if (eventType is null)
            throw new InvalidOperationException("invalid eventName");
        IntegrationEvent = (JsonSerializer.Deserialize(SerializedEvent, eventType, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) as IntegrationEvent)!;
        Attempts = new List<IntegrationEventLogEntrySendAttempt>();
        NoRetryOnFailure = IntegrationEvent.NoRetrySendOnFailure;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public IntegrationEventLogEntry(IntegrationEvent @event)
    {
        Id = @event.Id;
        CreatedOn = @event.CreationDate.ToUniversalTime();
        var en = EventNameAttribute.GetEventName(@event.GetType());
        if (en is null)
            throw new InvalidOperationException("invalid event: no EventNameAttribute");
        EventName = en;
        IntegrationEvent = @event;
        SerializedEvent = JsonSerializer.Serialize(@event, @event.GetType(), new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Status = IntegrationEventStatus.Pending;
        Attempts = new List<IntegrationEventLogEntrySendAttempt>();
        NoRetryOnFailure = @event.NoRetrySendOnFailure;
        ScheduledOn = DateTime.UtcNow;
        Version = 1;
    }

    public bool MayNeedProcessing()
    {
        return (Status == IntegrationEventStatus.Pending && (ScheduledOn == null || DateTime.UtcNow > ScheduledOn)) ||
               (Status == IntegrationEventStatus.InProgress && (DateTime.UtcNow > InProgressExpirationDate || DateTime.UtcNow > InProgressRestore));
    }
}

public class IntegrationEventLogEntrySendAttempt
{
    public DateTime AttemptedOn { get; private set; }
    public string? Exception { get; private set; }
    public string? ExceptionTypeName { get; private set; }
    [BsonIgnore]
    public string? ExceptionTypeShortName => ExceptionTypeName?.Split('.').Last();

    [BsonConstructor]
    public IntegrationEventLogEntrySendAttempt(DateTime attemptedOn, string? exception = null, string? exceptionTypeName = null)
    {
        AttemptedOn = attemptedOn;
        Exception = exception;
        ExceptionTypeName = exceptionTypeName;
    }
}

public enum IntegrationEventStatus
{
    Pending = 0,
    InProgress = 1,
    Published = 2,
    Failed = 3
}
