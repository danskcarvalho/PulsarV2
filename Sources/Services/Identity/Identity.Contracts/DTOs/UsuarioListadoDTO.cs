namespace Pulsar.Services.Identity.Contracts.DTOs;

public class UsuarioListadoDTO
{
    [JsonConstructor]
    public UsuarioListadoDTO(string usuarioId, string email, string primeiroNome, string nomeCompleto, string nomeUsuario)
    {
        PrimeiroNome = primeiroNome;
        NomeCompleto = nomeCompleto;
        NomeUsuario = nomeUsuario;
        Email = email;
        UsuarioId = usuarioId;
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
}

public class CursorUsuarioListadoDTO
{
    public string LastEmail { get; set; }
    public string? Filtro { get; set; }

    public CursorUsuarioListadoDTO(string lastEmail, string? filtro)
    {
        LastEmail = lastEmail;
        Filtro = filtro;
    }

    public CursorUsuarioListadoDTO? Next(List<UsuarioListadoDTO> usuarios)
    {
        if (usuarios == null || usuarios.Count == 0)
            return null;

        return new CursorUsuarioListadoDTO(usuarios.Last().Email, this.Filtro);
    }

    public static CursorUsuarioListadoDTO? Next(List<UsuarioListadoDTO> usuarios, string? filtro)
    {
        if (usuarios == null || usuarios.Count == 0)
            return null;

        return new CursorUsuarioListadoDTO(usuarios.Last().Email, filtro);
    }
}

public class CursorUsuariosBloqueadosDTO
{
    public string LastEmail { get; set; }
    public string DominioId { get; set; }
    public string? Filtro { get; set; }

    public CursorUsuariosBloqueadosDTO(string lastEmail, string dominioId, string? filtro)
    {
        LastEmail = lastEmail;
        DominioId = dominioId;
        Filtro = filtro;
    }

    public CursorUsuariosBloqueadosDTO? Next(List<UsuarioListadoDTO> usuarios)
    {
        if (usuarios == null || usuarios.Count == 0)
            return null;

        return new CursorUsuariosBloqueadosDTO(usuarios.Last().Email, this.DominioId, this.Filtro);
    }

    public static CursorUsuariosBloqueadosDTO? Next(List<UsuarioListadoDTO> usuarios, string dominioId, string? filtro)
    {
        if (usuarios == null || usuarios.Count == 0)
            return null;

        return new CursorUsuariosBloqueadosDTO(usuarios.Last().Email, dominioId, filtro);
    }
}