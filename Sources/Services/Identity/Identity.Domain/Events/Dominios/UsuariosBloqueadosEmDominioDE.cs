namespace Pulsar.Services.Identity.Domain.Events.Dominios;

public class UsuariosBloqueadosEmDominioDE : INotification
{
    public ObjectId UsuarioLogadoId { get; set; }
    public ObjectId DominioId { get; set; }
    public List<ObjectId> UsuariosIds { get; set; }
    public bool Bloquear { get; set; }

    public UsuariosBloqueadosEmDominioDE(ObjectId usuarioLogadoId, ObjectId dominioId, List<ObjectId> usuariosIds, bool bloquear)
    {
        UsuarioLogadoId = usuarioLogadoId;
        DominioId = dominioId;
        UsuariosIds = usuariosIds;
        Bloquear = bloquear;
    }
}
