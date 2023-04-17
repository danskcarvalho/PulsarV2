using Microsoft.AspNetCore.Builder;
using System.Reflection;
using Pulsar.BuildingBlocks.Utils;

namespace Pulsar.BuildingBlocks.EventBus;

public static class DIExtensions
{
    public static void AddEventBus(this IServiceCollection col)
    {
        col.AddSingleton<IIntegrationEventDispatcherService, GenericIntegrationEventDispatcherService>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            return new GenericIntegrationEventDispatcherService(
                sp.GetRequiredService<IIntegrationEventLogStorage>(),
                sp.GetRequiredService<ILogger<GenericIntegrationEventDispatcherService>>(),
                sp.GetRequiredService<IEventBus>(),
                new GenericIntegrationEventDispatcherServiceOptions()
                {
                    MaxConsumers = IsPositive(int.Parse(config.GetOrThrow("EventBusDispatcher:MaxConsumers")), "MaxConsumers"),
                    PollingTimeout = IsPositive(int.Parse(config.GetOrThrow("EventBusDispatcher:PollingTimeout")), "PollingTimeout")
                });

        });
    }

    private static int IsPositive(int num, string name)
    {
        if (num <= 0)
            throw new ArgumentOutOfRangeException($"{name} must be positive");

        return num;
    }
}
