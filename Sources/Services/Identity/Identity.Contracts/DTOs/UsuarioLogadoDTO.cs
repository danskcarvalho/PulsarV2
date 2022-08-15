namespace Pulsar.Services.Identity.Contracts.DTOs;

public class UsuarioLogadoDTO
{
    public string Id { get; set; }
    public string PrimeiroNome { get; set; }
    public string? UltimoNome { get; set; }
    public string NomeCompleto { get; set; }
    public string? Email { get; set; }
    public string NomeUsuario { get; set; }
    public bool IsAtivo { get; set; }
    public bool IsSuperUsuario { get; set; }  
    public string? AvatarUrl { get; set; }
    public List<DominioDTO> Dominios{ get; set; }

    [JsonConstructor]
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

    public class DominioDTO
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public bool IsAdministrador { get; set; }
        public bool PodeLogarDominio { get; set; }
        public List<PermissoesGerais> PermissoesGerais { get; set; }
        public List<EstabelecimentoDTO> Estabelecimentos { get; set; }

        [JsonConstructor]
        public DominioDTO(string id, string nome, bool isAdministrador, bool podeLogarDominio, List<PermissoesGerais> permissoesGerais, List<EstabelecimentoDTO> estabelecimentos)
        {
            Id = id;
            Nome = nome;
            IsAdministrador = isAdministrador;
            PodeLogarDominio = podeLogarDominio;
            PermissoesGerais = permissoesGerais;
            Estabelecimentos = estabelecimentos;
        }
    }

    public class EstabelecimentoDTO
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public List<PermissoesEstabelecimento> Permissoes { get; set; }

        [JsonConstructor]
        public EstabelecimentoDTO(string id, string nome, List<PermissoesEstabelecimento> permissoes)
        {
            Id = id;
            Nome = nome;
            Permissoes = permissoes;
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
            return this.Dominios.Any(d => d.Id == dominioId && (d.IsAdministrador || d.PermissoesGerais.Any()));
        else
            return false;
    }
}
