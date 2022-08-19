namespace Pulsar.BuildingBlocks.DDD;

public class AuditInfo : ValueObject
{
    public ObjectId? CriadoPorUsuarioId { get; }
    public DateTime CriadoEm { get; }
    public ObjectId? EditadoPorUsuarioId { get; }
    public DateTime? EditadoEm { get; }
    public ObjectId? RemovidoPorUsuarioId { get; }
    public DateTime? RemovidoEm { get; }

    [BsonConstructor(nameof(CriadoPorUsuarioId), nameof(CriadoEm), nameof(EditadoPorUsuarioId), nameof(EditadoEm), nameof(RemovidoPorUsuarioId), nameof(RemovidoEm))]
    public AuditInfo(ObjectId? criadoPorUsuarioId, DateTime? criadoEm = null, ObjectId? editadoPorUsuarioId = null, DateTime? editadoEm = null, ObjectId? removidoPorUsuarioId = null, DateTime? removidoEm = null)
    {
        CriadoPorUsuarioId = criadoPorUsuarioId;
        CriadoEm = criadoEm ?? DateTime.UtcNow;
        EditadoPorUsuarioId = editadoPorUsuarioId;
        EditadoEm = editadoEm;
        RemovidoPorUsuarioId = removidoPorUsuarioId;
        RemovidoEm = removidoEm;
    }

    public AuditInfo EditadoPor(ObjectId editadoPorUsuarioId, DateTime? editadoEm = null)
    {
        return new AuditInfo(CriadoPorUsuarioId, CriadoEm, editadoPorUsuarioId, editadoEm ?? DateTime.UtcNow);
    }

    public AuditInfo RemovidoPor(ObjectId removidoPorUsuarioId, DateTime? removidoEm = null)
    {
        return new AuditInfo(CriadoPorUsuarioId, CriadoEm, EditadoPorUsuarioId, EditadoEm, removidoPorUsuarioId, removidoEm ?? DateTime.UtcNow);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CriadoPorUsuarioId;
        yield return CriadoEm;
        yield return EditadoPorUsuarioId;
        yield return EditadoEm;
        yield return RemovidoPorUsuarioId;
        yield return RemovidoEm;
    }
}
