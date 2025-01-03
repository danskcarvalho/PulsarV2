﻿using FluentValidation;
using Pulsar.Services.Shared.Attributes;

namespace Pulsar.Services.Identity.Contracts.Commands.Dominios;

public class CriarDominioCmd : IRequest<CreatedCommandResult>
{

    [SwaggerExclude]
    public string? UsuarioLogadoId { get; set; }
    /// <summary>
    /// Nome do usuário.
    /// </summary>
    public string? Nome { get; set; }
    /// <summary>
    /// Id do usuário que administrará o domínio.
    /// </summary>
    public string? UsuarioAdministradorId { get; set; }
}


public class CriarDominioCommandValidator : AbstractValidator<CriarDominioCmd>
{
    public CriarDominioCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é de preenchimento obrigatório.")
            .Length(3, 64).WithMessage("O nome deve ter entre 3 e 64 caracteres.");
    }
}