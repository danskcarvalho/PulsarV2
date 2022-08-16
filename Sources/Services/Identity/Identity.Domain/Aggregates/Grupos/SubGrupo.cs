namespace Pulsar.Services.Identity.Domain.Aggregates.Grupos;

public class SubGrupo : AggregateComponent
{
    public ObjectId Id { get; set; }
    public string Nome { get; set; }
    public HashSet<PermissoesDominio> PermissoesDominio { get; private set; }
    public List<SubGrupoPermissoesEstabelecimento> PermissoesEstabelecimentos { get; private set; }

    [BsonConstructor(nameof(Id), nameof(Nome), nameof(PermissoesDominio), nameof(PermissoesEstabelecimentos))]
    public SubGrupo(ObjectId id, string nome, IEnumerable<PermissoesDominio>? permissoesDominio = null, IEnumerable<SubGrupoPermissoesEstabelecimento>? permissoesEstabelecimentos = null)
    {
        Id = id;
        Nome = nome;
        PermissoesDominio = permissoesDominio != null ? new HashSet<PermissoesDominio>(permissoesDominio) : new HashSet<PermissoesDominio>();
        PermissoesEstabelecimentos = permissoesEstabelecimentos != null ? new List<SubGrupoPermissoesEstabelecimento>(permissoesEstabelecimentos) : new List<SubGrupoPermissoesEstabelecimento>();
    }
}
