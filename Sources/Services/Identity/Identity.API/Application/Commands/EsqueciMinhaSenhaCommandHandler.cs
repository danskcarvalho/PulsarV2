using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.Contracts.Commands;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.Commands;

[NoTransaction, RetryOnException(VersionConcurrency = true, Retries = 2)]
public class EsqueciMinhaSenhaCommandHandler : IdentityCommandHandler<EsqueciMinhaSenhaCommand>
{
    public EsqueciMinhaSenhaCommandHandler(IConviteRepository conviteRepository, IDominioRepository dominioRepository, IEstabelecimentoRepository estabelecimentoRepository, IGrupoRepository grupoRepository, 
        IRedeEstabelecimentosRepository redeEstabelecimentosRepository, IUsuarioRepository usuarioRepository, IDbSession session) : base(conviteRepository, dominioRepository, estabelecimentoRepository, grupoRepository, redeEstabelecimentosRepository, usuarioRepository, session)
    {
    }

    protected override async Task HandleAsync(EsqueciMinhaSenhaCommand request, CancellationToken ct)
    {
        var usuario = await UsuarioRepository.FindOneAsync(new GetUsuarioByUsenameOrEmailSpec(request.UsernameOrEmail), ct);
        if (usuario is null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
        
        usuario.GerarTokenMudancaSenha(out long previousVersion);
        await UsuarioRepository.ReplaceOneAsync(usuario, previousVersion).CheckModified();
    }
}
