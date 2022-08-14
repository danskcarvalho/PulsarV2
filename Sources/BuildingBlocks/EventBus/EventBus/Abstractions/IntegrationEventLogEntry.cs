namespace Pulsar.BuildingBlocks.EventBus.Abstractions;

public class IntegrationEventLogEntry
{
    [BsonId]
    public Guid Id { get; private set; }
    public string EventTypeName { get; private set; }
    [BsonIgnore]
    public string EventTypeShortName => EventTypeName.Split('.').Last();
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
    private IntegrationEventLogEntry(string eventTypeName, string serializedEvent)
    {
        SerializedEvent = serializedEvent;
        EventTypeName = eventTypeName;
        IntegrationEvent = (JsonSerializer.Deserialize(SerializedEvent, GetTypeByName(EventTypeName)!, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) as IntegrationEvent)!;
        Attempts = new List<IntegrationEventLogEntrySendAttempt>();
        NoRetryOnFailure = IntegrationEvent.NoRetryOnFailure;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public IntegrationEventLogEntry(IntegrationEvent @event)
    {
        Id = @event.Id;
        CreatedOn = @event.CreationDate.ToUniversalTime();
        EventTypeName = @event.GetType().FullName!;
        IntegrationEvent = @event;
        SerializedEvent = JsonSerializer.Serialize(@event, @event.GetType(), new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Status = IntegrationEventStatus.Pending;
        Attempts = new List<IntegrationEventLogEntrySendAttempt>();
        NoRetryOnFailure = @event.NoRetryOnFailure;
        ScheduledOn = DateTime.UtcNow;
        Version = 1;
    }

    private static Dictionary<string, Type> _typesByName = new Dictionary<string, Type>();
    private static SpinLock _typesByNameLock = new SpinLock(false);
    private static Type? GetTypeByName(string name)
    {
        var gotLock = false;
        try
        {
            _typesByNameLock.Enter(ref gotLock);
            if(_typesByName.ContainsKey(name))
                return _typesByName[name];
        }
        finally
        {
            // Only give up the lock if you actually acquired it
            if (gotLock)
                _typesByNameLock.Exit();
        }

        var ty = SlowPathGetTypeByName(name);

        if (ty is not null)
        {
            gotLock = false;
            try
            {
                _typesByNameLock.Enter(ref gotLock);
                _typesByName[name] = ty;
            }
            finally
            {
                // Only give up the lock if you actually acquired it
                if (gotLock)
                    _typesByNameLock.Exit();
            }
        }

        return ty;
    }
    private static Type? SlowPathGetTypeByName(string name)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var tt = assembly.GetType(name);
            if (tt != null)
            {
                return tt;
            }
        }

        return null;
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
