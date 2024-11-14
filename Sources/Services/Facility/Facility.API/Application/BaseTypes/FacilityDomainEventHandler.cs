using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.DDD.Contexts;
using Pulsar.BuildingBlocks.EventBus.Abstractions;
using Pulsar.Services.Facility.Domain.Aggregates.Estabelecimentos;
using Pulsar.Services.Identity.Contracts.Shadows;

namespace Pulsar.Services.Identity.API.Application.BaseTypes;

public abstract class FacilityDomainEventHandler<TEvent> : DomainEventHandler<TEvent> where TEvent : INotification
{
	protected IEstabelecimentoRepository EstabelecimentoRepository { get; }
	protected IShadowRepository<DominioShadow> DominioRepository { get; }
	protected IShadowRepository<UsuarioShadow> UsuarioRepository { get; }
	protected ILogger Logger { get; }
	public ISaveIntegrationEventLog EventLog { get; }

	protected FacilityDomainEventHandler(FacilityDomainEventHandlerContext<TEvent> ctx) : base(ctx.Session, ctx.DbContextFactory)
    {
		EstabelecimentoRepository = (IEstabelecimentoRepository)ctx.Repositories.First(r => r is IEstabelecimentoRepository);
		DominioRepository = (IShadowRepository<DominioShadow>)ctx.Repositories.First(r => r is IShadowRepository<DominioShadow>);
		UsuarioRepository = (IShadowRepository<UsuarioShadow>)ctx.Repositories.First(r => r is IShadowRepository<UsuarioShadow>);
		Logger = ctx.Logger;
		EventLog = ctx.EventLog;
	}
}

public class FacilityDomainEventHandlerContext<TEvent> where TEvent : INotification
{
    public ILogger<FacilityDomainEventHandler<TEvent>> Logger { get; }
    public IDbSession Session { get; }
    public DbContextFactory DbContextFactory { get; }
    public IEnumerable<IIsRepository> Repositories { get; }
    public ISaveIntegrationEventLog EventLog { get; }

    public FacilityDomainEventHandlerContext(ILogger<FacilityDomainEventHandler<TEvent>> logger, IDbSession session, DbContextFactory contextFactory, IEnumerable<IIsRepository> repositories, ISaveIntegrationEventLog eventLog)
    {
        Logger = logger;
        Session = session;
        Repositories = repositories;
        EventLog = eventLog;
        DbContextFactory = contextFactory;
    }
}
