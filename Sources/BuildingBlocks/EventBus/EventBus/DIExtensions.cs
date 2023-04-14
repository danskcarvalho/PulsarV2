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
        col.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
    }

    public static void UseEventBus(this IApplicationBuilder app, Assembly assembly)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        var handlers = assembly.GetTypes().Where(t =>
        {
            return t.IsClass && !t.IsGenericTypeDefinition && t.IsPublic && !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));
        });

        var mapping = eventBus.GetType().GetInterfaceMap(typeof(IEventBus));
        var method = mapping.TargetMethods[mapping.InterfaceMethods.Select((m, i) => (m, i)).Where(t => t.m.Name == "Subscribe").Select(t => t.i).First()];

        foreach (var h in handlers)
        {
            var ie = h.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)).GetGenericArguments()[0];
            method.MakeGenericMethod(ie, h).Invoke(eventBus, null);
        }
    }

    private static int IsPositive(int num, string name)
    {
        if (num <= 0)
            throw new ArgumentOutOfRangeException($"{name} must be positive");

        return num;
    }
}
