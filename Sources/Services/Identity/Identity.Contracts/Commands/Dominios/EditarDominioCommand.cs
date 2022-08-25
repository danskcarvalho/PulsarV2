using FluentValidation;
using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Shared.Attributes;

namespace Pulsar.Services.Identity.Contracts.Commands.Dominios;

public class EditarDominioCommand : IRequest<CommandResult>
{

    [SwaggerExclude]
    public string? UsuarioLogadoId { get; set; }
    [SwaggerExclude]
    public string? DominioId { get; set; }
    /// <summary>
    /// Nome do domínio.
    /// </summary>
    public string? Nome { get; set; }
    /// <summary>
    /// Id do usuário que administrará o domínio.
    /// </summary>
    public string? UsuarioAdministradorId { get; set; }
}

public class EditarDominioCommandValidator : AbstractValidator<EditarDominioCommand>
{
    public EditarDominioCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é de preenchimento obrigatório.")
            .Length(3, 64).WithMessage("O nome deve ter entre 3 e 64 caracteres.");
    }
}
