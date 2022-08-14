namespace Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

public class UsuarioGrupo : ValueObject
{
    public ObjectId DominioId { get; }
    public ObjectId GrupoId { get; }
    public ObjectId SubGrupoId { get; }


    [BsonConstructor(nameof(DominioId), nameof(GrupoId), nameof(SubGrupoId))]
    public UsuarioGrupo(ObjectId dominioId, ObjectId grupoId, ObjectId subGrupoId)
    {
        GrupoId = grupoId;
        SubGrupoId = subGrupoId;
        DominioId = dominioId;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DominioId;
        yield return GrupoId;
        yield return SubGrupoId;
    }
}
