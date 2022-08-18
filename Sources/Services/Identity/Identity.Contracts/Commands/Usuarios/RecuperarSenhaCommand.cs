using FluentValidation;
using System.Text.RegularExpressions;

namespace Pulsar.Services.Identity.Contracts.Commands.Usuarios;

public class RecuperarSenhaCommand : IRequest
{
    public string? UsuarioId { get; set; }
    public string? Token { get; set; }
    public string? Senha { get; set; }
    public string? ConfirmarSenha { get; set; }
}


public class RecuperarSenhaCommandValidator : AbstractValidator<RecuperarSenhaCommand>
{
    public RecuperarSenhaCommandValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("UsuarioId precisa ser informado.");
        RuleFor(x => x.Token).NotEmpty().WithMessage("Token precisa ser informado.");
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