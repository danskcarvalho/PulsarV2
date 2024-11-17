using Pulsar.BuildingBlocks.Caching.Abstractions;
using Pulsar.BuildingBlocks.DDD.Mongo.Queries;
using Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications;
using Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;
using Pulsar.Services.PushNotification.Domain.Aggregates.UserContexts;

namespace Pulsar.Services.PushNotification.API.Application.BaseTypes;

public class PushNotificationQueries : QueryHandler
{
	public PushNotificationQueries(PushNotificationQueriesContext ctx) : base(ctx.Factory, ctx.ClusterName)
	{
		NotificacoesPushCollection = GetCollection<NotificacaoPush>(Constants.CollectionNames.NOTIFICACOES_PUSH);
		SessionsCollection = GetCollection<Session>(Constants.CollectionNames.SESSIONS);
		UserContextsCollection = GetCollection<UserContext>(Constants.CollectionNames.USER_CONTEXTS);
		CacheServer = ctx.CacheServer;
	}

	protected IMongoCollection<NotificacaoPush> NotificacoesPushCollection { get; private set; }
	protected IMongoCollection<Session> SessionsCollection { get; private set; }
	protected IMongoCollection<UserContext> UserContextsCollection { get; private set; }
	protected ICacheServer CacheServer { get; private set; }
}
