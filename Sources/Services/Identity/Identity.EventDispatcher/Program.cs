using Pulsar.BuildingBlocks.EventBus;
using Pulsar.BuildingBlocks.EventBusRabbitMQ;
using Pulsar.BuildingBlocks.IntegrationEventLogMongo;
using Pulsar.Services.Identity.EventDispatcher;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddEventBus();
        services.AddMongoEventBus();
        services.AddRabbitMQ();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
