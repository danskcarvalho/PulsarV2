﻿using Pulsar.Services.Identity.Contracts.Commands.Dominios;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Dominios;

public class BloquearOuDesbloquearUsuariosNoDominioCH : IdentityCommandHandler<BloquearOuDesbloquearUsuariosNoDominioCmd, CommandResult>
{
    public BloquearOuDesbloquearUsuariosNoDominioCH(IdentityCommandHandlerContext<BloquearOuDesbloquearUsuariosNoDominioCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(BloquearOuDesbloquearUsuariosNoDominioCmd cmd, CancellationToken ct)
    {
        var dominio = await DominioRepository.FindOneByIdAsync(cmd.DominioId!.ToObjectId(), ct);
        if (dominio == null)
            throw new IdentityDomainException(ExceptionKey.DominioNaoEncontrado);
        if (!await UsuarioRepository.AllExistsAsync(cmd.UsuarioIds!.Select(u => u.ToObjectId()), ct: ct))
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
        dominio.BloquearOuDesbloquearUsuarios(cmd.UsuarioLogadoId!.ToObjectId(), cmd.UsuarioIds!.Select(uid => uid.ToObjectId()).ToList(), cmd.Bloquear);
        await DominioRepository.ReplaceOneAsync(dominio, ct: ct);
        return new CommandResult(Session.ConsistencyToken);
    }
}