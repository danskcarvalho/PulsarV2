using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

public class RemoverUsuariosSubGrupoCommandHandler : IdentityCommandHandler<AdicionarUsuariosSubGrupoCommand, CommandResult>
{
    public RemoverUsuariosSubGrupoCommandHandler(IdentityCommandHandlerContext<AdicionarUsuariosSubGrupoCommand, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(AdicionarUsuariosSubGrupoCommand cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId(), ct);
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(ExceptionKey.GrupoNaoEncontrado);
        grupo.RemoverUsuariosEmSubGrupo(cmd.UsuarioLogadoId!.ToObjectId(), cmd.SubGrupoId!.ToObjectId(), cmd.UsuarioIds!.Select(x => x.ToObjectId()).ToList());
        return new CommandResult(Session.ConsistencyToken);
    }
}
