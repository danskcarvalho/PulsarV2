using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// DATABASES
var mongo = builder
    .AddMongoDB("pulsar-mongodb", port: int.Parse(builder.Configuration["MongoDB:Port"]!))
    .WithArgs("--replSet", "rs0", "--bind_ip_all")
    .WithInitBindMount("mongodb")
    .WithDataVolume("PulsarData");

var redis = builder
    .AddRedis("pulsar-redis", port: int.Parse(builder.Configuration["Redis:Port"]!))
    .WithDataVolume("PulsarRedis");

// SERVICE BUS MIGRATIONS
var serviceBusMigrations = builder.AddProject<Projects.ServiceBus_Migrations>("servicebus-migrations");

// IDENTITY
var identityMigrations = builder.AddProject<Projects.Identity_Migrations>("identity-migrations")
    .WithReference(mongo);

var identityApi = builder
    .AddProject<Projects.Identity_API>("identity-api")
    .WithReference(mongo)
    .WithReference(identityMigrations);

builder.AddProject<Projects.Identity_Functions>("identity-functions")
    .WithReference(mongo)
    .WithReference(identityMigrations)
    .WithReference(serviceBusMigrations);

builder.AddProject<Projects.Identity_EventDispatcher>("identity-eventdispatcher")
    .WithReference(mongo)
    .WithReference(identityMigrations)
    .WithReference(serviceBusMigrations);


// CATALOG
var catalogMigrations = builder.AddProject<Projects.Catalog_Migrations>("catalog-migrations")
    .WithReference(mongo);

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(mongo)
    .WithReference(redis)
    .WithReference(catalogMigrations);

// CROSS REFERENCES
catalogApi.WithReference(identityApi);
identityApi.WithReference(catalogApi);


builder.Build().Run();