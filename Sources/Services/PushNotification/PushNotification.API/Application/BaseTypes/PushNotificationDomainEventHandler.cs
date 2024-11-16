using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.DDD.Contexts;
using Pulsar.BuildingBlocks.EventBus.Abstractions;
using Pulsar.Services.Identity.Contracts.Shadows;
using Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications;

namespace Pulsar.Services.PushNotification.API.Application.BaseTypes;

public abstract class PushNotificationDomainEventHandler<TEvent> : DomainEventHandler<TEvent> where TEvent : INotification
{
	protected INotificacaoPushRepository NotificacaoPushRepository { get; }
	protected IShadowRepository<DominioShadow> DominioRepository { get; }
	protected IShadowRepository<UsuarioShadow> UsuarioRepository { get; }
	protected ILogger Logger { get; }
	public ISaveIntegrationEventLog EventLog { get; }

	protected PushNotificationDomainEventHandler(PushNotificationDomainEventHandlerContext<TEvent> ctx) : base(ctx.Session, ctx.DbContextFactory)
	{
		NotificacaoPushRepository = (INotificacaoPushRepository)ctx.Repositories.First(r => r is INotificacaoPushRepository);
		DominioRepository = (IShadowRepository<DominioShadow>)ctx.Repositories.First(r => r is IShadowRepository<DominioShadow>);
		UsuarioRepository = (IShadowRepository<UsuarioShadow>)ctx.Repositories.First(r => r is IShadowRepository<UsuarioShadow>);
		Logger = ctx.Logger;
		EventLog = ctx.EventLog;
	}
}

public class PushNotificationDomainEventHandlerContext<TEvent> where TEvent : INotification
{
	public ILogger<PushNotificationDomainEventHandler<TEvent>> Logger { get; }
	public IDbSession Session { get; }
	public DbContextFactory DbContextFactory { get; }
	public IEnumerable<IIsRepository> Repositories { get; }
	public ISaveIntegrationEventLog EventLog { get; }

	public PushNotificationDomainEventHandlerContext(ILogger<PushNotificationDomainEventHandler<TEvent>> logger, IDbSession session, DbContextFactory contextFactory, IEnumerable<IIsRepository> repositories, ISaveIntegrationEventLog eventLog)
	{
		Logger = logger;
		Session = session;
		Repositories = repositories;
		EventLog = eventLog;
		DbContextFactory = contextFactory;
	}
}
