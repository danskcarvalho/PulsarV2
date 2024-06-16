using Microsoft.Extensions.Logging;
using Pulsar.BuildingBlocks.Caching;
using Pulsar.BuildingBlocks.Caching.Abstractions;
using StackExchange.Redis;
using System.Text.Json;

namespace Pulsar.BuildingBlocks.RedisCaching;

class RedisCategory : ICategory
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheServer> _logger;
    private readonly string _categoryName;

    public RedisCategory(IDatabase database, ILogger<RedisCacheServer> logger, string categoryName)
    {
        _database = database;
        _logger = logger;
        _categoryName = "$$c:" + categoryName;
    }

    public async Task<TResult> Get<TResult>(ICacheKey key, Func<Task<TResult>> produceResult) where TResult : class?
    {
        var val = (string?)await _database.HashGetAsync(_categoryName, key.ToString());
        if (val == null)
        {
            _logger.LogInformation($"cache MISS for {key}");
            var result = await produceResult();
            await _database.HashSetAsync(_categoryName, key.ToString(), ToJson(result), flags: CommandFlags.FireAndForget);
            return result;
        }
        else
        {
            var cached = FromJson<TResult>(val);
            if (cached.Failed || DateTime.UtcNow > cached.ExpiresOn)
            {
                _logger.LogInformation($"cache EXPIRED or FAILED for {key}");
                var result = await produceResult();
                await _database.HashSetAsync(_categoryName, key.ToString(), ToJson(result), flags: CommandFlags.FireAndForget);
                return result;
            }
            else
            {
                _logger.LogInformation($"cache HIT for {key}");
                return cached.Value!;
            }
        }
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
        var keysArray = keys.ToArray();
        Dictionary<TKey, TResult> dic = new Dictionary<TKey, TResult>();
        List<TKey> missing = new List<TKey>();
        List<Task<RedisValue>> tasks = new List<Task<RedisValue>>();

        foreach (var k in keysArray)
        {
            var kc = k.ToCacheKey();
            tasks.Add(_database.HashGetAsync(_categoryName, kc.ToString()));
        }

        await Task.WhenAll(tasks);

        for (int i = 0; i < keysArray.Length; i++)
        {
            var val = (string?)tasks[i].Result;
            if (val != null)
            {
                var cached = FromJson<TResult>(val);
                if (!cached.Failed && DateTime.UtcNow <= cached.ExpiresOn)
                    dic[keysArray[i]] = cached.Value!;
                else
                    missing.Add(keysArray[i]);
            }
            else
                missing.Add(keysArray[i]);
        }

        var results = await produceResult(missing);
        var storeTasks = new List<Task<bool>>();
        foreach (var k in results.Keys)
        {
            dic[k] = results[k];
            var kc = k.ToCacheKey();
            storeTasks.Add(_database.HashSetAsync(_categoryName, kc.ToString(), ToJson(results[k]), flags: CommandFlags.FireAndForget));
        }

        await Task.WhenAll(storeTasks);
        return dic;
    }

    private CachedValue<T> FromJson<T>(string val) where T : class?
    {
        try
        {
            return (CachedValue<T>)JsonSerializer.Deserialize(val, typeof(CachedValue<T>), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "error deserializing json from redis cache");
            return new CachedValue<T>(null, true, DateTime.UtcNow);
        }
    }

    private string ToJson<T>(T? result) where T : class?
    {
        var cached = new CachedValue<T>(result, false, DateTime.UtcNow.AddHours(3));
        return JsonSerializer.Serialize(cached, typeof(CachedValue<T>), new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    public async Task Clear(ICacheKey key)
    {
        await _database.HashDeleteAsync(_categoryName, key.ToString(), CommandFlags.FireAndForget);
    }

    public async Task ClearMultiple(IEnumerable<ICacheKey> keys)
    {
        List<Task> allTasks = new List<Task>();
        foreach (var key in keys)
        {
            allTasks.Add(_database.HashDeleteAsync(_categoryName, key.ToString(), CommandFlags.FireAndForget));
        }
        await Task.WhenAll(allTasks);
    }

    public Task ClearMultiple(params ICacheKey[] keys) => ClearMultiple((IEnumerable<ICacheKey>)keys);

    public async Task ClearAll()
    {
        await _database.KeyDeleteAsync(_categoryName, CommandFlags.FireAndForget);
    }
}
