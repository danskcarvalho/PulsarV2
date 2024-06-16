namespace Pulsar.BuildingBlocks.Caching.Abstractions;

public interface ICacheKey
{
}

public static class CacheKeyExtensions {

    public static string NoCategory(this ICacheKey cacheKey)
    {
        return $"$$nocat[{cacheKey.ToString()}]";
    }

    public static string Category(this ICacheKey cacheKey, string category)
    {
        return $"$$cat[{category}, {cacheKey.ToString()}]";
    }
}
