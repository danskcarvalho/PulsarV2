﻿using Pulsar.Services.Identity.Contracts.Commands.Dominios;

namespace Pulsar.Services.Identity.API.Application.Commands.Dominios;

public class BloquearOuDesbloquearDominioCH : IdentityCommandHandler<BloquearOuDesbloquearDominioCmd, CommandResult>
{
    public BloquearOuDesbloquearDominioCH(IdentityCommandHandlerContext<BloquearOuDesbloquearDominioCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(BloquearOuDesbloquearDominioCmd cmd, CancellationToken ct)
    {
        var dominio = await DominioRepository.FindOneByIdAsync(cmd.DominioId!.ToObjectId());
        if (dominio == null)
            throw new IdentityDomainException(IdentityExceptionKey.DominioNaoEncontrado);
        dominio.BloquearOuDesbloquear(cmd.UsuarioLogadoId!.ToObjectId(), cmd.Bloquear);
        await DominioRepository.ReplaceOneAsync(dominio);
        return new CommandResult(Session.ConsistencyToken);
    }
}
