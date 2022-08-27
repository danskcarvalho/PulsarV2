using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

public class RemoverSubGrupoCommandHandler : IdentityCommandHandler<RemoverSubGrupoCommand, CommandResult>
{
    public RemoverSubGrupoCommandHandler(IdentityCommandHandlerContext<RemoverSubGrupoCommand, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(RemoverSubGrupoCommand cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId(), ct);
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(ExceptionKey.GrupoNaoEncontrado);
        grupo.RemoverSubGrupo(cmd.UsuarioLogadoId!.ToObjectId(), cmd.SubGrupoId!.ToObjectId());
        await GrupoRepository.ReplaceOneAsync(grupo, ct: ct);
        return new CommandResult(Session.ConsistencyToken);
    }
}
