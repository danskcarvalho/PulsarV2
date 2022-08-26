using System.Text.Json.Serialization;

namespace Pulsar.Services.Identity.Contracts.DTOs;

public class GrupoDetalhesDTO
{
    public GrupoDetalhesDTO(string grupoId, string nome, int numSubGrupos, int numUsuarios, List<SubGrupoDetalhes> subGrupos)
    {
        GrupoId = grupoId;
        Nome = nome;
        NumSubGrupos = numSubGrupos;
        NumUsuarios = numUsuarios;
        SubGrupos = subGrupos;
    }

    public string GrupoId { get; set; }
    public string Nome { get; set; }
    public int NumSubGrupos { get; set; }
    public int NumUsuarios { get; set; }
    public List<SubGrupoDetalhes> SubGrupos { get; set; }

    public class SubGrupoDetalhes
    {
        public string SubGrupoId { get; set; }
        public string Nome { get; set; }
        public List<PermissoesDominio> PermissoesDominio { get; set; }
        public List<SubGrupoPermissoesEstabelecimentoDetalhes> PermissoesEstabelecimentos { get; private set; }

        public SubGrupoDetalhes(string subGrupoId, string nome, List<PermissoesDominio> permissoesDominio, List<SubGrupoPermissoesEstabelecimentoDetalhes> permissoesEstabelecimentos)
        {
            SubGrupoId = subGrupoId;
            Nome = nome;
            PermissoesDominio = permissoesDominio;
            PermissoesEstabelecimentos = permissoesEstabelecimentos;
        }
    }

    public class SubGrupoPermissoesEstabelecimentoDetalhes
    {
        public string? EstabelecimentoId { get; set; }
        public string? EstabelecimentoNome { get; set; }
        public string? EstabelecimentoCnes { get; set; }
        public string? RedeEstabelecimentosId { get; set; }
        public string? RedeEstabelecimentosNome { get; set; }
        public List<PermissoesEstabelecimento> Permissoes { get; private set; }

        public SubGrupoPermissoesEstabelecimentoDetalhes(string? estabelecimentoId, string? estabelecimentoNome, string? estabelecimentoCnes, string? redeEstabelecimentosId, string? redeEstabelecimentosNome, List<PermissoesEstabelecimento> permissoes)
        {
            EstabelecimentoId = estabelecimentoId;
            EstabelecimentoNome = estabelecimentoNome;
            EstabelecimentoCnes = estabelecimentoCnes;
            RedeEstabelecimentosId = redeEstabelecimentosId;
            RedeEstabelecimentosNome = redeEstabelecimentosNome;
            Permissoes = permissoes;
        }
    }
}
