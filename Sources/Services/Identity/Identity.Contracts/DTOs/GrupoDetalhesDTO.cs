using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Pulsar.Services.Identity.Contracts.DTOs;

public class GrupoDetalhesDTO
{
    [SetsRequiredMembers]
    public GrupoDetalhesDTO(string grupoId, string nome, int numSubGrupos, int numUsuarios, List<SubGrupoDetalhes> subGrupos)
    {
        GrupoId = grupoId;
        Nome = nome;
        NumSubGrupos = numSubGrupos;
        NumUsuarios = numUsuarios;
        SubGrupos = subGrupos;
    }
    private GrupoDetalhesDTO() { }

    public required string GrupoId { get; set; }
    public required string Nome { get; set; }
    public int NumSubGrupos { get; set; }
    public int NumUsuarios { get; set; }
    public required List<SubGrupoDetalhes> SubGrupos { get; set; }

    public class SubGrupoDetalhes
    {
        public required string SubGrupoId { get; set; }
        public required string Nome { get; set; }
        public required List<PermissoesDominio> PermissoesDominio { get; set; }
        public int NumUsuarios { get; set; }
        public required List<SubGrupoPermissoesEstabelecimentoDetalhes> PermissoesEstabelecimentos { get; set; }

        [SetsRequiredMembers]
        public SubGrupoDetalhes(string subGrupoId, string nome, List<PermissoesDominio> permissoesDominio, int numUsuarios, List<SubGrupoPermissoesEstabelecimentoDetalhes> permissoesEstabelecimentos)
        {
            SubGrupoId = subGrupoId;
            Nome = nome;
            PermissoesDominio = permissoesDominio;
            PermissoesEstabelecimentos = permissoesEstabelecimentos;
            NumUsuarios = numUsuarios;
        }
        private SubGrupoDetalhes() { }
    }

    public class SubGrupoPermissoesEstabelecimentoDetalhes
    {
        public string? EstabelecimentoId { get; set; }
        public string? EstabelecimentoNome { get; set; }
        public string? EstabelecimentoCnes { get; set; }
        public string? RedeEstabelecimentosId { get; set; }
        public string? RedeEstabelecimentosNome { get; set; }
        public required List<PermissoesEstabelecimento> Permissoes { get; set; }

        [SetsRequiredMembers]
        public SubGrupoPermissoesEstabelecimentoDetalhes(string? estabelecimentoId, string? estabelecimentoNome, string? estabelecimentoCnes, string? redeEstabelecimentosId, string? redeEstabelecimentosNome, List<PermissoesEstabelecimento> permissoes)
        {
            EstabelecimentoId = estabelecimentoId;
            EstabelecimentoNome = estabelecimentoNome;
            EstabelecimentoCnes = estabelecimentoCnes;
            RedeEstabelecimentosId = redeEstabelecimentosId;
            RedeEstabelecimentosNome = redeEstabelecimentosNome;
            Permissoes = permissoes;
        }
        private SubGrupoPermissoesEstabelecimentoDetalhes() { }
    }
}
