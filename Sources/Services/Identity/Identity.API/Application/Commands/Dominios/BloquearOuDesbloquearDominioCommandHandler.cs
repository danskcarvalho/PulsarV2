using Pulsar.Services.Identity.Contracts.Commands.Dominios;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Dominios;

public class BloquearOuDesbloquearDominioCommandHandler : IdentityCommandHandler<BloquearOuDesbloquearDominioCommand, CommandResult>
{
    public BloquearOuDesbloquearDominioCommandHandler(IdentityCommandHandlerContext<BloquearOuDesbloquearDominioCommand, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(BloquearOuDesbloquearDominioCommand cmd, CancellationToken ct)
    {
        var dominio = await DominioRepository.FindOneByIdAsync(cmd.DominioId!.ToObjectId(), ct);
        if (dominio == null)
            throw new IdentityDomainException(ExceptionKey.DominioNaoEncontrado);
        dominio.BloquearOuDesbloquear(cmd.UsuarioLogadoId!.ToObjectId(), cmd.Bloquear);
        await DominioRepository.ReplaceOneAsync(dominio, ct: ct);
        return new CommandResult(Session.ConsistencyToken);
    }
}
