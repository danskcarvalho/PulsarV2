using Pulsar.Services.Identity.Domain.Aggregates.Grupos;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Domain.Specifications;

public class AtualizarNumUsuariosEmGrupoSpec : IUpdateSpecification<Grupo>
{
    public AtualizarNumUsuariosEmGrupoSpec(ObjectId usuarioLogadoId, ObjectId grupoId, int numUsuarios)
    {
        UsuarioLogadoId = usuarioLogadoId;
        GrupoId = grupoId;
        NumUsuarios = numUsuarios;
    }

    public ObjectId UsuarioLogadoId { get; }
    public ObjectId GrupoId { get; }
    public int NumUsuarios { get; }

    public UpdateSpecification<Grupo> GetSpec()
    {
        return Update
                .Where<Grupo>(u => u.Id == GrupoId)
                .Set(u => u.AuditInfo.EditadoPorUsuarioId, UsuarioLogadoId)
                .Set(u => u.AuditInfo.EditadoEm, DateTime.UtcNow)
                .Inc(u => u.NumUsuarios, NumUsuarios)
                .Inc(u => u.Version, 1)
                .Build();
    }
}
