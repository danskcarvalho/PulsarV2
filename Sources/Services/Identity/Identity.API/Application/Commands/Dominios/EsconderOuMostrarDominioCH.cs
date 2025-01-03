﻿using Pulsar.Services.Identity.Contracts.Commands.Dominios;

namespace Pulsar.Services.Identity.API.Application.Commands.Dominios;

public class EsconderOuMostrarDominioCH : IdentityCommandHandler<EsconderOuMostrarDominioCmd, CommandResult>
{
    public EsconderOuMostrarDominioCH(IdentityCommandHandlerContext<EsconderOuMostrarDominioCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(EsconderOuMostrarDominioCmd cmd, CancellationToken ct)
    {
        var dominio = await DominioRepository.FindOneByIdAsync(cmd.DominioId!.ToObjectId());
        if (dominio == null)
            throw new IdentityDomainException(IdentityExceptionKey.DominioNaoEncontrado);
        if (cmd.Esconder)
            dominio.Esconder(cmd.UsuarioLogadoId!.ToObjectId());
        else
            dominio.Mostrar(cmd.UsuarioLogadoId!.ToObjectId());
        await DominioRepository.ReplaceOneAsync(dominio);
        return new CommandResult(Session.ConsistencyToken);
    }
}
