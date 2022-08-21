namespace Pulsar.BuildingBlocks.Caching.Abstractions;

public class CachedDictionaryKey<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IEquatable<CachedDictionaryKey<TKey, TValue>>, ICacheKey
    where TKey : notnull
{
    private SortedDictionary<TKey, TValue> _dictionary;

    public CachedDictionaryKey(IReadOnlyDictionary<TKey, TValue> original)
    {
        _dictionary = new SortedDictionary<TKey, TValue>();
        foreach (var item in original)
        {
            _dictionary.Add(item.Key, item.Value);
        }
    }

    public TValue this[TKey key] => _dictionary[key];

    public IEnumerable<TKey> Keys => _dictionary.Keys;

    public IEnumerable<TValue> Values => _dictionary.Values;

    public int Count => _dictionary.Count;

    public bool ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    public bool Equals(CachedDictionaryKey<TKey, TValue>? other)
    {
        if (object.ReferenceEquals(other, this))
            return true;

        if (object.ReferenceEquals(other, null))
            return false;

        if (other._dictionary.Count != this._dictionary.Count)
            return false;

        foreach (var key in Keys)
        {
            if (!other._dictionary.ContainsKey(key))
                return false;

            if (!EqualityComparer<TValue>.Default.Equals(_dictionary[key], other._dictionary[key]))
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as CachedDictionaryKey<TKey, TValue>);
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("{");

        bool first = true;

        foreach (var key in Keys)
        {
            if (!first)
                builder.Append(", ");

            first = false;
            builder.Append(key.ToString());
            builder.Append(": ");

            if (_dictionary[key] == null)
                builder.Append("null");
            else if (_dictionary[key] is ICacheKey)
                builder.Append(_dictionary[key]!.ToString());
            else
                builder.Append($"<{_dictionary[key]!.ToString()}>");
        }

        builder.Append("}");

        return builder.ToString();
    }

    private int? hashCode = null;
    public override int GetHashCode()
    {
        if (hashCode is not null)
            return hashCode.Value;

        HashCode hs = new HashCode();
        hs.Add(_dictionary.Count);
        foreach (var key in Keys)
        {
            hs.Add(key);
            hs.Add(_dictionary[key]);
        }
        hashCode = hs.ToHashCode();
        return hashCode.Value;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
