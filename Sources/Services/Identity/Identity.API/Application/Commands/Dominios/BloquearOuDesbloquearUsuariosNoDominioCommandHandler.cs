using Pulsar.Services.Identity.Contracts.Commands.Dominios;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Dominios;

public class BloquearOuDesbloquearUsuariosNoDominioCommandHandler : IdentityCommandHandler<BloquearOuDesbloquearUsuariosNoDominioCommand, CommandResult>
{
    public BloquearOuDesbloquearUsuariosNoDominioCommandHandler(IdentityCommandHandlerContext<BloquearOuDesbloquearUsuariosNoDominioCommand, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(BloquearOuDesbloquearUsuariosNoDominioCommand cmd, CancellationToken ct)
    {
        var dominio = await DominioRepository.FindOneByIdAsync(cmd.DominioId!.ToObjectId(), ct);
        if (dominio == null)
            throw new IdentityDomainException(ExceptionKey.DominioNaoEncontrado);
        if (!await UsuarioRepository.AllExistsAsync(cmd.UsuarioIds!.Select(u => u.ToObjectId()), ct))
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
        dominio.BloquearOuDesbloquearUsuarios(cmd.UsuarioLogadoId!.ToObjectId(), cmd.UsuarioIds!.Select(uid => uid.ToObjectId()).ToList(), cmd.Bloquear);
        await DominioRepository.ReplaceOneAsync(dominio, ct: ct);
        return new CommandResult(Session.ConsistencyToken);
    }
}
