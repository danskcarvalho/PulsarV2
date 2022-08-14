namespace Pulsar.BuildingBlocks.Migrations;

public static class DIExtensions
{
    public static void AddMigrations(this IServiceCollection col)
    {
        col.AddMediatR(typeof(DIExtensions).GetTypeInfo().Assembly);

        AutoMappingConventions.Register();

        col.AddSingleton<MongoDbSessionFactory>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connStr = config["MongoDB:ConnectionString"];
            var database = config["MongoDB:Database"];

            return new MongoDbSessionFactory(connStr, database, () => sp.GetRequiredService<IMediator>());
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
}
