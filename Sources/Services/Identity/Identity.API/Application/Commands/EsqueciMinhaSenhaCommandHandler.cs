using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.Contracts.Commands;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.Commands;

[NoTransaction, RetryOnException(VersionConcurrency = true, Retries = 2)]
public class EsqueciMinhaSenhaCommandHandler : IdentityCommandHandler<EsqueciMinhaSenhaCommand>
{
    public EsqueciMinhaSenhaCommandHandler(IDbSession session, IEnumerable<IIsRepository> repositories) : base(session, repositories)
    {
    }

    protected override async Task HandleAsync(EsqueciMinhaSenhaCommand cmd, CancellationToken ct)
    {
        var usuario = await UsuarioRepository.FindOneAsync(new FindUsuarioByUsenameOrEmailSpec(cmd.UsernameOrEmail!), ct);
        if (usuario is null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
        
        usuario.GerarTokenMudancaSenha(out long previousVersion);
        await UsuarioRepository.ReplaceOneAsync(usuario, previousVersion).CheckModified();
    }
}
