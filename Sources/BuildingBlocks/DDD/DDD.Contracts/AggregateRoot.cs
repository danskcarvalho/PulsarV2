using System.ComponentModel;
using MediatR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD.Abstractions;

namespace DDD.Contracts;

public abstract class AggregateRoot : IAggregateRoot, IEquatable<AggregateRoot>
{
    [BsonIgnore]
    private List<INotification> _notifications = new List<INotification>();
    [BsonIgnore]
    private bool _isInitializing;

    public AggregateRoot() { }
    public AggregateRoot(ObjectId id) : this()
    {
        Id = id;
    }

    [BsonId]
    public ObjectId Id { get; protected set; }
    public long Version { get; private set; }
    [BsonIgnore]
    public IReadOnlyCollection<INotification> DomainEvents
    {
        get
        {
            if (_notifications == null)
            {
                _notifications = new List<INotification>();
            }
            return _notifications.AsReadOnly();
        }
    }
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
		if (_notifications == null)
		{
			_notifications = new List<INotification>();
		}
		_notifications.Add(eventItem);
    }

    void ISupportInitialize.BeginInit()
    {
        _isInitializing = true;
        OnBeginInit();
    }

    public void ClearDomainEvents()
    {
		if (_notifications == null)
		{
			_notifications = new List<INotification>();
		}
		_notifications.Clear();
    }

    void ISupportInitialize.EndInit()
    {
        _isInitializing = false;
        OnEndInit();
    }

    public void RemoveDomainEvent(INotification eventItem)
    {
		if (_notifications == null)
		{
			_notifications = new List<INotification>();
		}
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

	public void CopyVersionFrom(IAggregateRoot anotherRoot)
	{
		this.Version = anotherRoot.Version;
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
