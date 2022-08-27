using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

[NoTransaction]
public class CriarGrupoCommandHandler : IdentityCommandHandler<CriarGrupoCommand, CreatedCommandResult>
{
    public CriarGrupoCommandHandler(IdentityCommandHandlerContext<CriarGrupoCommand, CreatedCommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CreatedCommandResult> HandleAsync(CriarGrupoCommand cmd, CancellationToken ct)
    {
        var grupo = new Grupo(ObjectId.GenerateNewId(), cmd.DominioId!.ToObjectId(), cmd.Nome!, new AuditInfo(cmd.UsuarioLogadoId!.ToObjectId()), new List<SubGrupo>());
        grupo.Criar();
        await GrupoRepository.InsertOneAsync(grupo, ct);
        return new CreatedCommandResult(grupo.Id.ToString(), Session.ConsistencyToken);
    }
}
