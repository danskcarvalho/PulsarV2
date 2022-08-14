namespace Pulsar.Services.Identity.UI.Models
{
    public class LoginModel
    {
        public LoginStage Stage { get; set; } = LoginStage.UsuarioSenha;
        public string? EstabelecimentoId { get; set; }
        public SelectOption[]? Estabelecimentos { get; set; }
        public string? DominioId { get; set; }
        public SelectOption[]? Dominios { get; set; }
        public LoginOn LoginOn { get; set; } = LoginOn.Dominio;
        public string? ErrorMessage { get; set; } = null;
        public string? UsernameOrEmail { get; set; }
        public string? Password { get; set; }

        public LoginModel()
        {
        }
    }
}
