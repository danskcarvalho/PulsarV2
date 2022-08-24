using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Utils
{
    public static class AuditInfoDTOExtensions
    {
        public static AuditInfoDTO ToDTO(this AuditInfo ai) => new AuditInfoDTO()
        {
            CriadoEm = ai.CriadoEm,
            CriadoPorUsuarioId = ai.CriadoPorUsuarioId?.ToString(),
            EditadoEm = ai.EditadoEm,
            EditadoPorUsuarioId = ai.EditadoPorUsuarioId?.ToString(),
            RemovidoEm = ai.RemovidoEm,
            RemovidoPorUsuarioId = ai.RemovidoPorUsuarioId?.ToString()
        };
    }
}
