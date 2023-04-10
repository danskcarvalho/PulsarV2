namespace Pulsar.BuildingBlocks.Caching.Abstractions;

public interface ICacheServer
{
    ICategory Category<TCategory>();
    ICategory Category(string categoryName);
    Task<TResult> Get<TResult>(ICacheKey key, Func<Task<TResult>> produceResult) where TResult : class?;
    Task<Dictionary<TKey, TResult>> GetMultiple<TKey, TResult>(IEnumerable<TKey> keys, Func<TKey, Task<TResult>> produceResult) 
        where TResult : class? 
        where TKey : notnull;
    Task<Dictionary<TKey, TResult>> GetMultipleBatches<TKey, TResult>(IEnumerable<TKey> keys, Func<List<TKey>, Task<Dictionary<TKey, TResult>>> produceResult)
        where TResult : class?
        where TKey : notnull;
    Task Clear(ICacheKey key);
    Task ClearMultiple(IEnumerable<ICacheKey> keys);
    Task ClearMultiple(params ICacheKey[] keys);
}

public interface ICategory
{
    Task<TResult> Get<TResult>(ICacheKey key, Func<Task<TResult>> produceResult) where TResult : class?;
    Task<Dictionary<TKey, TResult>> GetMultiple<TKey, TResult>(IEnumerable<TKey> keys, Func<TKey, Task<TResult>> produceResult)
        where TResult : class?
        where TKey : notnull;
    Task<Dictionary<TKey, TResult>> GetMultipleBatches<TKey, TResult>(IEnumerable<TKey> keys, Func<List<TKey>, Task<Dictionary<TKey, TResult>>> produceResult)
        where TResult : class?
        where TKey : notnull;
    Task Clear(ICacheKey key);
    Task ClearMultiple(IEnumerable<ICacheKey> keys);
    Task ClearMultiple(params ICacheKey[] keys);
    Task ClearAll();
}
