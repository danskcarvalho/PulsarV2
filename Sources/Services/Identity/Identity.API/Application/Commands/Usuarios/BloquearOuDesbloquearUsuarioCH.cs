using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Usuarios;

public class BloquearOuDesbloquearUsuarioCH : IdentityCommandHandler<BloquearOuDesbloquearUsuarioCmd, CommandResult>
{
    public BloquearOuDesbloquearUsuarioCH(IdentityCommandHandlerContext<BloquearOuDesbloquearUsuarioCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(BloquearOuDesbloquearUsuarioCmd cmd, CancellationToken ct)
    {
        var usuario = await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioId!.ToObjectId());
        if (usuario == null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
        usuario.BloquearOuDesbloquear(cmd.UsuarioLogadoId.ToObjectId(), cmd.Bloquear);
        await UsuarioRepository.ReplaceOneAsync(usuario);
        return new CommandResult(Session.ConsistencyToken);
    }
}
