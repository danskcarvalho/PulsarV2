using static Pulsar.Services.Identity.Contracts.DTOs.UsuarioLogadoDTO;

namespace Pulsar.Services.Identity.Contracts.DTOs;

public class BasicUserInfoDTO
{
    public string Id { get; set; }
    public string PrimeiroNome { get; set; }
    public string? UltimoNome { get; set; }
    public string NomeCompleto { get; set; }
    public string? Email { get; set; }
    public string NomeUsuario { get; set; }
    public bool IsSuperUsuario { get; set; }
    public string? AvatarUrl { get; set; }

    public BasicUserInfoDTO(string id, string primeiroNome, string? ultimoNome, string nomeCompleto, string? email, string nomeUsuario, string? avatarUrl, bool isSuperUsuario)
    {
        Id = id;
        PrimeiroNome = primeiroNome;
        UltimoNome = ultimoNome;
        NomeCompleto = nomeCompleto;
        Email = email;
        NomeUsuario = nomeUsuario;
        IsSuperUsuario = isSuperUsuario;
        AvatarUrl = avatarUrl;
    }
}
