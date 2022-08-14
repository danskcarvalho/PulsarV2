namespace Pulsar.Services.Identity.Domain.Aggregates.Convites;

public class ConviteGrupo : ValueObject
{
    public ObjectId GrupoId { get; }
    public ObjectId SubGrupoId { get; }


    [BsonConstructor(nameof(GrupoId), nameof(SubGrupoId))]
    public ConviteGrupo(ObjectId grupoId, ObjectId subGrupoId)
    {
        GrupoId = grupoId;
        SubGrupoId = subGrupoId;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return GrupoId;
        yield return SubGrupoId;
    }
}
