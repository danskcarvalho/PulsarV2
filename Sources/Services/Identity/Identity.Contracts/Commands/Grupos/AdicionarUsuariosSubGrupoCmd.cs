using FluentValidation;
using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Shared.Attributes;

namespace Pulsar.Services.Identity.Contracts.Commands.Grupos;

public class AdicionarUsuariosSubGrupoCmd : IRequest<CommandResult>
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
    /// Usuários a serem adicionados.
    /// </summary>
    public List<string>? UsuarioIds { get; set; }
}

public class AdicionarUsuariosSubGrupoCommandValidator : AbstractValidator<AdicionarUsuariosSubGrupoCmd>
{
    public AdicionarUsuariosSubGrupoCommandValidator()
    {
        RuleFor(x => x.UsuarioIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("É necessário informar pelo menos 1 usuário.")
            .Must(u => u!.Count < 256).WithMessage("Só é possível adicionar até 256 usuários por vez.");
        RuleForEach(x => x.UsuarioIds).NotEmpty().WithMessage("Usuário não informado.");
    }
}
