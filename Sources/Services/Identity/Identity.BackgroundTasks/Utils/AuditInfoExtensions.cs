namespace Pulsar.Services.Identity.BackgroundTasks.Utils;

public static class AuditInfoExtensions
{
    public static AuditInfo ToDomainObject(this AuditInfoDTO dto)
    {
        return new AuditInfo(dto.CriadoPorUsuarioId?.ToObjectId(), dto.CriadoEm, dto.EditadoPorUsuarioId?.ToObjectId(), dto.EditadoEm, dto.RemovidoPorUsuarioId?.ToObjectId(), dto.RemovidoEm);
    }
}
