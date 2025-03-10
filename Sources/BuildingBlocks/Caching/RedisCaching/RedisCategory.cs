﻿using Microsoft.Extensions.Logging;
using Pulsar.BuildingBlocks.Caching;
using Pulsar.BuildingBlocks.Caching.Abstractions;
using StackExchange.Redis;
using System.Data.Common;
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
        var val = (string?)await TryGetFromCache(key);
        if (val == null)
        {
            _logger.LogInformation($"cache MISS for {key}");
            var result = await produceResult();
            await TryCache(key, result);
            return result;
        }
        else
        {
            var cached = FromJson<TResult>(val, out var failed);
            if (failed)
            {
                _logger.LogInformation($"cache EXPIRED or FAILED for {key}");
                var result = await produceResult();
                await TryCache(key, result);
                return result;
            }
            else
            {
                _logger.LogInformation($"cache HIT for {key}");
                return cached!;
            }
        }
    }

    private async Task<RedisValue> TryGetFromCache(ICacheKey key)
    {
        try
        {
            return await _database.StringGetAsync(key.Category(_categoryName));
        }
        catch
        {
            return RedisValue.Null;
        }
    }

    private async Task TryCache<TResult>(ICacheKey key, TResult result) where TResult : class?
    {
        var js = ToJson(result);
        try
        {
            if (js != null)
            {
                await _database.StringSetAsync(key.Category(_categoryName), js);
                await _database.KeyExpireAsync(key.Category(_categoryName), DateTime.UtcNow.AddHours(3), flags: CommandFlags.FireAndForget);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"error caching to redis");
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
            tasks.Add(TryGetFromCache(kc));
        }

        await Task.WhenAll(tasks);

        for (int i = 0; i < keysArray.Length; i++)
        {
            var val = (string?)tasks[i].Result;
            if (val != null)
            {
                var cached = FromJson<TResult>(val, out var failed);
                if (!failed)
                    dic[keysArray[i]] = cached!;
                else
                    missing.Add(keysArray[i]);
            }
            else
                missing.Add(keysArray[i]);
        }

        var results = await produceResult(missing);
        var storeTasks = new List<Task>();
        foreach (var k in results.Keys)
        {
            dic[k] = results[k];
            var kc = k.ToCacheKey();
            storeTasks.Add(TryCache(kc, results[k]));
        }

        await Task.WhenAll(storeTasks);
        return dic;
    }

    public async Task Clear(ICacheKey key)
    {
        try
        {
            await _database.KeyDeleteAsync(key.Category(_categoryName), CommandFlags.FireAndForget);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"error clearing key {key}");
        }
    }

    public async Task ClearMultiple(IEnumerable<ICacheKey> keys)
    {
        try
        {
            List<Task> allTasks = new List<Task>();
            foreach (var key in keys)
            {
                allTasks.Add(_database.KeyDeleteAsync(key.Category(_categoryName), CommandFlags.FireAndForget));
            }
            await Task.WhenAll(allTasks);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"error clearing multiple keys");
        }
    }

    public Task ClearMultiple(params ICacheKey[] keys) => ClearMultiple((IEnumerable<ICacheKey>)keys);
    private T? FromJson<T>(string val, out bool failed) where T : class?
    {
        failed = false;
        try
        {
            return (T)JsonSerializer.Deserialize(val, typeof(T), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"error deserializing json from redis cache {typeof(T).FullName}");
            failed = true;
            return default(T);
        }
    }

    private string? ToJson<T>(T? result) where T : class?
    {
        try
        {
            return JsonSerializer.Serialize(result, typeof(T), new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"error serializing to json {typeof(T).FullName}");
            return null;
        }
    }
}
