using Pulsar.BuildingBlocks.Caching.Abstractions;

namespace Pulsar.Services.PushNotification.API.Application.BaseTypes;

public class PushNotificationQueriesContext
{
	public MongoDbSessionFactory Factory { get; }
	public ICacheServer CacheServer { get; }
	public string ClusterName { get; }

	public PushNotificationQueriesContext(MongoDbSessionFactory factory, ICacheServer cacheServer, string clusterName)
	{
		Factory = factory;
		CacheServer = cacheServer;
		ClusterName = clusterName;
	}
}