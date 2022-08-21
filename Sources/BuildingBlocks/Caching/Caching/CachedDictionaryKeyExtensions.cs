namespace Pulsar.BuildingBlocks.Caching;

public static class CachedDictionaryKeyExtensions
{
    public static ICacheKey ToCacheKey(this object? obj)
    {
        if (obj == null)
            return new CachedPrimitiveValue(null);
        else if (obj is ICacheKey ck)
            return ck;
        else if (obj is string)
            return new CachedPrimitiveValue(obj);
        else if (obj.GetType().IsPrimitive)
            return new CachedPrimitiveValue(obj);
        else if (obj is IEnumerable en)
            return new CachedArrayKey<object?>(en.Cast<object>().Select(x => ToCacheKey(x)));
        else
        {
            Dictionary<string, object?> dict = new Dictionary<string, object?>();
            foreach (var prop in obj.GetType().GetProperties())
            {
                var pname = prop.Name;
                var pvalue = prop.GetValue(obj);
                dict[pname] = ToCacheKey(pvalue);
            }
            return new CachedDictionaryKey<string, object?>(dict);
        }
    }
}
