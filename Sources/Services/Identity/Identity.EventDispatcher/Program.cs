using Pulsar.BuildingBlocks.EventBus;
using Pulsar.BuildingBlocks.EventBusAzure;
using Pulsar.BuildingBlocks.IntegrationEventLogMongo;
using Pulsar.BuildingBlocks.Sync.Services;
using Pulsar.Services.Identity.Domain.Aggregates.Dominios;
using Pulsar.Services.Identity.EventDispatcher;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddEventBus();
        services.AddMongoEventBus();
        services.AddAzureEventBus();
        services.AddHostedService<Worker>();
        services.AddSyncService(scanForAggregateRoots: typeof(Dominio).Assembly);
    })
    .Build();

await host.RunAsync();
