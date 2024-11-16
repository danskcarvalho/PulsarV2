using FluentValidation;
using Pulsar.Services.Shared.Attributes;

namespace Pulsar.Services.Identity.Contracts.Commands.Dominios;

public class BloquearOuDesbloquearUsuariosNoDominioCmd : IRequest<CommandResult>
{

    [SwaggerExclude]
    public string? UsuarioLogadoId { get; set; }

    [SwaggerExclude]
    public string? DominioId { get; set; }
    /// <summary>
    /// Id dos usuários a serem bloqueados ou desbloqueados.
    /// </summary>
    public List<string>? UsuarioIds { get; set; }
    [SwaggerExclude]
    public bool Bloquear { get; set; }
}

public class BloquearOuDesbloquearUsuariosNoDominioCommandValidator : AbstractValidator<BloquearOuDesbloquearUsuariosNoDominioCmd>
{
    public BloquearOuDesbloquearUsuariosNoDominioCommandValidator()
    {
        RuleFor(x => x.UsuarioIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O nome é de preenchimento obrigatório.")
            .Must(x => x != null && x.Count <= 1024).WithMessage("Não é possível bloquear mais de 1024 usuários em uma única chamada da API.");
        RuleForEach(x => x.UsuarioIds).NotEmpty().WithMessage("Id do usuário inválido.");
    }
}
