namespace Pulsar.Services.Identity.Domain.Events.Grupos
{
    public class NumUsuariosEmGrupoModificadoDomainEvent : INotification
    {
        public ObjectId UsuarioLogadoId { get; set; }
        public ObjectId GrupoId { get; set; }
        public int DeltaNumUsuarios { get; set; }

        public NumUsuariosEmGrupoModificadoDomainEvent(ObjectId usuarioLogadoId, ObjectId grupoId, int deltaNumUsuarios)
        {
            UsuarioLogadoId = usuarioLogadoId;
            GrupoId = grupoId;
            DeltaNumUsuarios = deltaNumUsuarios;
        }
    }
}
