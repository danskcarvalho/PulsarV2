﻿namespace Pulsar.BuildingBlocks.DDD.Mongo;

public static class DIExtensions
{
    public static void AddMongoDB(this IServiceCollection col, params Assembly[] assemblies)
    {
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

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t =>
            {
                return t.IsPublic && t.IsClass && !t.IsGenericTypeDefinition && !t.IsAbstract &&
                    EnumerateAllBaseTypes(t).Any(b => b.IsGenericType && b.GetGenericTypeDefinition() == typeof(MongoRepository<,>));
            });

            foreach (var repoType in types)
            {
                var interfaceType = repoType.GetInterfaces().Where(t => t.GetInterfaces().Any(s => s.IsGenericType && s.GetGenericTypeDefinition() == typeof(IRepository<,>))).First();
                col.AddTransient(repoType);
                col.AddTransient(interfaceType, repoType);
            }
        }

        col.AddTransient<MongoSaveIntegrationEventLog>();
        col.AddTransient<ISaveIntegrationEventLog, MongoSaveIntegrationEventLog>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t =>
            {
                return t.IsPublic && t.IsClass && !t.IsAbstract && EnumerateAllBaseTypes(t).Any(b => b == typeof(QueryHandler));
            });

            foreach (var qType in types)
            {
                col.AddTransient(qType);
            }
        }

        col.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        col.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));
    }

    private static IEnumerable<Type> EnumerateAllBaseTypes(Type t)
    {
        var bType = t.BaseType;
        while (bType != null)
        {
            yield return bType;
            bType = bType.BaseType;
        }
    }
}