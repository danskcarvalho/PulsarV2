namespace Pulsar.Services.Identity.Contracts.DTOs;

public class LogoutResultDTO
{
    public bool ShowSignoutPrompt { get; set; }
    public string? ClientName { get; set; }
    public string? PostLogoutRedirectUri { get; set; }
    public string? SignOutIFrameUrl { get; set; }
    public string? LogoutId { get; set; }
}
