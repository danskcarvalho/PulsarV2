using FluentValidation;
using Pulsar.Services.Shared.Attributes;

namespace Pulsar.Services.Identity.Contracts.Commands.Grupos;

public class EditarGrupoCmd : IRequest<CommandResult>
{
    [SwaggerExclude]
    public string? UsuarioLogadoId { get; set; }
    [SwaggerExclude]
    public string? DominioId { get; set; }
    [SwaggerExclude]
    public string? GrupoId { get; set; }
    /// <summary>
    /// Nome do grupo.
    /// </summary>
    public string? Nome { get; set; }
}

public class EditarGrupoCommandValidator : AbstractValidator<EditarGrupoCmd>
{
    public EditarGrupoCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é de preenchimento obrigatório.")
            .Length(3, 64).WithMessage("O nome deve ter entre 3 e 64 caracteres.");
    }
}
