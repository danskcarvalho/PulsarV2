using FluentValidation;
using Pulsar.Services.Shared.Attributes;

namespace Pulsar.Services.Identity.Contracts.Commands.Usuarios;

public class MudarMinhaSenhaCmd : IRequest<CommandResult>
{
    [SwaggerExclude]
    public string? UsuarioId { get; set; }
    /// <summary>
    /// Senha atual do usuário logado.
    /// </summary>
    public string? SenhaAtual { get; set; }
    /// <summary>
    /// Nova senha.
    /// </summary>
    public string? Senha { get; set; }
    /// <summary>
    /// Deve ser igual ao campo Senha.
    /// </summary>
    public string? ConfirmarSenha { get; set; }
}

public class MudarMinhaSenhaCommandValidator : AbstractValidator<MudarMinhaSenhaCmd>
{
    public MudarMinhaSenhaCommandValidator()
    {
        RuleFor(x => x.SenhaAtual)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("A senha atual é de preenchimento obrigatório.")
            .Length(8, 32).WithMessage("A senha atual precisa ter entre 8 e 32 caracteres");
        RuleFor(x => x.Senha)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("A senha é de preenchimento obrigatório.")
            .Length(8, 32).WithMessage("A senha precisa ter entre 8 e 32 caracteres")
            .Must(s => HaveCharsNumberAndSymbols(s)).WithMessage("A senha precisa ser formada por letras, números e símbolos.");
        RuleFor(x => x.ConfirmarSenha)
            .Equal(x => x.Senha).WithMessage("Este campo precisa ser igual à senha.");
    }

    private bool HaveCharsNumberAndSymbols(string s)
    {
        return s.Any(c => char.IsLetter(c)) && s.Any(c => char.IsDigit(c)) && s.Any(c => char.IsSymbol(c) || char.IsPunctuation(c));
    }
}