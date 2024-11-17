using Pulsar.Services.Identity.Contracts.Commands.Grupos;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

public class EditarGrupoCH : IdentityCommandHandler<EditarGrupoCmd, CommandResult>
{
    public EditarGrupoCH(IdentityCommandHandlerContext<EditarGrupoCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(EditarGrupoCmd cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId());
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(IdentityExceptionKey.GrupoNaoEncontrado);
        grupo.Editar(cmd.UsuarioLogadoId!.ToObjectId(), cmd.Nome!);
		await GrupoRepository.ReplaceOneAsync(grupo);
		return new CommandResult(Session.ConsistencyToken);
    }
}
