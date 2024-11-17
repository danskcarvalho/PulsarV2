using Pulsar.Services.Identity.Contracts.Commands.Dominios;

namespace Pulsar.Services.Identity.API.Application.Commands.Dominios;

public class BloquearOuDesbloquearUsuariosNoDominioCH : IdentityCommandHandler<BloquearOuDesbloquearUsuariosNoDominioCmd, CommandResult>
{
    public BloquearOuDesbloquearUsuariosNoDominioCH(IdentityCommandHandlerContext<BloquearOuDesbloquearUsuariosNoDominioCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(BloquearOuDesbloquearUsuariosNoDominioCmd cmd, CancellationToken ct)
    {
        var dominio = await DominioRepository.FindOneByIdAsync(cmd.DominioId!.ToObjectId());
        if (dominio == null)
            throw new IdentityDomainException(IdentityExceptionKey.DominioNaoEncontrado);
        if (!await UsuarioRepository.AllExistsAsync(cmd.UsuarioIds!.Select(u => u.ToObjectId())))
            throw new IdentityDomainException(IdentityExceptionKey.UsuarioNaoEncontrado);
        dominio.BloquearOuDesbloquearUsuarios(cmd.UsuarioLogadoId!.ToObjectId(), cmd.UsuarioIds!.Select(uid => uid.ToObjectId()).ToList(), cmd.Bloquear);
        await DominioRepository.ReplaceOneAsync(dominio);
        return new CommandResult(Session.ConsistencyToken);
    }
}
