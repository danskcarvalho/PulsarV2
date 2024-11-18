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
    .WithReference(mongo)
    .WaitFor(mongo);

var identityApi = builder
    .AddProject<Projects.Identity_API>("identity-api")
    .WithReference(mongo)
    .WithReference(identityMigrations)
	.WaitFor(mongo)
    .WaitForCompletion(identityMigrations);

var storageIdentityFunctions = builder.AddAzureStorage("pulsar-fnstorage-identity")
					 .RunAsEmulator();

builder.AddAzureFunctionsProject<Projects.Identity_Functions>("identity-functions")
	.WithHostStorage(storageIdentityFunctions)
	.WithReference(mongo)
	.WithReference(identityMigrations)
	.WithReference(serviceBusMigrations)
	.WaitFor(mongo)
	.WaitForCompletion(identityMigrations)
	.WaitForCompletion(serviceBusMigrations);

builder.AddProject<Projects.Identity_EventDispatcher>("identity-eventdispatcher")
    .WithReference(mongo)
    .WithReference(identityMigrations)
    .WithReference(serviceBusMigrations)
	.WaitFor(mongo)
	.WaitForCompletion(identityMigrations)
	.WaitForCompletion(serviceBusMigrations);


// CATALOG
var catalogMigrations = builder.AddProject<Projects.Catalog_Migrations>("catalog-migrations")
    .WithReference(mongo)
	.WaitFor(mongo);

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(mongo)
    .WithReference(redis)
    .WithReference(catalogMigrations)
	.WaitFor(mongo)
	.WaitForCompletion(catalogMigrations);

// FACILITY
var facilityMigrations = builder.AddProject<Projects.Facility_Migrations>("facility-migrations")
	.WithReference(mongo)
	.WaitFor(mongo);

var facilityApi = builder.AddProject<Projects.Facility_API>("facility-api")
	.WithReference(mongo)
	.WithReference(redis)
	.WithReference(facilityMigrations)
	.WaitFor(mongo)
	.WaitForCompletion(facilityMigrations);

var storageFacilityFunctions = builder.AddAzureStorage("pulsar-fnstorage-facility")
					 .RunAsEmulator();

builder.AddAzureFunctionsProject<Projects.Facility_Functions>("facility-functions")
	.WithHostStorage(storageFacilityFunctions)
	.WithReference(mongo)
	.WithReference(facilityMigrations)
	.WithReference(serviceBusMigrations)
	.WaitFor(mongo)
	.WaitForCompletion(facilityMigrations)
	.WaitForCompletion(serviceBusMigrations);

builder.AddProject<Projects.Facility_EventDispatcher>("facility-eventdispatcher")
	.WithReference(mongo)
	.WithReference(facilityMigrations)
	.WithReference(serviceBusMigrations)
	.WaitFor(mongo)
	.WaitForCompletion(facilityMigrations)
	.WaitForCompletion(serviceBusMigrations);

// PUSH NOTIFICATION
var pushNotificationMigrations = builder.AddProject<Projects.PushNotification_Migrations>("pushnotification-migrations")
    .WithReference(mongo)
	.WaitFor(mongo);

var pushNotificationApi = builder.AddProject<Projects.PushNotification_API>("pushnotification-api")
    .WithReference(mongo)
    .WithReference(redis)
    .WithReference(pushNotificationMigrations)
	.WaitFor(mongo)
	.WaitForCompletion(pushNotificationMigrations);

var storagePushNotifications = builder.AddAzureStorage("pulsar-fnstorage-pushnotification")
					 .RunAsEmulator();

var pushNotificationFunctions = builder.AddAzureFunctionsProject<Projects.PushNotification_Functions>("pushnotification-functions")
	.WithHostStorage(storagePushNotifications)
	.WithReference(mongo)
	.WithReference(pushNotificationMigrations)
	.WithReference(serviceBusMigrations)
	.WaitFor(mongo)
	.WaitForCompletion(pushNotificationMigrations)
	.WaitForCompletion(serviceBusMigrations);

// FRONTEND
var frontend = builder.AddProject<Projects.Pulsar_Web>("pulsar-web", "https");

// CROSS REFERENCES
// IDENTITY-API
identityApi.WithReference(catalogApi);
identityApi.WithReference(facilityApi);
identityApi.WithReference(frontend);
identityApi.WithReference(pushNotificationApi);
// CATALOG-API
catalogApi.WithReference(identityApi);
// FACILITY-API
facilityApi.WithReference(identityApi);
// PUSH NOTIFICATION-API
pushNotificationApi.WithReference(identityApi);
pushNotificationApi.WithReference(pushNotificationFunctions);
// FRONTEND
frontend.WithReference(identityApi);
frontend.WithReference(catalogApi);
frontend.WithReference(facilityApi);
frontend.WithReference(pushNotificationApi);

builder.Build().Run();
