using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Domain.Specifications.Usuarios;

public class BloquearOuDesbloquearUsuariosEmDominioSpec : IUpdateSpecification<Usuario>
{
    public BloquearOuDesbloquearUsuariosEmDominioSpec(ObjectId usuarioLogadoId, ObjectId dominioId, List<ObjectId> usuariosIds, bool bloquear)
    {
        UsuarioLogadoId = usuarioLogadoId;
        DominioId = dominioId;
        UsuariosIds = usuariosIds;
        Bloquear = bloquear;
    }

    public ObjectId UsuarioLogadoId { get; set; }
    public ObjectId DominioId { get; set; }
    public List<ObjectId> UsuariosIds { get; set; }
    public bool Bloquear { get; set; }

    public UpdateSpecification<Usuario> GetSpec()
    {
        if (Bloquear)
        {
            return Update.Where<Usuario>(u => UsuariosIds.Contains(u.Id))
                .AddToSet(u => u.DominiosBloqueados, DominioId)
                .Set(u => u.AuditInfo.EditadoPorUsuarioId, UsuarioLogadoId)
                .Set(u => u.AuditInfo.EditadoEm, DateTime.UtcNow)
                .Inc(u => u.Version, 1)
                .Build();
        }
        else
        {
            return Update.Where<Usuario>(u => UsuariosIds.Contains(u.Id))
                .Pull(u => u.DominiosBloqueados, DominioId)
                .Set(u => u.AuditInfo.EditadoPorUsuarioId, UsuarioLogadoId)
                .Set(u => u.AuditInfo.EditadoEm, DateTime.UtcNow)
                .Inc(u => u.Version, 1)
                .Build();
        }
    }
}
