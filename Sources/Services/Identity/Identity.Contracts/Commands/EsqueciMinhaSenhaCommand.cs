using FluentValidation;
using System;
using System.Text.RegularExpressions;

namespace Pulsar.Services.Identity.Contracts.Commands
{
    public class EsqueciMinhaSenhaCommand : IRequest
    {
        public string? UsernameOrEmail { get; set; }
    }

    public class EsqueciMinhaSenhaCommandValidator : AbstractValidator<EsqueciMinhaSenhaCommand>
    {
        public EsqueciMinhaSenhaCommandValidator()
        {
            RuleFor(x => x.UsernameOrEmail).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Nome de usuário ou e-mail é de preenchimento obrigatório.")
                .Must(x => IsEmailOrUserName(x)).WithMessage("Nome de usuário ou e-mail inválidos.");
        }

        private bool IsEmailOrUserName(string? usernameOrEmail)
        {
            if (usernameOrEmail == null)
                return true;
            else if (usernameOrEmail.All(c => char.IsLetterOrDigit(c) || c == '_'))
                return true;
            else if (Regex.IsMatch(usernameOrEmail, "^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$"))
                return true;
            else
                return false;
        }
    }
}
