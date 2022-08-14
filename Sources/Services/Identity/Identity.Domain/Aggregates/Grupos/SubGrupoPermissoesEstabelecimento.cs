namespace Pulsar.Services.Identity.Domain.Aggregates.Grupos;

public class SubGrupoPermissoesEstabelecimento : AggregateComponent
{
    public Seletor Seletor { get; private set; }
    public HashSet<PermissoesEstabelecimento> Permissoes { get; private set; }

    [BsonConstructor(nameof(Seletor), nameof(Permissoes))]
    public SubGrupoPermissoesEstabelecimento(Seletor seletor, IEnumerable<PermissoesEstabelecimento>? permissoes = null)
    {
        Seletor = seletor;
        Permissoes = permissoes != null ? new HashSet<PermissoesEstabelecimento>(permissoes) : new HashSet<PermissoesEstabelecimento>();
    }
}
