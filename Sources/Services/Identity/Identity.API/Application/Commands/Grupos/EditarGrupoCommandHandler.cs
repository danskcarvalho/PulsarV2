using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

public class EditarGrupoCommandHandler : IdentityCommandHandler<EditarGrupoCommand, CommandResult>
{
    public EditarGrupoCommandHandler(IdentityCommandHandlerContext<EditarGrupoCommand, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(EditarGrupoCommand cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId(), ct);
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(ExceptionKey.GrupoNaoEncontrado);
        grupo.Editar(cmd.UsuarioLogadoId!.ToObjectId(), cmd.Nome!);
        return new CommandResult(Session.ConsistencyToken);
    }
}
