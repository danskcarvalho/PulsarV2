namespace Pulsar.Services.Identity.API.Application.DomainEvents;

public abstract class IdentityDomainEventHandler<TRequest> : DomainEventHandler<TRequest> where TRequest : INotification
{
    protected IConviteRepository ConviteRepository { get; }
    protected IDominioRepository DominioRepository { get; }
    protected IEstabelecimentoRepository EstabelecimentoRepository { get; }
    protected IGrupoRepository GrupoRepository { get; }
    protected IRedeEstabelecimentosRepository RedeEstabelecimentosRepository { get; }
    protected IUsuarioRepository UsuarioRepository { get; }

    protected IdentityDomainEventHandler(IConviteRepository conviteRepository, IDominioRepository dominioRepository, IEstabelecimentoRepository estabelecimentoRepository, IGrupoRepository grupoRepository, 
        IRedeEstabelecimentosRepository redeEstabelecimentosRepository, IUsuarioRepository usuarioRepository, IDbSession session) : base(session)
    {
        ConviteRepository = conviteRepository;
        DominioRepository = dominioRepository;
        EstabelecimentoRepository = estabelecimentoRepository;
        GrupoRepository = grupoRepository;
        RedeEstabelecimentosRepository = redeEstabelecimentosRepository;
        UsuarioRepository = usuarioRepository;
    }
}
