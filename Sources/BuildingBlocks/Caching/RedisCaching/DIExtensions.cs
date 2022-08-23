using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pulsar.BuildingBlocks.Caching.Abstractions;
using StackExchange.Redis;

namespace Pulsar.BuildingBlocks.RedisCaching;

public static class DIExtensions
{
    public static void AddRedisCache(this IServiceCollection col)
    {
        col.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            return ConnectionMultiplexer.Connect(config["Redis:ConnectionString"]);
        });
        col.AddSingleton<ICacheServer>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var enabled = config.GetValue<bool>("Redis:Enabled", true);
            if (enabled)
                return new RedisCacheServer(sp.GetRequiredService<ConnectionMultiplexer>(), sp.GetRequiredService<ILogger<RedisCacheServer>>());
            else
                return new RedisNullCacheServer();
        });
    }
}
