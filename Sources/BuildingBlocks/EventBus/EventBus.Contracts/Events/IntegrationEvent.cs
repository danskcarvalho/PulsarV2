namespace Pulsar.BuildingBlocks.EventBus.Events;

public record IntegrationEvent
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

    public IntegrationEvent(Guid id, bool noRetryOnFailure)
    {
        Id = id;
        CreationDate = DateTime.UtcNow;
        NoRetryOnFailure = noRetryOnFailure;
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
        NoRetryOnFailure = noRetryOnFailure;
    }

    [JsonInclude]
    public Guid Id { get; private init; }

    [JsonInclude]
    public DateTime CreationDate { get; private init; }
    public bool NoRetryOnFailure { get; private init; }
}
