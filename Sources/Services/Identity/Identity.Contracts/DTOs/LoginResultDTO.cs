namespace Pulsar.Services.Identity.Contracts.DTOs;

public class LoginResultDTO
{
    public bool Ok { get; set; }
    public string? Erro { get; set; }
    public string? RedirectUrl { get; set; }
}
