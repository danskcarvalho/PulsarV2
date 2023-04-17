using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

[RequiresCausalConsistency]
public class RemoverGrupoCH : IdentityCommandHandler<RemoverGrupoCmd, CommandResult>
{
    public RemoverGrupoCH(IdentityCommandHandlerContext<RemoverGrupoCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(RemoverGrupoCmd cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId());
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(ExceptionKey.GrupoNaoEncontrado);
        grupo.Remover(cmd.UsuarioLogadoId!.ToObjectId());
        await GrupoRepository.ReplaceOneAsync(grupo);
        return new CommandResult(Session.ConsistencyToken);
    }
}
