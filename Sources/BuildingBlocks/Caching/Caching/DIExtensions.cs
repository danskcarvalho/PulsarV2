namespace Pulsar.BuildingBlocks.Caching;

public static class DIExtensions 
{
    public static void AddInMemoryScopedCache(this IServiceCollection collection)
    {
        collection.AddScoped<InMemoryScopedCache>();
        collection.AddScoped<IScopedCache, InMemoryScopedCache>(sp => sp.GetRequiredService<InMemoryScopedCache>());
    }
}
