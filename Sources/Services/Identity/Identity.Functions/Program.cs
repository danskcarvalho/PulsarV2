using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder
            .AddJsonFile(Path.Combine(context.HostingEnvironment.ContentRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
            .AddJsonFile(Path.Combine(context.HostingEnvironment.ContentRootPath, $"appsettings.{context.HostingEnvironment.EnvironmentName}.json"), optional: true, reloadOnChange: false);
    })
.ConfigureServices(s =>
{
    s.AddMongoDB(typeof(UsuarioMongoRepository).Assembly);
        s.AddMediatR(c =>
        {
            c.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
        s.AddTransient(typeof(IdentityCommandHandlerContext<>));
        s.AddTransient(typeof(IdentityDomainEventHandlerContext<>));
        s.AddTransient(typeof(IdentityCommandHandlerContext<,>));
    })
    .Build();

host.Run();
