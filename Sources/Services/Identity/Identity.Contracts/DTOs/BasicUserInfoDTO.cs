using System.Diagnostics.CodeAnalysis;
using static Pulsar.Services.Identity.Contracts.DTOs.UsuarioLogadoDTO;

namespace Pulsar.Services.Identity.Contracts.DTOs;

public class BasicUserInfoDTO
{
    /// <summary>
    /// Id do usuário.
    /// </summary>
    public required string UsuarioId { get; set; }
    /// <summary>
    /// Primeiro nome.
    /// </summary>
    public required string PrimeiroNome { get; set; }
    /// <summary>
    /// Sobrenome.
    /// </summary>
    public string? UltimoNome { get; set; }
    /// <summary>
    /// Nome completo. Junção do primeiro nome + sobrenome.
    /// </summary>
    public required string NomeCompleto { get; set; }
    /// <summary>
    /// E-mail do usuário. Único.
    /// </summary>
    public string? Email { get; set; }
    /// <summary>
    /// Nome de usuário. Único.
    /// </summary>
    public required string NomeUsuario { get; set; }
    /// <summary>
    /// Apenas o usuário administrador é um super usuário.
    /// </summary>
    public bool IsSuperUsuario { get; set; }
    /// <summary>
    /// Url da imagem utilizada como avatar do usuário.
    /// </summary>
    public string? AvatarUrl { get; set; }

    [SetsRequiredMembers]
    public BasicUserInfoDTO(string usuarioId, string primeiroNome, string? ultimoNome, string nomeCompleto, string? email, string nomeUsuario, string? avatarUrl, bool isSuperUsuario)
    {
        UsuarioId = usuarioId;
        PrimeiroNome = primeiroNome;
        UltimoNome = ultimoNome;
        NomeCompleto = nomeCompleto;
        Email = email;
        NomeUsuario = nomeUsuario;
        IsSuperUsuario = isSuperUsuario;
        AvatarUrl = avatarUrl;
    }
    private BasicUserInfoDTO()
    {

    }
}
