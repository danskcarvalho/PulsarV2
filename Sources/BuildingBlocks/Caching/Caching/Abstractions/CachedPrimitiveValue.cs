namespace Pulsar.BuildingBlocks.Caching.Abstractions;

public class CachedPrimitiveValue : ICacheKey, IEquatable<CachedPrimitiveValue>
{
    public object? Value { get; }

    public CachedPrimitiveValue(object? value)
    {
        Value = value;
    }

    public bool Equals(CachedPrimitiveValue? other)
    {
        if (object.ReferenceEquals(other, null))
            return false;

        if (object.ReferenceEquals(other.Value, this.Value))
            return true;

        if (object.ReferenceEquals(other.Value, null))
            return false;

        if (object.ReferenceEquals(this.Value, null))
            return false;

        return this.Value.Equals(other);
    }

    public override int GetHashCode()
    {
        return this.Value?.GetHashCode() ?? 0;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as CachedPrimitiveValue);
    }

    public override string ToString()
    {
        return this.Value != null ? $"<{this.Value}>" : "null";
    }

    public static bool operator==(CachedPrimitiveValue v1, CachedPrimitiveValue v2)
    {
        if (object.ReferenceEquals(v1, v2))
            return true;
        if (object.ReferenceEquals(v1, null))
            return false;
        return v1.Equals(v2);
    }
    public static bool operator !=(CachedPrimitiveValue v1, CachedPrimitiveValue v2)
    {
        if (object.ReferenceEquals(v1, v2))
            return false;
        if (object.ReferenceEquals(v1, null))
            return true;
        return !v1.Equals(v2);
    }
}
