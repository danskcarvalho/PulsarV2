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
    TokenMudancaSenhaInvalido = 4
}
