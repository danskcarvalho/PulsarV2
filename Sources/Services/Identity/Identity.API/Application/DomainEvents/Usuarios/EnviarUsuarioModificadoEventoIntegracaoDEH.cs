using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.IntegrationEvents;
using Pulsar.Services.Identity.Domain.Events.Usuarios;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class EnviarUsuarioModificadoEventoIntegracaoDEH : IdentityDomainEventHandler<UsuarioModificadoDE>
{
    public EnviarUsuarioModificadoEventoIntegracaoDEH(IdentityDomainEventHandlerContext<UsuarioModificadoDE> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(UsuarioModificadoDE evt, CancellationToken ct)
    {
        await EventLog.SaveEventAsync(new UsuarioModificadoIE {

            UsuarioId = evt.UsuarioId.ToString(),
            AvatarUrl = evt.AvatarUrl,
            PrimeiroNome = evt.PrimeiroNome,
            UltimoNome = evt.UltimoNome,
            NomeCompleto = evt.NomeCompleto,
            IsAtivo = evt.IsAtivo,
            NomeUsuario = evt.NomeUsuario,
            Email = evt.Email,
            IsConvitePendente = evt.IsConvitePendente,
            AuditInfo = evt.AuditInfo.ToDTO(),
            Modificacao = evt.Modificacao 
        });
    }
}
