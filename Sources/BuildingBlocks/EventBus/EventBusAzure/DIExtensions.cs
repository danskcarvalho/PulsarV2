namespace Pulsar.BuildingBlocks.EventBusAzure;

public static class DIExtensions
{
    public static void AddAzureEventBus(this IServiceCollection col)
    {
        col.AddSingleton<IEventBus>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILogger<AzureEventBusService>>();

            return new AzureEventBusService(
                logger,
                config.GetOrThrow("Azure:EventBus:Namespace"),
                config.GetOrThrow("Azure:EventBus:TopicName"),
                config.GetOrThrow("Azure:EventBus:DeveloperName"));
        });
    }
}
