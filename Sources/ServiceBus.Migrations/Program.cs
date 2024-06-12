using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pulsar.Services.Identity.Functions.Application.Synchronizations;
using ServiceBus.Migrations.Core;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<Migrator>();
    })
    .Build();

var migrator = host.Services.GetRequiredService<Migrator>();

await migrator
    .AddAssembly(typeof(SyncFacilityFN).Assembly)
    .EnsureTopicCreation("%ServiceBusDeveloper%.Identity")
    .Migrate();