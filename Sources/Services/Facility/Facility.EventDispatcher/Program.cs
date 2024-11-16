using Pulsar.BuildingBlocks.EventBus;
using Pulsar.BuildingBlocks.EventBusAzure;
using Pulsar.BuildingBlocks.IntegrationEventLogMongo;
using Pulsar.BuildingBlocks.Sync.Services;
using Pulsar.Services.Facility.Domain.Aggregates.Estabelecimentos;
using Pulsar.Services.Facility.EventDispatcher;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddEventBus();
        services.AddMongoEventBus();
        services.AddAzureEventBus();
        services.AddHostedService<Worker>();
        services.AddSyncService(scanForAggregateRoots: typeof(Estabelecimento).Assembly);
    })
    .Build();

await host.RunAsync();
