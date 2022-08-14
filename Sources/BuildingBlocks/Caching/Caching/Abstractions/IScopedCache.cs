namespace Pulsar.BuildingBlocks.Caching.Abstractions;

public interface IScopedCache
{
    Task<TResult?> Cached<TKey, TResult>(TKey key, Func<Task<TResult?>> produceResult) 
        where TKey : ICacheKey
        where TResult : class, ICachedResult;
}
