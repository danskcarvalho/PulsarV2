using System.Diagnostics.CodeAnalysis;

namespace Pulsar.Services.Identity.Contracts.DTOs;

public class UsuarioLogadoDTO
{
    public required string Id { get; set; }
    public required string PrimeiroNome { get; set; }
    public string? UltimoNome { get; set; }
    public required string NomeCompleto { get; set; }
    public string? Email { get; set; }
    public required string NomeUsuario { get; set; }
    public bool IsAtivo { get; set; }
    public bool IsSuperUsuario { get; set; }  
    public string? AvatarUrl { get; set; }
    public required List<DominioDTO> Dominios { get; set; }

    [SetsRequiredMembers]
    public UsuarioLogadoDTO(string id, string primeiroNome, string? ultimoNome, string nomeCompleto, string? email, string nomeUsuario, bool isAtivo, bool isSuperUsuario, string? avatarUrl, List<DominioDTO> dominios)
    {
        Id = id;
        PrimeiroNome = primeiroNome;
        UltimoNome = ultimoNome;
        NomeCompleto = nomeCompleto;
        Email = email;
        NomeUsuario = nomeUsuario;
        IsAtivo = isAtivo;
        IsSuperUsuario = isSuperUsuario;
        AvatarUrl = avatarUrl;
        Dominios = dominios;
    }
    private UsuarioLogadoDTO()
    {
    }

    public class DominioDTO
    {
        public required string Id { get; set; }
        public required string Nome { get; set; }
        public bool IsAdministrador { get; set; }
        public bool PodeLogarDominio { get; set; }
        public required List<PermissoesDominio> Permissoes { get; set; }
        public required List<EstabelecimentoDTO> Estabelecimentos { get; set; }

        [SetsRequiredMembers]
        public DominioDTO(string id, string nome, bool isAdministrador, bool podeLogarDominio, List<PermissoesDominio> permissoes, List<EstabelecimentoDTO> estabelecimentos)
        {
            Id = id;
            Nome = nome;
            IsAdministrador = isAdministrador;
            PodeLogarDominio = podeLogarDominio;
            Permissoes = permissoes;
            Estabelecimentos = estabelecimentos;
        }

        private DominioDTO()
        {
        }
    }

    public class EstabelecimentoDTO
    {
        public required string Id { get; set; }
        public required string Nome { get; set; }
        public required List<PermissoesEstabelecimento> Permissoes { get; set; }

        [SetsRequiredMembers]
        public EstabelecimentoDTO(string id, string nome, List<PermissoesEstabelecimento> permissoes)
        {
            Id = id;
            Nome = nome;
            Permissoes = permissoes;
        }

        private EstabelecimentoDTO()
        {
        }
    }

    public bool ValidateLogin(string? dominioId, string? estabelecimentoId)
    {
        if (!this.IsAtivo)
            return false;

        if (this.IsSuperUsuario)
            return dominioId is null && estabelecimentoId is null;
        else if (dominioId is null && estabelecimentoId is null)
            return this.IsSuperUsuario;
        else if (dominioId is null && estabelecimentoId is not null)
            return false;
        else if (estabelecimentoId is not null)
            return this.Dominios.Any(d => d.Id == dominioId && d.Estabelecimentos.Any(e => e.Id == estabelecimentoId && e.Permissoes.Any()));
        else if (dominioId is not null)
            return this.Dominios.Any(d => d.Id == dominioId && (d.IsAdministrador || d.Permissoes.Any()));
        else
            return false;
    }
}
