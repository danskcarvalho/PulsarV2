using FluentValidation;
using Pulsar.Services.Shared.Attributes;

namespace Pulsar.Services.Identity.Contracts.Commands.Convites;

public class CriarConviteCommand : IRequest
{
    [SwaggerExclude]
    public string? UsuarioLogadoId { get; set; }
    /// <summary>
    /// E-mail do usuário a ser convidado para usar o Pulsar.
    /// </summary>
    public string? Email { get; set; }
}

public class CriarConviteCommandValidator : AbstractValidator<CriarConviteCommand>
{
    public CriarConviteCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é de preenchimento obrigatório.")
            .Length(0, 256).WithMessage("O e-mail não pode ter mais de 256 caracteres")
            .EmailAddress().WithMessage("E-mail inválido.");
    }
}
