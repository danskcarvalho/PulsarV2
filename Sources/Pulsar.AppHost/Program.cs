using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// DATABASE
var mongo = builder.AddMongoDB("PulsarMongoCluster", port: int.Parse(builder.Configuration["MongoDB:Port"]!))
    .WithArgs("--replSet", "rs0", "--bind_ip_all")
    .WithInitBindMount("mongodb")
    .WithDataVolume("PulsarData");

// SERVICE BUS MIGRATIONS
var serviceBusMigrations = builder.AddProject<Projects.ServiceBus_Migrations>("servicebus-migrations");

// IDENTITY
var identityMigrations = builder.AddProject<Projects.Identity_Migrations>("identity-migrations")
    .WithReference(mongo);

builder
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


builder.Build().Run();
