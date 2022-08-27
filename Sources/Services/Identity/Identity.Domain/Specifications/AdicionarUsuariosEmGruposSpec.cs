using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Domain.Specifications;

public class AdicionarUsuariosEmGruposSpec : IUpdateSpecification<Usuario>
{
    public AdicionarUsuariosEmGruposSpec(ObjectId usuarioLogadoId, ObjectId dominioId, ObjectId grupoId, ObjectId subGrupoId, List<ObjectId> usuarioIds)
    {
        UsuarioLogadoId = usuarioLogadoId;
        DominioId = dominioId;
        GrupoId = grupoId;
        SubGrupoId = subGrupoId;
        UsuarioIds = usuarioIds;
    }

    public ObjectId UsuarioLogadoId { get; }
    public ObjectId DominioId { get; }
    public ObjectId GrupoId { get; }
    public ObjectId SubGrupoId { get; }
    public List<ObjectId> UsuarioIds { get; }

    public UpdateSpecification<Usuario> GetSpec()
    {
        return Update
               .Where<Usuario>(u => UsuarioIds.Contains(u.Id) && !u.Grupos.Any(g => g.GrupoId == GrupoId && g.SubGrupoId == SubGrupoId))
               .AddToSet(u => u.Grupos, new UsuarioGrupo(DominioId, GrupoId, SubGrupoId))
               .Set(u => u.AuditInfo.EditadoPorUsuarioId, UsuarioLogadoId)
               .Set(u => u.AuditInfo.EditadoEm, DateTime.UtcNow)
               .Inc(u => u.Version, 1)
               .Build();
    }
}
