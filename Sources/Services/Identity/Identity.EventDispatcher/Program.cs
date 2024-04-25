using Pulsar.BuildingBlocks.EventBus;
using Pulsar.BuildingBlocks.EventBusAzure;
using Pulsar.BuildingBlocks.IntegrationEventLogMongo;
using Pulsar.Services.Identity.EventDispatcher;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddEventBus();
        services.AddMongoEventBus();
        services.AddAzureEventBus();
        services.AddHostedService<Worker>();
        services.AddSyncService();
    })
    .Build();

await host.RunAsync();
