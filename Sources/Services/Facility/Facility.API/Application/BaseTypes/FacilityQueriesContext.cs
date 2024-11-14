using Pulsar.BuildingBlocks.Caching.Abstractions;

namespace Pulsar.Services.Facility.API.Application.BaseTypes;

public class FacilityQueriesContext
{
	public MongoDbSessionFactory Factory { get; }
	public ICacheServer CacheServer { get; }
	public string ClusterName { get; }

	public FacilityQueriesContext(MongoDbSessionFactory factory, ICacheServer cacheServer, string clusterName)
	{
		Factory = factory;
		CacheServer = cacheServer;
		ClusterName = clusterName;
	}
}