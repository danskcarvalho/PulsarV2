﻿using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Events.Convites;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class ConfigurarUsuarioConvidadoDEH : IdentityDomainEventHandler<ConviteAceitoDE>
{
    public ConfigurarUsuarioConvidadoDEH(IdentityDomainEventHandlerContext<ConviteAceitoDE> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(ConviteAceitoDE evt, CancellationToken ct)
    {
        var usuarioExistente = await UsuarioRepository.FindOneByIdAsync(evt.UsuarioId, ct);
        if (usuarioExistente == null)
            throw new IdentityDomainException(ExceptionKey.ConviteInvalido);
        if (!usuarioExistente.IsConvitePendente)
            throw new IdentityDomainException(ExceptionKey.UsuarioJaConvidado);

        var usuarioComNomeInformado = await UsuarioRepository.FindOneAsync(new FindUsuarioByEitherUsenameOrEmailSpec(evt.NomeUsuario, null), ct);
        if (usuarioComNomeInformado != null)
            throw new IdentityDomainException(ExceptionKey.NomeUsuarioNaoUnico);

        usuarioExistente.AceitarConvite(evt.PrimeiroNome, evt.Sobrenome, evt.NomeUsuario, evt.Senha);
        await UsuarioRepository.ReplaceOneAsync(usuarioExistente, ct: ct);
    }
}