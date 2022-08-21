namespace Pulsar.BuildingBlocks.Caching.Abstractions;

public interface ICacheServer
{
    ICategory Category<TCategory>();
    ICategory Category(string categoryName);
    Task<TResult> Get<TResult>(ICacheKey key, Func<Task<TResult>> produceResult) where TResult : class?;
    Task<Dictionary<ICacheKey, TResult>> GetMultiple<TResult>(IEnumerable<ICacheKey> keys, Func<ICacheKey, Task<TResult>> produceResult) where TResult : class?;
    Task Clear(ICacheKey key);
    Task ClearMultiple(IEnumerable<ICacheKey> keys);
    Task ClearMultiple(params ICacheKey[] keys);
}

public interface ICategory
{
    Task<TResult> Get<TResult>(ICacheKey key, Func<Task<TResult>> produceResult) where TResult : class?;
    Task<Dictionary<ICacheKey, TResult>> GetMultiple<TResult>(IEnumerable<ICacheKey> keys, Func<ICacheKey, Task<TResult>> produceResult) where TResult : class?;
    Task Clear(ICacheKey key);
    Task ClearMultiple(IEnumerable<ICacheKey> keys);
    Task ClearMultiple(params ICacheKey[] keys);
    Task ClearAll();
}
