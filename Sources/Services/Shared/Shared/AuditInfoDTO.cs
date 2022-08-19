namespace Pulsar.Services.Shared;

public class AuditInfoDTO
{
    public string? CriadoPorUsuarioId { get; set; }
    public DateTime? CriadoEm { get; set; }
    public string? EditadoPorUsuarioId { get; set; }
    public DateTime? EditadoEm { get; set; }
    public string? RemovidoPorUsuarioId { get; set; }
    public DateTime? RemovidoEm { get; set; }
}
