namespace Pulsar.Services.Identity.BackgroundTasks.Application.BaseTypes;

public abstract class IdentityDomainEventHandler<TRequest> : DomainEventHandler<TRequest> where TRequest : INotification
{
    protected IConviteRepository ConviteRepository { get; }
    protected IDominioRepository DominioRepository { get; }
    protected IEstabelecimentoRepository EstabelecimentoRepository { get; }
    protected IGrupoRepository GrupoRepository { get; }
    protected IRedeEstabelecimentosRepository RedeEstabelecimentosRepository { get; }
    protected IUsuarioRepository UsuarioRepository { get; }
    protected ILogger Logger { get; }

    protected IdentityDomainEventHandler(ILogger<IdentityDomainEventHandler<TRequest>> logger, IDbSession session, IEnumerable<IIsRepository> repositories) : base(session)
    {
        ConviteRepository = (IConviteRepository)repositories.First(r => r is IConviteRepository);
        DominioRepository = (IDominioRepository)repositories.First(r => r is IDominioRepository);
        EstabelecimentoRepository = (IEstabelecimentoRepository)repositories.First(r => r is IEstabelecimentoRepository);
        GrupoRepository = (IGrupoRepository)repositories.First(r => r is IGrupoRepository);
        RedeEstabelecimentosRepository = (IRedeEstabelecimentosRepository)repositories.First(r => r is IRedeEstabelecimentosRepository);
        UsuarioRepository = (IUsuarioRepository)repositories.First(r => r is IUsuarioRepository);
        Logger = logger;
    }
}
