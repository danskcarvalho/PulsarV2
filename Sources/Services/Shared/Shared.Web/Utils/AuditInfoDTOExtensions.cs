using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Shared.API.Utils
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
            RemovidoPorUsuarioId = ai.RemovidoPorUsuarioId?.ToString(),
            EscondidoEm = ai.EscondidoEm,
            EscondidoPorUsuarioId = ai.EscondidoPorUsuarioId?.ToString()
        };

        public static AuditInfoShadow ToShadow(this AuditInfo ai) => new AuditInfoShadow()
        {
            CriadoEm = ai.CriadoEm,
            CriadoPorUsuarioId = ai.CriadoPorUsuarioId,
            EditadoEm = ai.EditadoEm,
            EditadoPorUsuarioId = ai.EditadoPorUsuarioId,
            RemovidoEm = ai.RemovidoEm,
            RemovidoPorUsuarioId = ai.RemovidoPorUsuarioId,
            EscondidoEm = ai.EscondidoEm,
            EscondidoPorUsuarioId = ai.EscondidoPorUsuarioId
        };
    }
}
