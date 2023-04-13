namespace Pulsar.Services.Identity.Domain.Events.Grupos;

public class UsuariosAdicionadosOuRemovidosEmGrupoDE : INotification
{
    public ObjectId UsuarioLogadoId { get; set; }
    public ObjectId GrupoId { get; set; }
    public ObjectId SubGrupoId { get; set; }
    public ObjectId DominioId { get; set; }
    public bool Remocao { get; set; }
    public List<ObjectId> UsuarioIds { get; set; }

    public UsuariosAdicionadosOuRemovidosEmGrupoDE(ObjectId usuarioLogadoId, ObjectId grupoId, ObjectId subGrupoId, ObjectId dominioId, bool remocao, List<ObjectId> usuarioIds)
    {
        UsuarioLogadoId = usuarioLogadoId;
        GrupoId = grupoId;
        SubGrupoId = subGrupoId;
        DominioId = dominioId;
        Remocao = remocao;
        UsuarioIds = usuarioIds;
    }
}
