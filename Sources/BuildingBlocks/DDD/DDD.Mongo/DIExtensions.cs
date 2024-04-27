using Pulsar.BuildingBlocks.DDD.Contexts;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.DDD.Mongo;

public static class DIExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection col, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t =>
            {
                return t.IsPublic && t.IsClass && !t.IsGenericTypeDefinition && !t.IsAbstract &&
                    t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));
            });

            foreach (var validator in types)
            {
                col.AddTransient(validator);
                var interfaceTypes = validator.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));
                foreach (var interfaceType in interfaceTypes)
                {
                    col.AddTransient(interfaceType, validator);
                }
            }
        }

        return col;
    }
    public static IServiceCollection AddMongoDB(this IServiceCollection col, params Assembly[] assembliesToScan)
    {
        AutoMappingConventions.Register();

        col.AddSingleton<MongoDbSessionFactory>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connStr = config.GetOrThrow("MongoDB:ConnectionString");
            var database = config.GetOrThrow("MongoDB:Database");
            var clusterName = config.GetOrThrow("MongoDB:ClusterName");

            return new MongoDbSessionFactory(connStr, database, clusterName, () => sp.GetRequiredService<IMediator>());
        });
        col.AddSingleton<IDbSessionFactory, MongoDbSessionFactory>(sp => sp.GetRequiredService<MongoDbSessionFactory>());
        col.AddScoped<MongoDbSession>(sp =>
        {
            var factory = sp.GetRequiredService<MongoDbSessionFactory>();
            return (MongoDbSession)factory.CreateSession(sp.GetRequiredService<IMediator>());
        });
        col.AddScoped<IDbSession, MongoDbSession>(sp => sp.GetRequiredService<MongoDbSession>());

        // repositories
        AddRepositories(col, assembliesToScan);

        col.AddTransient<MongoSaveIntegrationEventLog>();
        col.AddTransient<ISaveIntegrationEventLog, MongoSaveIntegrationEventLog>();
        col.AddTransient<DbContextFactory>();

        //query handlers
        AddQueryHandlers(col, assembliesToScan);

        col.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        col.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));

        // shadow repositories
        AddShadowRepositories(col, assembliesToScan);

        return col;
    }

    private static void AddShadowRepositories(IServiceCollection col, Assembly[] assembliesToScan)
    {
        var types = ShadowAttribute.GetShadowTypes(assembliesToScan);

        foreach (var item in types)
        {
            var impl = typeof(MongoShadowRepository<>).MakeGenericType(item.Type);
            var shadowInt = typeof(IShadowRepository<>).MakeGenericType(item.Type);
            col.AddTransient(impl);
            col.AddTransient(shadowInt, impl);
            col.AddTransient(typeof(IIsRepository), impl);
        }
    }

    private static void AddQueryHandlers(IServiceCollection col, Assembly[] assembliesToScan)
    {
        foreach (var assembly in assembliesToScan)
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
    }

    private static void AddRepositories(IServiceCollection col, Assembly[] assembliesToScan)
    {
        foreach (var assembly in assembliesToScan)
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
                col.AddTransient(typeof(IIsRepository), repoType);
            }
        }
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
