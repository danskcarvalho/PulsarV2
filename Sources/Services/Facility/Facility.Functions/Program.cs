using Azure.Core.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pulsar.BuildingBlocks.Sync.Functions;
using Pulsar.Services.Facility.Functions.Application.BaseTypes;
using Pulsar.Services.Facility.Infrastructure.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pulsar.Services.Identity.Contracts.Shadows;
using static Pulsar.BuildingBlocks.Utils.GeneralExtensions;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((WorkerOptions cfg) =>
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ObjectIdConverter());
        options.Converters.Add(new JsonStringEnumConverter());
        cfg.Serializer = new JsonObjectSerializer(options);
    })
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder
            .AddJsonFile(Path.Combine(context.HostingEnvironment.ContentRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
            .AddJsonFile(Path.Combine(context.HostingEnvironment.ContentRootPath, $"appsettings.{context.HostingEnvironment.EnvironmentName}.json"), optional: true, reloadOnChange: false);
    })
    .ConfigureServices(s =>
    {
        s.AddMongoDB(
            typeof(EstabelecimentoMongoRepository).Assembly,
            // shadows from other services
            typeof(UsuarioShadow).Assembly
            );
        s.AddSyncFunctionServices();
        s.AddMediatR(c =>
        {
            c.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
        s.AddTransient(typeof(FacilityCommandHandlerContext<>));
        s.AddTransient(typeof(FacilityDomainEventHandlerContext<>));
        s.AddTransient(typeof(FacilityCommandHandlerContext<,>));
    })
    .Build();

host.Run();
