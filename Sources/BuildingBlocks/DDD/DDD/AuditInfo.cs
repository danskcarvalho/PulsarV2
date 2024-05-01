using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.DDD;

public class AuditInfo : ValueObject
{
    [TrackChanges]
    public ObjectId? CriadoPorUsuarioId { get; }
    [TrackChanges]
    public DateTime CriadoEm { get; }
    [TrackChanges]
    public ObjectId? EditadoPorUsuarioId { get; }
    [TrackChanges]
    public DateTime? EditadoEm { get; }
    [TrackChanges]
    public ObjectId? RemovidoPorUsuarioId { get; }
    [TrackChanges]
    public DateTime? RemovidoEm { get; }
    [TrackChanges]
    public ObjectId? EscondidoPorUsuarioId { get; }
    [TrackChanges]
    public DateTime? EscondidoEm { get; }

    [BsonConstructor(nameof(CriadoPorUsuarioId), nameof(CriadoEm), nameof(EditadoPorUsuarioId), nameof(EditadoEm), nameof(RemovidoPorUsuarioId), nameof(RemovidoEm),
        nameof(EscondidoPorUsuarioId), nameof(EscondidoEm))]
    public AuditInfo(ObjectId? criadoPorUsuarioId, DateTime? criadoEm = null, ObjectId? editadoPorUsuarioId = null, DateTime? editadoEm = null, 
        ObjectId? removidoPorUsuarioId = null, DateTime? removidoEm = null, ObjectId? escondidoPorUsuarioId = null, DateTime? escondidoEm = null)
    {
        CriadoPorUsuarioId = criadoPorUsuarioId;
        CriadoEm = criadoEm ?? DateTime.UtcNow;
        EditadoPorUsuarioId = editadoPorUsuarioId;
        EditadoEm = editadoEm;
        RemovidoPorUsuarioId = removidoPorUsuarioId;
        RemovidoEm = removidoEm;
        EscondidoPorUsuarioId = escondidoPorUsuarioId;
        EscondidoEm = escondidoEm;
    }

    public AuditInfo EditadoPor(ObjectId editadoPorUsuarioId, DateTime? editadoEm = null)
    {
        return new AuditInfo(CriadoPorUsuarioId, CriadoEm, editadoPorUsuarioId, editadoEm ?? DateTime.UtcNow, RemovidoPorUsuarioId, RemovidoEm, EscondidoPorUsuarioId, EscondidoEm);
    }

    public AuditInfo RemovidoPor(ObjectId removidoPorUsuarioId, DateTime? removidoEm = null)
    {
        return new AuditInfo(CriadoPorUsuarioId, CriadoEm, EditadoPorUsuarioId, EditadoEm, removidoPorUsuarioId, removidoEm ?? DateTime.UtcNow, EscondidoPorUsuarioId, EscondidoEm);
    }

    public AuditInfo EscondidoPor(ObjectId escondidoPorUsuarioId, DateTime? escondidoEm = null)
    {
        return new AuditInfo(CriadoPorUsuarioId, CriadoEm, EditadoPorUsuarioId, EditadoEm, RemovidoPorUsuarioId, RemovidoEm, escondidoPorUsuarioId, escondidoEm ?? DateTime.UtcNow);
    }

    public AuditInfo MostradoPor()
    {
        return new AuditInfo(CriadoPorUsuarioId, CriadoEm, EditadoPorUsuarioId, EditadoEm, RemovidoPorUsuarioId, RemovidoEm, null, null);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CriadoPorUsuarioId;
        yield return CriadoEm;
        yield return EditadoPorUsuarioId;
        yield return EditadoEm;
        yield return RemovidoPorUsuarioId;
        yield return RemovidoEm;
        yield return EscondidoPorUsuarioId;
        yield return EscondidoEm;
    }
}
