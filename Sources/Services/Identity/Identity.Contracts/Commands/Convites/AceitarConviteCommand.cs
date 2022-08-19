using FluentValidation;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using System.Text.RegularExpressions;

namespace Pulsar.Services.Identity.Contracts.Commands.Convites;

public class AceitarConviteCommand : IRequest
{
    public string? ConviteId { get; set; }
    public string? Token { get; set; }
    public string? PrimeiroNome { get; set; }
    public string? Sobrenome { get; set; }
    public string? NomeUsuario { get; set; }
    public string? Senha { get; set; }
    public string? ConfirmarSenha { get; set; }
}

public class AceitarConviteCommandValidator : AbstractValidator<AceitarConviteCommand>
{
    public AceitarConviteCommandValidator()
    {
        RuleFor(x => x.ConviteId).NotEmpty().WithMessage("ConviteId precisa ser informado.");
        RuleFor(x => x.Token).NotEmpty().WithMessage("Token precisa ser informado.");
        RuleFor(x => x.PrimeiroNome)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O primeiro nome é de preenchimento obrigatório.")
            .Length(2, 128).WithMessage("O primeiro nome deve ter entre 2 e 128 caracteres.");
        RuleFor(x => x.Sobrenome)
            .Length(2, 256).WithMessage("O sobrenome deve ter entre 2 e 256 caracteres.");
        RuleFor(x => x.Sobrenome)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O nome de usuário é de preenchimento obrigatório.")
            .Length(8, 32).WithMessage("O nome de usuário deve ter entre 8 e 32 caracteres.")
            .Must(x => IsUserName(x)).WithMessage("Nome de usuário inválido.");
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

    private bool IsUserName(string? usernameOrEmail)
    {
        if (usernameOrEmail == null)
            return true;
        else if (usernameOrEmail.All(c => char.IsLetterOrDigit(c) || c == '_'))
            return true;
        else
            return false;
    }
}