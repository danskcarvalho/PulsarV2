namespace Pulsar.Services.Identity.Contracts.Enumerations;

public enum ExceptionKey
{
    [Display(Description = "Usuário não encontrado.")]
    UsuarioNaoEncontrado = 1,
    [Display(Description = "Usuário não possui e-mail cadastrado.")]
    UsuarioSemEmail = 2
}
