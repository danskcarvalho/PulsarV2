using System.Diagnostics.CodeAnalysis;

namespace Pulsar.Services.Identity.Contracts.DTOs;

public class UsuarioListadoDTO
{
    [SetsRequiredMembers]
    public UsuarioListadoDTO(string usuarioId, string email, string primeiroNome, string nomeCompleto, string nomeUsuario)
    {
        PrimeiroNome = primeiroNome;
        NomeCompleto = nomeCompleto;
        NomeUsuario = nomeUsuario;
        Email = email;
        UsuarioId = usuarioId;
    }
    private UsuarioListadoDTO() { }

    /// <summary>
    /// Id do usuário.
    /// </summary>
    public required string UsuarioId { get; set; }
    /// <summary>
    /// Url do avatar do usuário.
    /// </summary>
    public string? AvatarUrl { get; set; }
    /// <summary>
    /// Primeiro nome.
    /// </summary>
    public required string PrimeiroNome { get; set; }
    /// <summary>
    /// Sobrenome.
    /// </summary>
    public string? UltimoNome { get; set; }
    /// <summary>
    /// Nome completo.
    /// </summary>
    public required string NomeCompleto { get; set; }
    /// <summary>
    /// Flag global indicando se o usuário está ativo.
    /// </summary>
    public bool IsAtivo { get; set; }
    /// <summary>
    /// E-mail do usuário.
    /// </summary>
    public required string Email { get; set; }
    /// <summary>
    /// Nome de usuário.
    /// </summary>
    public required string NomeUsuario { get; set; }
    /// <summary>
    /// Está aguardando o usuário aceitar o convite.
    /// </summary>
    public bool IsConvitePendente { get; set; }
}