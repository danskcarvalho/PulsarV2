using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Usuarios;

[NoTransaction, RetryOnException(VersionConcurrency = true, Retries = 2)]
public class MudarMinhaSenhaCH : IdentityCommandHandler<MudarMinhaSenhaCmd, CommandResult>
{
    public MudarMinhaSenhaCH(IdentityCommandHandlerContext<MudarMinhaSenhaCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(MudarMinhaSenhaCmd cmd, CancellationToken ct)
    {
        var usuario = await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioId!.ToObjectId(), ct);
        if (usuario == null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
        usuario.MudarMinhaSenha(cmd.SenhaAtual!, cmd.Senha!, out long previousVersion);
        await UsuarioRepository.ReplaceOneAsync(usuario, previousVersion, ct);
        return new CommandResult(Session.ConsistencyToken);
    }
}
