﻿using Pulsar.Services.Identity.Domain.Events.Dominios;
using Pulsar.Services.Identity.Domain.Specifications.Usuarios;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class BloquearOuDesbloquearUsuariosEmDominioDEH : IdentityDomainEventHandler<UsuariosBloqueadosEmDominioDE>
{
    public BloquearOuDesbloquearUsuariosEmDominioDEH(IdentityDomainEventHandlerContext<UsuariosBloqueadosEmDominioDE> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(UsuariosBloqueadosEmDominioDE evt, CancellationToken ct)
    {
        var dominio = await DominioRepository.FindOneByIdAsync(evt.DominioId);
        if (dominio == null)
            throw new IdentityDomainException(IdentityExceptionKey.DominioNaoEncontrado);
        var updateUsuarios = new BloquearOuDesbloquearUsuariosEmDominioSpec(evt.UsuarioLogadoId, evt.DominioId, evt.UsuariosIds, evt.Bloquear);
        await UsuarioRepository.UpdateManyAsync(updateUsuarios);
    }
}
