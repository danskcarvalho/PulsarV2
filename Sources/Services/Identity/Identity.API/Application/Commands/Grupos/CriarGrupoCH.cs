using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

[NoTransaction]
public class CriarGrupoCH : IdentityCommandHandler<CriarGrupoCmd, CreatedCommandResult>
{
    public CriarGrupoCH(IdentityCommandHandlerContext<CriarGrupoCmd, CreatedCommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CreatedCommandResult> HandleAsync(CriarGrupoCmd cmd, CancellationToken ct)
    {
        var grupo = new Grupo(ObjectId.GenerateNewId(), cmd.DominioId!.ToObjectId(), cmd.Nome!, new AuditInfo(cmd.UsuarioLogadoId!.ToObjectId()), new List<SubGrupo>());
        await grupo.Criar();
        return new CreatedCommandResult(grupo.Id.ToString(), Session.ConsistencyToken);
    }
}
