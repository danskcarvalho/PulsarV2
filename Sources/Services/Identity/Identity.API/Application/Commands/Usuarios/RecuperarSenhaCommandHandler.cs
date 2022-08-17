using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.Commands;

namespace Pulsar.Services.Identity.API.Application.Commands.Usuarios;

[NoTransaction, RetryOnException(VersionConcurrency = true, Retries = 2)]
public class RecuperarSenhaCommandHandler : IdentityCommandHandler<RecuperarSenhaCommand>
{
    public RecuperarSenhaCommandHandler(IDbSession session, IEnumerable<IIsRepository> repositories) : base(session, repositories)
    {
    }

    protected override async Task HandleAsync(RecuperarSenhaCommand cmd, CancellationToken ct)
    {
        var usuario = await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioId!.ToObjectId(), ct);
        if (usuario == null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);

        usuario.RecuperarSenha(cmd.Token!, cmd.Senha!, out long previousVersion);
        await UsuarioRepository.ReplaceOneAsync(usuario, previousVersion, ct).CheckModified();
    }
}
