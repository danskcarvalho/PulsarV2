namespace Pulsar.Services.Identity.Domain.Events.Usuarios;

public class UsuariosModificadosNonBasicDomainEvent : INotification
{
    public ObjectId UsuarioLogadoId { get; set; }
    public List<ObjectId> UsuariosIds { get; set; }
    public ChangeEvent Modificacao { get; set; }

    public UsuariosModificadosNonBasicDomainEvent(ObjectId usuarioLogadoId, List<ObjectId> usuariosIds, ChangeEvent modificacao)
    {
        UsuarioLogadoId = usuarioLogadoId;
        UsuariosIds = usuariosIds;
        Modificacao = modificacao;
    }
}
