using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.DDD.Contexts;
using Pulsar.BuildingBlocks.EventBus.Abstractions;
using Pulsar.Services.Facility.Domain.Aggregates.Estabelecimentos;
using Pulsar.Services.Identity.Contracts.Shadows;

namespace Pulsar.Services.Facility.Functions.Application.BaseTypes;

public abstract class FacilityCommandHandler<TRequest> : CommandHandler<TRequest> where TRequest : IRequest
{
	protected IEstabelecimentoRepository EstabelecimentoRepository { get; }
	protected IShadowRepository<DominioShadow> DominioRepository { get; }
	protected IShadowRepository<UsuarioShadow> UsuarioRepository { get; }
	protected ILogger Logger { get; }
	public ISaveIntegrationEventLog EventLog { get; }

	protected FacilityCommandHandler(FacilityCommandHandlerContext<TRequest> ctx) : base(ctx.Session, ctx.DbContextFactory)
	{
		EstabelecimentoRepository = (IEstabelecimentoRepository)ctx.Repositories.First(r => r is IEstabelecimentoRepository);
		DominioRepository = (IShadowRepository<DominioShadow>)ctx.Repositories.First(r => r is IShadowRepository<DominioShadow>);
		UsuarioRepository = (IShadowRepository<UsuarioShadow>)ctx.Repositories.First(r => r is IShadowRepository<UsuarioShadow>);
		Logger = ctx.Logger;
		EventLog = ctx.EventLog;
	}
}

public abstract class FacilityCommandHandler<TRequest, TResponse> : CommandHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
	protected IEstabelecimentoRepository EstabelecimentoRepository { get; }
	protected IShadowRepository<DominioShadow> DominioRepository { get; }
	protected IShadowRepository<UsuarioShadow> UsuarioRepository { get; }
	protected ILogger Logger { get; }
	public ISaveIntegrationEventLog EventLog { get; }

	protected FacilityCommandHandler(FacilityCommandHandlerContext<TRequest, TResponse> ctx) : base(ctx.Session, ctx.DbContextFactory)
	{
		EstabelecimentoRepository = (IEstabelecimentoRepository)ctx.Repositories.First(r => r is IEstabelecimentoRepository);
		DominioRepository = (IShadowRepository<DominioShadow>)ctx.Repositories.First(r => r is IShadowRepository<DominioShadow>);
		UsuarioRepository = (IShadowRepository<UsuarioShadow>)ctx.Repositories.First(r => r is IShadowRepository<UsuarioShadow>);
		Logger = ctx.Logger;
		EventLog = ctx.EventLog;
	}
}

public class FacilityCommandHandlerContext<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
	public ILogger<FacilityCommandHandler<TRequest, TResponse>> Logger { get; }
	public IDbSession Session { get; }
	public DbContextFactory DbContextFactory { get; }
	public IEnumerable<IIsRepository> Repositories { get; }
	public ISaveIntegrationEventLog EventLog { get; }

	public FacilityCommandHandlerContext(ILogger<FacilityCommandHandler<TRequest, TResponse>> logger, IDbSession session, DbContextFactory contextFactory, IEnumerable<IIsRepository> repositories, ISaveIntegrationEventLog eventLog)
	{
		Logger = logger;
		Session = session;
		DbContextFactory = contextFactory;
		Repositories = repositories;
		EventLog = eventLog;
	}
}

public class FacilityCommandHandlerContext<TEvent> where TEvent : IRequest
{
	public ILogger<FacilityCommandHandler<TEvent>> Logger { get; }
	public IDbSession Session { get; }
	public DbContextFactory DbContextFactory { get; }
	public IEnumerable<IIsRepository> Repositories { get; }
	public ISaveIntegrationEventLog EventLog { get; }

	public FacilityCommandHandlerContext(ILogger<FacilityCommandHandler<TEvent>> logger, IDbSession session, DbContextFactory contextFactory, IEnumerable<IIsRepository> repositories, ISaveIntegrationEventLog eventLog)
	{
		Logger = logger;
		Session = session;
		DbContextFactory = contextFactory;
		Repositories = repositories;
		EventLog = eventLog;
	}
}
