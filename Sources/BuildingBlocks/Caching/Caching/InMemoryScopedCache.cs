using System.Text.Json;

namespace Pulsar.BuildingBlocks.Caching;

public class InMemoryScopedCache : IScopedCache
{
    private Dictionary<object, CachedObject> _cache = new Dictionary<object, CachedObject>(); 
    
    public InMemoryScopedCache() { }

    public async Task<TResult?> Cached<TKey, TResult>(TKey key, Func<Task<TResult?>> produceResult)
        where TKey : ICacheKey
        where TResult : class, ICachedResult
    {
        if (_cache.ContainsKey(key))
            return (TResult?)_cache[key].GetObject();

        var obj = await produceResult();
        if (obj == null)
            _cache[key] = new CachedObject(null, null, null);
        else if (obj.IsImmutable)
            _cache[key] = new CachedObject(obj, obj.GetType(), null);
        else
            _cache[key] = new CachedObject(null, obj.GetType(), ToJson(obj));

        return obj;
    }

    private string? ToJson<TResult>(TResult obj) where TResult : class, ICachedResult
    {
        return JsonSerializer.Serialize(obj, obj.GetType(), new JsonSerializerOptions());
    }

    private record struct CachedObject(object? Object, Type? ObjectType, string? Json)
    {
        public object? GetObject()
        {
            if (ObjectType == null)
                return null;
            else if (Object != null)
                return Object;
            else
                return JsonSerializer.Deserialize(Json!, ObjectType, new JsonSerializerOptions());
        }
    }
}
