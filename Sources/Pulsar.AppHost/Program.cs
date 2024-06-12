using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);


var mongo = builder.AddMongoDB("PulsarMongoCluster", port: int.Parse(builder.Configuration["MongoDB:Port"]!))
    .WithArgs("--replSet", "rs0", "--bind_ip_all")
    .WithInitBindMount("mongodb")
    .WithDataVolume("PulsarData");

var identityMigrations = builder.AddProject<Projects.Identity_Migrations>("identity-migrations")
    .WithReference(mongo);

builder
    .AddProject<Projects.Identity_API>("identity-api")
    .WithReference(mongo)
    .WithReference(identityMigrations);

builder.AddProject<Projects.Identity_Functions>("identity-functions");


builder.AddProject<Projects.Identity_EventDispatcher>("identity-eventdispatcher");


builder.Build().Run();
