var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMigrations();
    })
    .Build();

var migrations = host.Services.GetRequiredService<MigrationRunner>();
await migrations.Run(typeof(Program).Assembly);
public partial class Program { }