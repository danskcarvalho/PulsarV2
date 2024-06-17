using Pulsar.BuildingBlocks.Caching.Abstractions;

namespace Catalog.API.Application.BaseTypes;

public class CatalogQueriesContext
{
    public MongoDbSessionFactory Factory { get; }
    public ICacheServer CacheServer { get; }
    public string ClusterName { get; }

    public CatalogQueriesContext(MongoDbSessionFactory factory, ICacheServer cacheServer, string clusterName)
    {
        Factory = factory;
        CacheServer = cacheServer;
        ClusterName = clusterName;
    }
}