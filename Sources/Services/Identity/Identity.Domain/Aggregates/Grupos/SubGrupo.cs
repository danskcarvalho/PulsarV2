using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Contracts.Enumerations;

namespace Pulsar.Services.Identity.Domain.Aggregates.Grupos;

public class SubGrupo : AggregateComponent
{
    public ObjectId SubGrupoId { get; set; }
    public string Nome { get; set; }
    [BsonRepresentation(BsonType.String)]
    public HashSet<PermissoesDominio> PermissoesDominio { get; private set; }
    public List<SubGrupoPermissoesEstabelecimento> PermissoesEstabelecimentos { get; private set; }
    public int NumUsuarios { get; set; }

    [BsonConstructor(nameof(SubGrupoId), nameof(Nome), nameof(PermissoesDominio), nameof(PermissoesEstabelecimentos))]
    public SubGrupo(ObjectId subGrupoId, string nome, IEnumerable<PermissoesDominio>? permissoesDominio = null, IEnumerable<SubGrupoPermissoesEstabelecimento>? permissoesEstabelecimentos = null)
    {
        SubGrupoId = subGrupoId;
        Nome = nome;
        PermissoesDominio = permissoesDominio != null ? new HashSet<PermissoesDominio>(permissoesDominio) : new HashSet<PermissoesDominio>();
        PermissoesEstabelecimentos = permissoesEstabelecimentos != null ? new List<SubGrupoPermissoesEstabelecimento>(permissoesEstabelecimentos) : new List<SubGrupoPermissoesEstabelecimento>();
    }

    public void Editar(string nome, List<PermissoesDominio> permissoesDominios, List<EditarSubGrupoCmd.PermissoesEstabelecimentoOuRede> permissoesEstabelecimentoOuRedes)
    {
        nome = nome.Trim();
        this.Nome = nome;
        this.PermissoesDominio.Clear();
        this.PermissoesDominio.UnionWith(permissoesDominios);
        this.PermissoesEstabelecimentos.Clear();
        this.PermissoesEstabelecimentos.AddRange(permissoesEstabelecimentoOuRedes.Select(pe => new SubGrupoPermissoesEstabelecimento(new Seletor(pe.EstabelecimentoId?.ToObjectId(), pe.RedeEstabelecimentosId?.ToObjectId()), pe.Permissoes!)));
    }
}
