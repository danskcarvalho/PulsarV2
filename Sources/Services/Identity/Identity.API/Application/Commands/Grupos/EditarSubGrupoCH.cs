using Pulsar.Services.Identity.Contracts.Commands.Grupos;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

public class EditarSubGrupoCH : IdentityCommandHandler<EditarSubGrupoCmd, CommandResult>
{
    public EditarSubGrupoCH(IdentityCommandHandlerContext<EditarSubGrupoCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(EditarSubGrupoCmd cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId());
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(IdentityExceptionKey.GrupoNaoEncontrado); 
        grupo.EditarSubGrupo(cmd.UsuarioLogadoId!.ToObjectId(), cmd.SubGrupoId!.ToObjectId(), cmd.Nome!, cmd.PermissoesDominio!, cmd.PermissoesEstabelecimento!);
		await GrupoRepository.ReplaceOneAsync(grupo);
		return new CommandResult(Session.ConsistencyToken);
    }
}
