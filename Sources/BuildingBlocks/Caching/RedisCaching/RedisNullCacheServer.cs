using Pulsar.BuildingBlocks.Caching;
using Pulsar.BuildingBlocks.Caching.Abstractions;

namespace Pulsar.BuildingBlocks.RedisCaching;

public class RedisNullCacheServer : ICacheServer
{
    public ICategory Category<TCategory>()
    {
        return new RedisNullCategory();
    }

    public ICategory Category(string categoryName)
    {
        return new RedisNullCategory();
    }

    public Task Clear(ICacheKey key)
    {
        return Task.CompletedTask;
    }

    public Task ClearMultiple(IEnumerable<ICacheKey> keys)
    {
        return Task.CompletedTask;
    }

    public Task ClearMultiple(params ICacheKey[] keys)
    {
        return Task.CompletedTask;
    }

    public async Task<TResult> Get<TResult>(ICacheKey key, Func<Task<TResult>> produceResult) where TResult : class?
    {
        return await produceResult();
    }

    public async Task<Dictionary<TKey, TResult>> GetMultiple<TKey, TResult>(IEnumerable<TKey> keys, Func<TKey, Task<TResult>> produceResult) where TKey : notnull where TResult : class?
    {
        Dictionary<TKey, Task<TResult>> dic = new Dictionary<TKey, Task<TResult>>();
        foreach (var k in keys)
        {
            dic[k] = Get(k.ToCacheKey(), () => produceResult(k));
        }
        List<Task> allTasks = new List<Task>();
        foreach (var k in dic.Keys)
        {
            allTasks.Add(dic[k]);
        }
        await Task.WhenAll(allTasks);
        Dictionary<TKey, TResult> res = new Dictionary<TKey, TResult>();
        foreach (var k in dic.Keys)
        {
            res[k] = dic[k].Result;
        }
        return res;
    }

    public async Task<Dictionary<TKey, TResult>> GetMultipleBatches<TKey, TResult>(IEnumerable<TKey> keys, Func<List<TKey>, Task<Dictionary<TKey, TResult>>> produceResult)
        where TKey : notnull
        where TResult : class?
    {
        return await produceResult(keys.ToList());
    }
}

class RedisNullCategory : ICategory
{
    public Task Clear(ICacheKey key)
    {
        return Task.CompletedTask;
    }

    public Task ClearAll()
    {
        return Task.CompletedTask;
    }

    public Task ClearMultiple(IEnumerable<ICacheKey> keys)
    {
        return Task.CompletedTask;
    }

    public Task ClearMultiple(params ICacheKey[] keys)
    {
        return Task.CompletedTask;
    }

    public async Task<TResult> Get<TResult>(ICacheKey key, Func<Task<TResult>> produceResult) where TResult : class?
    {
        return await produceResult();
    }

    public async Task<Dictionary<TKey, TResult>> GetMultiple<TKey, TResult>(IEnumerable<TKey> keys, Func<TKey, Task<TResult>> produceResult) where TKey : notnull where TResult : class?
    {
        Dictionary<TKey, Task<TResult>> dic = new Dictionary<TKey, Task<TResult>>();
        foreach (var k in keys)
        {
            dic[k] = Get(k.ToCacheKey(), () => produceResult(k));
        }
        List<Task> allTasks = new List<Task>();
        foreach (var k in dic.Keys)
        {
            allTasks.Add(dic[k]);
        }
        await Task.WhenAll(allTasks);
        Dictionary<TKey, TResult> res = new Dictionary<TKey, TResult>();
        foreach (var k in dic.Keys)
        {
            res[k] = dic[k].Result;
        }
        return res;
    }

    public async Task<Dictionary<TKey, TResult>> GetMultipleBatches<TKey, TResult>(IEnumerable<TKey> keys, Func<List<TKey>, Task<Dictionary<TKey, TResult>>> produceResult)
        where TKey : notnull
        where TResult : class?
    {
        return await produceResult(keys.ToList());
    }
}
