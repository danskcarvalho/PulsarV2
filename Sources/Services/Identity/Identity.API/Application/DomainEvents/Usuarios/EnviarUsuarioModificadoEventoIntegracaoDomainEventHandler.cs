using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.IntegrationEvents;
using Pulsar.Services.Identity.Domain.Events.Usuarios;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class EnviarUsuarioModificadoEventoIntegracaoDomainEventHandler : IdentityDomainEventHandler<UsuarioModificadoDomainEvent>
{
    public EnviarUsuarioModificadoEventoIntegracaoDomainEventHandler(IdentityDomainEventHandlerContext<UsuarioModificadoDomainEvent> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(UsuarioModificadoDomainEvent evt, CancellationToken ct)
    {
        await EventLog.SaveEventAsync(new UsuarioModificadoIntegrationEvent {

            UsuarioId = evt.UsuarioId.ToString(),
            PublicAvatarUrl = evt.PublicAvatarUrl,
            PrivateAvatarUrl = evt.PrivateAvatarUrl,
            PrimeiroNome = evt.PrimeiroNome,
            UltimoNome = evt.UltimoNome,
            NomeCompleto = evt.NomeCompleto,
            IsAtivo = evt.IsAtivo,
            NomeUsuario = evt.NomeUsuario,
            IsConvitePendente = evt.IsConvitePendente,
            AuditInfo = evt.AuditInfo.ToDTO(),
            Modificacao = evt.Modificacao 
        }, ct);
    }
}
