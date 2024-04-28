using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using System.ComponentModel;

namespace Pulsar.BuildingBlocks.DDD;

public abstract class ValueObject : IValueObject, IEquatable<ValueObject>
{
    [BsonIgnore]
    private bool _isInitializing = false;

    public ValueObject() { }

    public bool IsInitializing => _isInitializing;

    void ISupportInitialize.BeginInit()
    {
        _isInitializing = true;
        OnBeginInit();
    }

    void ISupportInitialize.EndInit()
    {
        _isInitializing = false;
        OnEndInit();
    }

    protected virtual void OnBeginInit() { }
    protected virtual void OnEndInit() { }
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    public bool Equals(ValueObject? other)
    {
        if (other == null || other.GetType() != GetType())
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ValueObject);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (Equals(left, null))
            return Equals(right, null) ? true : false;
        else
            return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
