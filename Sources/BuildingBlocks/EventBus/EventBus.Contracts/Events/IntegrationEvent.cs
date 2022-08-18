namespace Pulsar.BuildingBlocks.EventBus.Events;

public class IntegrationEvent
{
    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }

    public IntegrationEvent(Guid id)
    {
        Id = id;
        CreationDate = DateTime.UtcNow;
    }

    public IntegrationEvent(Guid id, bool noRetrySendOnFailure)
    {
        Id = id;
        CreationDate = DateTime.UtcNow;
        NoRetrySendOnFailure = noRetrySendOnFailure;
    }

    public IntegrationEvent(Guid id, DateTime creationDate)
    {
        Id = id;
        CreationDate = creationDate;
    }

    [JsonConstructor]
    public IntegrationEvent(Guid id, DateTime creationDate, bool noRetryOnFailure)
    {
        Id = id;
        CreationDate = creationDate;
        NoRetrySendOnFailure = noRetryOnFailure;
    }

    [JsonInclude]
    public Guid Id { get; private init; }

    [JsonInclude]
    public DateTime CreationDate { get; private init; }
    [JsonIgnore]
    public bool NoRetrySendOnFailure { get; private init; }
}
