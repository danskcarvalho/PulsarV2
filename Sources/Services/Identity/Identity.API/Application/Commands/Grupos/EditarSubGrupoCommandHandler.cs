using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Identity.Domain.Aggregates.Grupos;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

public class EditarSubGrupoCommandHandler : IdentityCommandHandler<EditarSubGrupoCommand, CommandResult>
{
    public EditarSubGrupoCommandHandler(IdentityCommandHandlerContext<EditarSubGrupoCommand, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(EditarSubGrupoCommand cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId(), ct);
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(ExceptionKey.GrupoNaoEncontrado); 
        grupo.EditarSubGrupo(cmd.UsuarioLogadoId!.ToObjectId(), cmd.SubGrupoId!.ToObjectId(), cmd.Nome!, cmd.PermissoesDominio!, cmd.PermissoesEstabelecimento!);
        await GrupoRepository.ReplaceOneAsync(grupo, ct: ct);
        return new CommandResult(Session.ConsistencyToken);
    }
}
