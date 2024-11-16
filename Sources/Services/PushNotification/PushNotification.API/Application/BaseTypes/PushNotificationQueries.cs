using Pulsar.BuildingBlocks.Caching.Abstractions;
using Pulsar.BuildingBlocks.DDD.Mongo.Queries;
using Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications;

namespace Pulsar.Services.PushNotification.API.Application.BaseTypes;

public class PushNotificationQueries : QueryHandler
{
	public PushNotificationQueries(PushNotificationQueriesContext ctx) : base(ctx.Factory, ctx.ClusterName)
	{
		NotificacoesPushCollection = GetCollection<NotificacaoPush>(Constants.CollectionNames.NOTIFICACOES_PUSH);
		CacheServer = ctx.CacheServer;
	}

	protected IMongoCollection<NotificacaoPush> NotificacoesPushCollection { get; private set; }
	protected ICacheServer CacheServer { get; private set; }
}
