using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

[RequiresCausalConsistency]
public class RemoverUsuariosSubGrupoCommandHandler : IdentityCommandHandler<RemoverUsuariosSubGrupoCommand, CommandResult>
{
    public RemoverUsuariosSubGrupoCommandHandler(IdentityCommandHandlerContext<RemoverUsuariosSubGrupoCommand, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(RemoverUsuariosSubGrupoCommand cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId(), ct);
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(ExceptionKey.GrupoNaoEncontrado);
        if (!await UsuarioRepository.AllExistsAsync(cmd.UsuarioIds!.Select(u => u.ToObjectId())))
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);

        grupo.RemoverUsuariosEmSubGrupo(cmd.UsuarioLogadoId!.ToObjectId(), cmd.SubGrupoId!.ToObjectId(), cmd.UsuarioIds!.Select(x => x.ToObjectId()).ToList());
        return new CommandResult(Session.ConsistencyToken);
    }
}
