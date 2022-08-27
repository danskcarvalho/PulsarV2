using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

public class RemoverGrupoCommandHandler : IdentityCommandHandler<RemoverGrupoCommand, CommandResult>
{
    public RemoverGrupoCommandHandler(IdentityCommandHandlerContext<RemoverGrupoCommand, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(RemoverGrupoCommand cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId(), ct);
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(ExceptionKey.GrupoNaoEncontrado);
        grupo.Remover(cmd.UsuarioLogadoId!.ToObjectId());
        await GrupoRepository.ReplaceOneAsync(grupo, ct: ct);
        return new CommandResult(Session.ConsistencyToken);
    }
}
