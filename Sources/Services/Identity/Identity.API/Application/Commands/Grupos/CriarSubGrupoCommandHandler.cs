using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

public class CriarSubGrupoCommandHandler : IdentityCommandHandler<CriarSubGrupoCommand, CreatedCommandResult>
{
    public CriarSubGrupoCommandHandler(IdentityCommandHandlerContext<CriarSubGrupoCommand, CreatedCommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CreatedCommandResult> HandleAsync(CriarSubGrupoCommand cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId(), ct);
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(ExceptionKey.GrupoNaoEncontrado);
        var subgrupoId = grupo.CriarSubGrupo(cmd.UsuarioLogadoId!.ToObjectId(), cmd.Nome!, cmd.PermissoesDominio!, cmd.PermissoesEstabelecimento!);
        await GrupoRepository.ReplaceOneAsync(grupo, ct: ct);
        return new CreatedCommandResult(subgrupoId.ToString());
    }
}
