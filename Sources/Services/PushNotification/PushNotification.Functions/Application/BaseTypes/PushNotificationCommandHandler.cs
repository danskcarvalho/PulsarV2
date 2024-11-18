using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.DDD.Contexts;
using Pulsar.BuildingBlocks.EventBus.Abstractions;
using Pulsar.Services.Identity.Contracts.Shadows;
using Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications;
using Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;
using Pulsar.Services.PushNotification.Domain.Aggregates.UserContexts;

namespace Pulsar.Services.PushNotification.Functions.Application.BaseTypes;

public abstract class PushNotificationCommandHandler<TRequest> : CommandHandler<TRequest> where TRequest : IRequest
{
	protected INotificacaoPushRepository PushNotificationRepository { get; }
	protected ISessionRepository SessionRepository { get; }
	protected IUserContextRepository UserContextRepository { get; }
	protected IShadowRepository<DominioShadow> DominioRepository { get; }
	protected IShadowRepository<UsuarioShadow> UsuarioRepository { get; }
	protected ILogger Logger { get; }
	public ISaveIntegrationEventLog EventLog { get; }

	protected PushNotificationCommandHandler(PushNotificationCommandHandlerContext<TRequest> ctx) : base(ctx.Session, ctx.DbContextFactory)
	{
		PushNotificationRepository = (INotificacaoPushRepository)ctx.Repositories.First(r => r is INotificacaoPushRepository);
		DominioRepository = (IShadowRepository<DominioShadow>)ctx.Repositories.First(r => r is IShadowRepository<DominioShadow>);
		UsuarioRepository = (IShadowRepository<UsuarioShadow>)ctx.Repositories.First(r => r is IShadowRepository<UsuarioShadow>);
		SessionRepository = (ISessionRepository)ctx.Repositories.First(r => r is ISessionRepository);
		UserContextRepository = (IUserContextRepository)ctx.Repositories.First(r => r is IUserContextRepository);
		Logger = ctx.Logger;
		EventLog = ctx.EventLog;
	}
}

public abstract class PushNotificationCommandHandler<TRequest, TResponse> : CommandHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
	protected INotificacaoPushRepository PushNotificationRepository { get; }
	protected ISessionRepository SessionRepository { get; }
	protected IUserContextRepository UserContextRepository { get; }
	protected IShadowRepository<DominioShadow> DominioRepository { get; }
	protected IShadowRepository<UsuarioShadow> UsuarioRepository { get; }
	protected ILogger Logger { get; }
	public ISaveIntegrationEventLog EventLog { get; }

	protected PushNotificationCommandHandler(PushNotificationCommandHandlerContext<TRequest, TResponse> ctx) : base(ctx.Session, ctx.DbContextFactory)
	{
		PushNotificationRepository = (INotificacaoPushRepository)ctx.Repositories.First(r => r is INotificacaoPushRepository);
		DominioRepository = (IShadowRepository<DominioShadow>)ctx.Repositories.First(r => r is IShadowRepository<DominioShadow>);
		UsuarioRepository = (IShadowRepository<UsuarioShadow>)ctx.Repositories.First(r => r is IShadowRepository<UsuarioShadow>);
		SessionRepository = (ISessionRepository)ctx.Repositories.First(r => r is ISessionRepository);
		UserContextRepository = (IUserContextRepository)ctx.Repositories.First(r => r is IUserContextRepository);
		Logger = ctx.Logger;
		EventLog = ctx.EventLog;
	}
}

public class PushNotificationCommandHandlerContext<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
	public ILogger<PushNotificationCommandHandler<TRequest, TResponse>> Logger { get; }
	public IDbSession Session { get; }
	public DbContextFactory DbContextFactory { get; }
	public IEnumerable<IIsRepository> Repositories { get; }
	public ISaveIntegrationEventLog EventLog { get; }

	public PushNotificationCommandHandlerContext(ILogger<PushNotificationCommandHandler<TRequest, TResponse>> logger, IDbSession session, DbContextFactory contextFactory, IEnumerable<IIsRepository> repositories, ISaveIntegrationEventLog eventLog)
	{
		Logger = logger;
		Session = session;
		DbContextFactory = contextFactory;
		Repositories = repositories;
		EventLog = eventLog;
	}
}

public class PushNotificationCommandHandlerContext<TEvent> where TEvent : IRequest
{
	public ILogger<PushNotificationCommandHandler<TEvent>> Logger { get; }
	public IDbSession Session { get; }
	public DbContextFactory DbContextFactory { get; }
	public IEnumerable<IIsRepository> Repositories { get; }
	public ISaveIntegrationEventLog EventLog { get; }

	public PushNotificationCommandHandlerContext(ILogger<PushNotificationCommandHandler<TEvent>> logger, IDbSession session, DbContextFactory contextFactory, IEnumerable<IIsRepository> repositories, ISaveIntegrationEventLog eventLog)
	{
		Logger = logger;
		Session = session;
		DbContextFactory = contextFactory;
		Repositories = repositories;
		EventLog = eventLog;
	}
}
