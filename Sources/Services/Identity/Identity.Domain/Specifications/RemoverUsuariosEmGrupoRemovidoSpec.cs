using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Domain.Specifications;

public class RemoverUsuariosEmGrupoRemovidoSpec : IUpdateSpecification<Usuario>
{
    public RemoverUsuariosEmGrupoRemovidoSpec(ObjectId grupoId, List<ObjectId>? subGrupoIds, ObjectId usuarioLogadoId)
    {
        GrupoId = grupoId;
        SubGrupoIds = subGrupoIds;
        UsuarioLogadoId = usuarioLogadoId;
    }

    public ObjectId GrupoId { get; }
    public List<ObjectId>? SubGrupoIds { get; }
    public ObjectId UsuarioLogadoId { get; }

    public UpdateSpecification<Usuario> GetSpec()
    {
        if (SubGrupoIds == null || SubGrupoIds.Count == 0)
        {
            return Update
                .Where<Usuario>(u => u.Grupos.Any(g => g.GrupoId == GrupoId))
                .PullFilter(u => u.Grupos, g => g.GrupoId == GrupoId)
                .Set(u => u.AuditInfo.EditadoPorUsuarioId, UsuarioLogadoId)
                .Set(u => u.AuditInfo.EditadoEm, DateTime.UtcNow)
                .Inc(u => u.Version, 1)
                .Build();
        }
        else
        {
            return Update
                .Where<Usuario>(u => u.Grupos.Any(g => g.GrupoId == GrupoId && SubGrupoIds.Contains(g.SubGrupoId)))
                .PullFilter(u => u.Grupos, g => g.GrupoId == GrupoId && SubGrupoIds.Contains(g.SubGrupoId))
                .Set(u => u.AuditInfo.EditadoPorUsuarioId, UsuarioLogadoId)
                .Set(u => u.AuditInfo.EditadoEm, DateTime.UtcNow)
                .Inc(u => u.Version, 1)
                .Build();
        }
    }
}
