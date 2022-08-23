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

    public async Task<Dictionary<ICacheKey, TResult>> GetMultiple<TResult>(IEnumerable<ICacheKey> keys, Func<ICacheKey, Task<TResult>> produceResult) where TResult : class?
    {
        Dictionary<ICacheKey, Task<TResult>> dic = new Dictionary<ICacheKey, Task<TResult>>();
        foreach (var k in keys)
        {
            dic[k] = Get(k, () => produceResult(k));
        }
        List<Task> allTasks = new List<Task>();
        foreach (var k in dic.Keys)
        {
            allTasks.Add(dic[k]);
        }
        await Task.WhenAll(allTasks);
        Dictionary<ICacheKey, TResult> res = new Dictionary<ICacheKey, TResult>();
        foreach (var k in dic.Keys)
        {
            res[k] = dic[k].Result;
        }
        return res;
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

    public async Task<Dictionary<ICacheKey, TResult>> GetMultiple<TResult>(IEnumerable<ICacheKey> keys, Func<ICacheKey, Task<TResult>> produceResult) where TResult : class?
    {
        Dictionary<ICacheKey, Task<TResult>> dic = new Dictionary<ICacheKey, Task<TResult>>();
        foreach (var k in keys)
        {
            dic[k] = Get(k, () => produceResult(k));
        }
        List<Task> allTasks = new List<Task>();
        foreach (var k in dic.Keys)
        {
            allTasks.Add(dic[k]);
        }
        await Task.WhenAll(allTasks);
        Dictionary<ICacheKey, TResult> res = new Dictionary<ICacheKey, TResult>();
        foreach (var k in dic.Keys)
        {
            res[k] = dic[k].Result;
        }
        return res;
    }
}
