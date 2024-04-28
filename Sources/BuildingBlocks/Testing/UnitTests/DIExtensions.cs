using Microsoft.Extensions.DependencyInjection;
using Pulsar.BuildingBlocks.DDD.Contexts;
using System.Diagnostics;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Mongo;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.UnitTests;

public static class DIExtensions
{
    public static void AddMockedMongoDB(this IServiceCollection col, IMockedDatabase database, params Assembly[] assemblies)
    {
        AutoMappingConventions.Register();

        col.AddScoped<IMockedDbSessionFactory>(sp =>
        {
            return Mock.SessionFactory(database, () => sp.GetRequiredService<IMediator>());
        });
        col.AddScoped<IDbSessionFactory>(sp => sp.GetRequiredService<IMockedDbSessionFactory>());
        col.AddScoped<IMockedDbSession>(sp =>
        {
            var factory = sp.GetRequiredService<IMockedDbSessionFactory>();
            return factory.CreateSession(sp.GetRequiredService<IMediator>());
        });
        col.AddScoped<IDbSession>(sp => sp.GetRequiredService<IMockedDbSession>());

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t =>
            {
                return t.IsPublic && t.IsClass && !t.IsGenericTypeDefinition && !t.IsAbstract &&
                    EnumerateAllBaseTypes(t).Any(b => b.IsGenericType && b.GetGenericTypeDefinition() == typeof(Repository<,>));
            });

            foreach (var repoType in types)
            {
                var interfaceType = repoType.GetInterfaces().First(t => t.GetInterfaces().Any(s => s.IsGenericType && s.GetGenericTypeDefinition() == typeof(IRepository<,>)));
                var entityType = interfaceType.GetInterfaces().First(t =>
                        t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(IRepository<,>))
                    .GetGenericArguments()[1];
                var baseInterfaceType = typeof(IRepositoryBase<>).MakeGenericType(entityType);
                col.AddTransient(repoType);
                col.AddTransient(interfaceType, repoType);
                col.AddTransient(baseInterfaceType, repoType);
                col.AddTransient(typeof(IIsRepository), repoType);
            }
        }
        
        AddShadowRepositories(col, assemblies);

        col.AddScoped<SaveIntegrationEventLog>();
        col.AddScoped<ISaveIntegrationEventLog, SaveIntegrationEventLog>();
        col.AddTransient<DbContextFactory>();
    }
    
    public static void AddShadowRepositories(IServiceCollection col, Assembly[] assembliesToScan)
    {
        var types = ShadowAttribute.GetShadowTypes(assembliesToScan);

        foreach (var item in types)
        {
            var impl = typeof(ShadowRepository<>).MakeGenericType(item.Type);
            var shadowInt = typeof(IShadowRepository<>).MakeGenericType(item.Type);
            col.AddTransient(impl);
            col.AddTransient(shadowInt, impl);
            col.AddTransient(typeof(IIsRepository), impl);
        }
    }

    public static void AddMockedEmails(this IServiceCollection col)
    {
        col.AddScoped<MockedEmailService>();
        col.AddScoped<IEmailService>(sp => sp.GetRequiredService<MockedEmailService>());
    }

    public static void AddMockedFileSystem(this IServiceCollection col)
    {
        col.AddScoped<MockedFileSystem>();
        col.AddScoped<IFileSystem>(sp => sp.GetRequiredService<MockedFileSystem>());
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
