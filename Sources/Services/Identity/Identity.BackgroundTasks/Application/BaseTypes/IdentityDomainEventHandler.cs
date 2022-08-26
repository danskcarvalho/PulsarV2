using Pulsar.BuildingBlocks.EventBus.Abstractions;

namespace Pulsar.Services.Identity.BackgroundTasks.Application.BaseTypes;

public abstract class IdentityDomainEventHandler<TEvent> : DomainEventHandler<TEvent> where TEvent : INotification
{
    protected IConviteRepository ConviteRepository { get; }
    protected IDominioRepository DominioRepository { get; }
    protected IEstabelecimentoRepository EstabelecimentoRepository { get; }
    protected IGrupoRepository GrupoRepository { get; }
    protected IRedeEstabelecimentosRepository RedeEstabelecimentosRepository { get; }
    protected IUsuarioRepository UsuarioRepository { get; }
    protected ILogger Logger { get; }
    public ISaveIntegrationEventLog EventLog { get; }

    protected IdentityDomainEventHandler(IdentityDomainEventHandlerContext<TEvent> ctx) : base(ctx.Session)
    {
        ConviteRepository = (IConviteRepository)ctx.Repositories.First(r => r is IConviteRepository);
        DominioRepository = (IDominioRepository)ctx.Repositories.First(r => r is IDominioRepository);
        EstabelecimentoRepository = (IEstabelecimentoRepository)ctx.Repositories.First(r => r is IEstabelecimentoRepository);
        GrupoRepository = (IGrupoRepository)ctx.Repositories.First(r => r is IGrupoRepository);
        RedeEstabelecimentosRepository = (IRedeEstabelecimentosRepository)ctx.Repositories.First(r => r is IRedeEstabelecimentosRepository);
        UsuarioRepository = (IUsuarioRepository)ctx.Repositories.First(r => r is IUsuarioRepository);
        Logger = ctx.Logger;
        EventLog = ctx.EventLog;
    }
}

public class IdentityDomainEventHandlerContext<TEvent> where TEvent : INotification
{
    public ILogger<IdentityDomainEventHandler<TEvent>> Logger { get; }
    public IDbSession Session { get; }
    public IEnumerable<IIsRepository> Repositories { get; }
    public ISaveIntegrationEventLog EventLog { get; }

    public IdentityDomainEventHandlerContext(ILogger<IdentityDomainEventHandler<TEvent>> logger, IDbSession session, IEnumerable<IIsRepository> repositories, ISaveIntegrationEventLog eventLog)
    {
        Logger = logger;
        Session = session;
        Repositories = repositories;
        EventLog = eventLog;
    }
}
