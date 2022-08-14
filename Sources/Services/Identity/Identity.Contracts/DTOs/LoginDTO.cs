namespace Pulsar.Services.Identity.Contracts.DTOs;
public class LoginDTO
{
    public string? UsernameOrEmail { get; set; }
    public string? Senha { get; set; }
    public string? DominioId { get; set; }
    public string? EstabelecimentoId { get; set; }
    public string? ReturnUrl { get; set; }
}
