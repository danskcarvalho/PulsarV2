using Pulsar.BuildingBlocks.Utils;

namespace Pulsar.BuildingBlocks.Migrations;

public static class DIExtensions
{
    public static void AddMigrations(this IServiceCollection col)
    {
        col.AddMediatR(c =>
        {
            c.RegisterServicesFromAssembly(typeof(DIExtensions).GetTypeInfo().Assembly);
        });

        AutoMappingConventions.Register();

        col.AddSingleton<MongoDbSessionFactory>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connStr = GetConnectionString(config);
            var database = config.GetOrThrow("MongoDB:Database");

            return new MongoDbSessionFactory(connStr, database, null, () => sp.GetRequiredService<IMediator>());
        });
        col.AddSingleton<IDbSessionFactory, MongoDbSessionFactory>(sp => sp.GetRequiredService<MongoDbSessionFactory>());
        col.AddScoped<MongoDbSession>(sp =>
        {
            var factory = sp.GetRequiredService<MongoDbSessionFactory>();
            return (MongoDbSession)factory.CreateSession();
        });
        col.AddScoped<IDbSession, MongoDbSession>(sp => sp.GetRequiredService<MongoDbSession>());

        col.AddSingleton<MigrationRunner>();
        col.AddScoped<ScopedMigration>();
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        var connectionStringName = configuration.GetOrThrow("MongoDB:ConnectionStringName");
        var connectionString = configuration.GetOrThrow("ConnectionStrings:" + connectionStringName);
        return connectionString;
    }

    public static void AddMigrationsWithoutDatabaseMachinery(this IServiceCollection col)
    {
        col.AddSingleton<MigrationRunner>();
        col.AddScoped<ScopedMigration>();
    }
}
