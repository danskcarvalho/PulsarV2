namespace Pulsar.BuildingBlocks.Caching.Abstractions;

public class CachedArrayKey<T> : IEnumerable<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IEquatable<CachedArrayKey<T>>, ICacheKey
{
    private readonly T[] _array;

    public CachedArrayKey(IEnumerable<T> values)
    {
        _array = values.ToArray();
    }

    public T this[int index] => _array[index];

    public int Count => _array.Length;

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _array.Length; i++)
        {
            yield return _array[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("[");

        for (int i = 0; i < _array.Length; i++)
        {
            if (i != 0)
                builder.Append(", ");

            if (_array[i] == null)
                builder.Append("null");
            else if (_array[i] is ICacheKey)
                builder.Append(_array[i]!.ToString());
            else
                builder.Append($"<{_array[i]!.ToString()}>");
        }

        builder.Append("]");

        return builder.ToString();
    }

    private int? hashCode = null;
    public override int GetHashCode()
    {
        if (hashCode is not null)
            return hashCode.Value;

        HashCode hs = new HashCode();
        hs.Add(_array.Length);
        for (int i = 0; i < _array.Length; i++)
        {
            hs.Add(_array[i]);
        }
        hashCode = hs.ToHashCode();
        return hashCode.Value;
    }

    public bool Equals(CachedArrayKey<T>? other)
    {
        if (object.ReferenceEquals(other, this))
            return true;

        if (object.ReferenceEquals(other, null))
            return false;

        if (other._array.Length != this._array.Length)
            return false;

        for (int i = 0; i < _array.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(_array[i], other._array[i]))
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as CachedArrayKey<T>);
    }

    public static bool operator==(CachedArrayKey<T> t1, CachedArrayKey<T> t2)
    {
        if (object.ReferenceEquals(t1, t2))
            return true;

        if (object.ReferenceEquals(t1, null))
            return false;

        return t1.Equals(t2);
    }

    public static bool operator !=(CachedArrayKey<T> t1, CachedArrayKey<T> t2)
    {
        if (object.ReferenceEquals(t1, t2))
            return false;

        if (object.ReferenceEquals(t1, null))
            return true;

        return !t1.Equals(t2);
    }
}
