using FluentValidation;
using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Shared.Attributes;

namespace Pulsar.Services.Identity.Contracts.Commands.Grupos;

public class RemoverUsuariosSubGrupoCommand : IRequest<CommandResult>
{
    [SwaggerExclude]
    public string? UsuarioLogadoId { get; set; }
    [SwaggerExclude]
    public string? DominioId { get; set; }
    [SwaggerExclude]
    public string? GrupoId { get; set; }
    [SwaggerExclude]
    public string? SubGrupoId { get; set; }
    /// <summary>
    /// Usuários a serem removidos.
    /// </summary>
    public List<string>? UsuarioIds { get; set; }
}

public class RemoverUsuariosSubGrupoCommandValidator : AbstractValidator<RemoverUsuariosSubGrupoCommand>
{
    public RemoverUsuariosSubGrupoCommandValidator()
    {
        RuleFor(x => x.UsuarioIds).NotEmpty().WithMessage("É necessário informar pelo menos 1 usuário.");
        RuleForEach(x => x.UsuarioIds).NotEmpty().WithMessage("Usuário não informado.");
    }
}