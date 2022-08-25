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
        await EventLog.SaveEventAsync(new UsuarioModificadoIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            evt.UsuarioId.ToString(),
            evt.PublicAvatarUrl,
            evt.PrivateAvatarUrl,
            evt.PrimeiroNome,
            evt.UltimoNome,
            evt.NomeCompleto,
            evt.IsAtivo,
            evt.NomeUsuario,
            evt.IsConvitePendente,
            evt.AuditInfo.ToDTO(),
            evt.Modificacao), ct);
    }
}
