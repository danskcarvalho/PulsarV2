using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceBus.Migrations.Core;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<Migrator>();
    })
    .Build();

var migrator = host.Services.GetRequiredService<Migrator>();

await migrator
    .AddAssembly(typeof(Pulsar.Services.Identity.Functions.Application.Synchronizations.SyncFacilityFN).Assembly)
	.AddAssembly(typeof(Pulsar.Services.Facility.Functions.Application.Synchronizations.SyncIdentityFN).Assembly)
	.EnsureTopicCreation("%ServiceBusDeveloper%.Identity")
	.EnsureTopicCreation("%ServiceBusDeveloper%.Facility")
	.Migrate();