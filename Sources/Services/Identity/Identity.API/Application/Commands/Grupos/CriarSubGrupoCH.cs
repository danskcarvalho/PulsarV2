using Pulsar.Services.Identity.Contracts.Commands.Grupos;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

public class CriarSubGrupoCH : IdentityCommandHandler<CriarSubGrupoCmd, CreatedCommandResult>
{
    public CriarSubGrupoCH(IdentityCommandHandlerContext<CriarSubGrupoCmd, CreatedCommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CreatedCommandResult> HandleAsync(CriarSubGrupoCmd cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId());
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(ExceptionKey.GrupoNaoEncontrado);
        var subgrupoId = grupo.CriarSubGrupo(cmd.UsuarioLogadoId!.ToObjectId(), cmd.Nome!, cmd.PermissoesDominio!, cmd.PermissoesEstabelecimento!);
        await GrupoRepository.ReplaceOneAsync(grupo);
        return new CreatedCommandResult(subgrupoId.ToString());
    }
}
