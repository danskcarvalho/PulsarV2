using Pulsar.BuildingBlocks.Caching.Abstractions;
using Pulsar.BuildingBlocks.DDD.Mongo.Queries;

namespace Pulsar.Services.PushNotification.API.Application.BaseTypes;

public class PushNotificationQueries : QueryHandler
{
	public PushNotificationQueries(PushNotificationQueriesContext ctx) : base(ctx.Factory, ctx.ClusterName)
	{
		PushNotificationsCollection = GetCollection<Domain.Aggregates.PushNotifications.PushNotification>(Constants.CollectionNames.PUSH_NOTIFICATIONS);
		CacheServer = ctx.CacheServer;
	}

	protected IMongoCollection<Domain.Aggregates.PushNotifications.PushNotification> PushNotificationsCollection { get; private set; }
	protected ICacheServer CacheServer { get; private set; }
}
