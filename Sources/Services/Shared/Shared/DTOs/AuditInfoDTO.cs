namespace Pulsar.Services.Shared.DTOs;

public class AuditInfoDTO
{
    /// <summary>
    /// Id do usuário criador.
    /// </summary>
    public string? CriadoPorUsuarioId { get; set; }
    /// <summary>
    /// Data da criação.
    /// </summary>
    public DateTime? CriadoEm { get; set; }
    /// <summary>
    /// Id do usuário que editou.
    /// </summary>
    public string? EditadoPorUsuarioId { get; set; }
    /// <summary>
    /// Data da edição.
    /// </summary>
    public DateTime? EditadoEm { get; set; }
    /// <summary>
    /// Id do usuário que removeu.
    /// </summary>
    public string? RemovidoPorUsuarioId { get; set; }
    /// <summary>
    /// Data da remoção.
    /// </summary>
    public DateTime? RemovidoEm { get; set; }
    /// <summary>
    /// Id do usuário que removeu.
    /// </summary>
    public string? EscondidoPorUsuarioId { get; set; }
    /// <summary>
    /// Data da remoção.
    /// </summary>
    public DateTime? EscondidoEm { get; set; }
}
