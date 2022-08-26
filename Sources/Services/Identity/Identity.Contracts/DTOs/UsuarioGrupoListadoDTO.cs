namespace Pulsar.Services.Identity.Contracts.DTOs;

public class UsuarioGrupoListadoDTO
{
    [JsonConstructor]
    public UsuarioGrupoListadoDTO(string usuarioId, string email, string primeiroNome, string nomeCompleto, string nomeUsuario)
    {
        PrimeiroNome = primeiroNome;
        NomeCompleto = nomeCompleto;
        NomeUsuario = nomeUsuario;
        Email = email;
        UsuarioId = usuarioId;
        SubGrupos = new List<SubGrupoDetalhes>();
    }

    /// <summary>
    /// Id do usuário.
    /// </summary>
    public string UsuarioId { get; set; }
    /// <summary>
    /// Url do avatar do usuário.
    /// </summary>
    public string? AvatarUrl { get; set; }
    /// <summary>
    /// Primeiro nome.
    /// </summary>
    public string PrimeiroNome { get; set; }
    /// <summary>
    /// Sobrenome.
    /// </summary>
    public string? UltimoNome { get; set; }
    /// <summary>
    /// Nome completo.
    /// </summary>
    public string NomeCompleto { get; set; }
    /// <summary>
    /// Flag global indicando se o usuário está ativo.
    /// </summary>
    public bool IsAtivo { get; set; }
    /// <summary>
    /// E-mail do usuário.
    /// </summary>
    public string Email { get; set; }
    /// <summary>
    /// Nome de usuário.
    /// </summary>
    public string NomeUsuario { get; set; }
    /// <summary>
    /// Está aguardando o usuário aceitar o convite.
    /// </summary>
    public bool IsConvitePendente { get; set; }
    /// <summary>
    /// Subgrupos aos quais este usuário pertence.
    /// </summary>
    public List<SubGrupoDetalhes> SubGrupos { get; set; }

    public class SubGrupoDetalhes
    {
        public string SubGrupoId { get; set; }
        public string Nome { get; set; }

        public SubGrupoDetalhes(string subGrupoId, string nome)
        {
            SubGrupoId = subGrupoId;
            Nome = nome;
        }
    }
}

public class CursorUsuarioGrupoListadoDTO
{
    public string LastEmail { get; set; }
    public string GrupoId { get; set; }
    public string? Filtro { get; set; }

    public CursorUsuarioGrupoListadoDTO(string lastEmail, string grupoId, string? filtro)
    {
        LastEmail = lastEmail;
        GrupoId = grupoId;
        Filtro = filtro;
    }

    public CursorUsuarioGrupoListadoDTO? Next(List<UsuarioGrupoListadoDTO> usuarios)
    {
        if (usuarios == null || usuarios.Count == 0)
            return null;

        return new CursorUsuarioGrupoListadoDTO(usuarios.Last().Email, this.GrupoId, this.Filtro);
    }

    public static CursorUsuarioGrupoListadoDTO? Next(List<UsuarioGrupoListadoDTO> usuarios, string grupoId, string? filtro)
    {
        if (usuarios == null || usuarios.Count == 0)
            return null;

        return new CursorUsuarioGrupoListadoDTO(usuarios.Last().Email, grupoId, filtro);
    }
}