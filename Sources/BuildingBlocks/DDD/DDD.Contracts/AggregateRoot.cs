using MediatR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using System.ComponentModel;

namespace Pulsar.BuildingBlocks.DDD;

public abstract class AggregateRoot : IAggregateRoot, IEquatable<AggregateRoot>
{
    [BsonIgnore]
    private List<INotification> _notifications = new List<INotification>();
    [BsonIgnore]
    private bool _isInitializing = false;

    public AggregateRoot() { }
    public AggregateRoot(ObjectId id) : this()
    {
        Id = id;
    }

    [BsonId]
    public ObjectId Id { get; protected set; }
    public long Version { get; private set; }
    [BsonIgnore]
    public IReadOnlyCollection<INotification> DomainEvents => _notifications.AsReadOnly();
    [BsonIgnore]
    public bool IsInitializing => _isInitializing;
    [BsonIgnore]
    public bool IsTransient => Id == ObjectId.Empty;

    public void IncVersion()
    {
        Version++;
    }

    public void AddDomainEvent(INotification eventItem)
    {
        _notifications.Add(eventItem);
    }

    void ISupportInitialize.BeginInit()
    {
        _isInitializing = true;
        OnBeginInit();
    }

    public void ClearDomainEvents()
    {
        _notifications.Clear();
    }

    void ISupportInitialize.EndInit()
    {
        _isInitializing = false;
        OnEndInit();
    }

    public void RemoveDomainEvent(INotification eventItem)
    {
        _notifications.Remove(eventItem);
    }

    protected virtual void OnBeginInit() { }
    protected virtual void OnEndInit() { }

    public override bool Equals(object? obj)
    {
        return Equals(obj as AggregateRoot);
    }

    public override int GetHashCode()
    {
        return !IsTransient ? Id.GetHashCode() : base.GetHashCode();

    }

    public bool Equals(AggregateRoot? other)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (ReferenceEquals(other, null))
            return false;

        if (GetType() != other.GetType())
            return false;

        if (other.IsTransient || IsTransient)
            return false;
        else
            return other.Id == Id;
    }

    public static bool operator ==(AggregateRoot? left, AggregateRoot? right)
    {
        if (Equals(left, null))
            return Equals(right, null) ? true : false;
        else
            return left.Equals(right);
    }

    public static bool operator !=(AggregateRoot? left, AggregateRoot? right)
    {
        return !(left == right);
    }
}
