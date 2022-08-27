namespace Pulsar.Services.Identity.Domain.Events.Grupos
{
    public class NumUsuariosEmGrupoModificadoDomainEvent : INotification
    {
        public ObjectId UsuarioLogadoId { get; set; }
        public ObjectId GrupoId { get; set; }
        public List<ObjectId>? SubGrupoIds { get; set; }

        public NumUsuariosEmGrupoModificadoDomainEvent(ObjectId usuarioLogadoId, ObjectId grupoId, List<ObjectId>? subGrupoIds)
        {
            UsuarioLogadoId = usuarioLogadoId;
            GrupoId = grupoId;
            SubGrupoIds = subGrupoIds;
        }
    }
}
