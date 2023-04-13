namespace Pulsar.Services.Identity.Domain.Events.Grupos
{
    public class NumUsuariosEmGrupoModificadoDE : INotification
    {
        public ObjectId UsuarioLogadoId { get; set; }
        public ObjectId GrupoId { get; set; }
        public List<ObjectId>? SubGrupoIds { get; set; }

        public NumUsuariosEmGrupoModificadoDE(ObjectId usuarioLogadoId, ObjectId grupoId, List<ObjectId>? subGrupoIds)
        {
            UsuarioLogadoId = usuarioLogadoId;
            GrupoId = grupoId;
            SubGrupoIds = subGrupoIds;
        }
    }
}
