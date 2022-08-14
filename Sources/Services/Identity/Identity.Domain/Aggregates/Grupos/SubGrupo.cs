namespace Pulsar.Services.Identity.Domain.Aggregates.Grupos;

public class SubGrupo : AggregateComponent
{
    public ObjectId Id { get; set; }
    public string Nome { get; set; }
    public HashSet<PermissoesGerais> PermissoesGerais { get; private set; }
    public List<SubGrupoPermissoesEstabelecimento> PermissoesEstabelecimentos { get; private set; }

    [BsonConstructor(nameof(Id), nameof(Nome), nameof(PermissoesGerais), nameof(PermissoesEstabelecimentos))]
    public SubGrupo(ObjectId id, string nome, IEnumerable<PermissoesGerais>? permissoesGerais = null, IEnumerable<SubGrupoPermissoesEstabelecimento>? permissoesEstabelecimentos = null)
    {
        Id = id;
        Nome = nome;
        PermissoesGerais = permissoesGerais != null ? new HashSet<PermissoesGerais>(permissoesGerais) : new HashSet<PermissoesGerais>();
        PermissoesEstabelecimentos = permissoesEstabelecimentos != null ? new List<SubGrupoPermissoesEstabelecimento>(permissoesEstabelecimentos) : new List<SubGrupoPermissoesEstabelecimento>();
    }
}
