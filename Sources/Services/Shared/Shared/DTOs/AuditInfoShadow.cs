using MongoDB.Bson;

namespace Pulsar.Services.Shared.DTOs;

public class AuditInfoShadow
{
    /// <summary>
    /// Id do usuário criador.
    /// </summary>
    public ObjectId? CriadoPorUsuarioId { get; set; }
    /// <summary>
    /// Data da criação.
    /// </summary>
    public DateTime CriadoEm { get; set; }
    /// <summary>
    /// Id do usuário que editou.
    /// </summary>
    public ObjectId? EditadoPorUsuarioId { get; set; }
    /// <summary>
    /// Data da edição.
    /// </summary>
    public DateTime? EditadoEm { get; set; }
    /// <summary>
    /// Id do usuário que removeu.
    /// </summary>
    public ObjectId? RemovidoPorUsuarioId { get; set; }
    /// <summary>
    /// Data da remoção.
    /// </summary>
    public DateTime? RemovidoEm { get; set; }
    /// <summary>
    /// Id do usuário que removeu.
    /// </summary>
    public ObjectId? EscondidoPorUsuarioId { get; set; }
    /// <summary>
    /// Data da remoção.
    /// </summary>
    public DateTime? EscondidoEm { get; set; }
}
