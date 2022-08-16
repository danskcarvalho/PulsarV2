using Pulsar.BuildingBlocks.DDD.Abstractions;

namespace Pulsar.Services.Identity.API.Application.Commands;

public abstract class IdentityCommandHandler<TRequest> : CommandHandler<TRequest> where TRequest : IRequest<Unit>
{
    protected IConviteRepository ConviteRepository { get; }
    protected IDominioRepository DominioRepository { get; }
    protected IEstabelecimentoRepository EstabelecimentoRepository { get; }
    protected IGrupoRepository GrupoRepository { get; }
    protected IRedeEstabelecimentosRepository RedeEstabelecimentosRepository { get; }
    protected IUsuarioRepository UsuarioRepository { get; }

    protected IdentityCommandHandler(IConviteRepository conviteRepository, IDominioRepository dominioRepository, IEstabelecimentoRepository estabelecimentoRepository, IGrupoRepository grupoRepository, 
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

public abstract class IdentityCommandHandler<TRequest, TResponse> : CommandHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    protected IConviteRepository ConviteRepository { get; }
    protected IDominioRepository DominioRepository { get; }
    protected IEstabelecimentoRepository EstabelecimentoRepository { get; }
    protected IGrupoRepository GrupoRepository { get; }
    protected IRedeEstabelecimentosRepository RedeEstabelecimentosRepository { get; }
    protected IUsuarioRepository UsuarioRepository { get; }

    protected IdentityCommandHandler(IConviteRepository conviteRepository, IDominioRepository dominioRepository, IEstabelecimentoRepository estabelecimentoRepository, IGrupoRepository grupoRepository,
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
