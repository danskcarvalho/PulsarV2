using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Events.Convites;
using Pulsar.Services.Identity.Domain.Specifications.Usuarios;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class ConfigurarUsuarioConvidadoDEH : IdentityDomainEventHandler<ConviteAceitoDE>
{
    public ConfigurarUsuarioConvidadoDEH(IdentityDomainEventHandlerContext<ConviteAceitoDE> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(ConviteAceitoDE evt, CancellationToken ct)
    {
        var usuarioExistente = await UsuarioRepository.FindOneByIdAsync(evt.UsuarioId);
        if (usuarioExistente == null)
            throw new IdentityDomainException(ExceptionKey.ConviteInvalido);
        if (!usuarioExistente.IsConvitePendente)
            throw new IdentityDomainException(ExceptionKey.UsuarioJaConvidado);

        var usuarioComNomeInformado = await UsuarioRepository.FindOneAsync(new FindUsuarioByEitherUsenameOrEmailSpec(evt.NomeUsuario, null));
        if (usuarioComNomeInformado != null)
            throw new IdentityDomainException(ExceptionKey.NomeUsuarioNaoUnico);

        await usuarioExistente.AceitarConvite(evt.PrimeiroNome, evt.Sobrenome, evt.NomeUsuario, evt.Senha);
    }
}
