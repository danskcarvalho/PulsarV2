using FluentValidation;
using Pulsar.Services.Shared.Attributes;

namespace Pulsar.Services.Identity.Contracts.Commands.Usuarios;

public class EditarMeusDadosCmd : IRequest<CommandResult>
{
    [SwaggerExclude]
    public string? UsuarioId { get; set; }
    /// <summary>
    /// Primeiro nome.
    /// </summary>
    public string? PrimeiroNome { get; set; }
    /// <summary>
    /// Sobrenome.
    /// </summary>
    public string? Sobrenome { get; set; }
}

public class EditarMeusDadosCommandValidator : AbstractValidator<EditarMeusDadosCmd>
{
    public EditarMeusDadosCommandValidator()
    {
        RuleFor(x => x.PrimeiroNome)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O primeiro nome é de preenchimento obrigatório.")
            .Length(2, 128).WithMessage("O primeiro nome deve ter entre 2 e 128 caracteres.");
        RuleFor(x => x.Sobrenome)
            .Length(2, 256).WithMessage("O sobrenome deve ter entre 2 e 256 caracteres.");
    }
}
