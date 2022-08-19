namespace Pulsar.Services.Identity.Contracts.Enumerations;

public enum ExceptionKey
{
    [Display(Description = "Usuário não encontrado.")]
    UsuarioNaoEncontrado = 1,
    [Display(Description = "Usuário não possui e-mail cadastrado.")]
    UsuarioSemEmail = 2,
    [Display(Description = "Token para a mudança de senha expirado.")]
    TokenMudancaSenhaExpirado = 3,
    [Display(Description = "Token para a mudança de senha inválido.")]
    TokenMudancaSenhaInvalido = 4,
    [Display(Description = "Convite não encontrado.")]
    ConviteNaoEncontrado = 5,
    [Display(Description = "Convite expirado.")]
    ConviteExpirado = 6,
    [Display(Description = "Este convite já foi aceito.")]
    ConviteJaAceito = 7,
    [Display(Description = "Já existe um usuário para o e-mail informado neste convite.")]
    UsuarioJaConvidado = 8, 
    [Display(Description = "Token inválido.")]
    ConviteTokenInvalido = 9,
    [Display(Description = "Usuário foi convidado para administrar o domínio mas o domínio já tem administrador ou está inativo.")]
    ConviteDominioInvalido = 10
}
