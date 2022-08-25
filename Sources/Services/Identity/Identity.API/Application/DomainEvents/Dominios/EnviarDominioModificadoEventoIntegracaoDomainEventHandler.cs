using Pulsar.Services.Identity.Contracts.IntegrationEvents;
using Pulsar.Services.Identity.Domain.Events.Dominios;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Dominios;

public class EnviarDominioModificadoEventoIntegracaoDomainEventHandler : IdentityDomainEventHandler<DominioModificadoDomainEvent>
{
    public EnviarDominioModificadoEventoIntegracaoDomainEventHandler(IdentityDomainEventHandlerContext<DominioModificadoDomainEvent> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(DominioModificadoDomainEvent evt, CancellationToken ct)
    {
        await EventLog.SaveEventAsync(new DominioModificadoIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                evt.DominioId.ToString(),
                evt.Nome,
                evt.IsAtivo,
                evt.AuditInfo.ToDTO(),
                evt.UsuarioAdministradorId.ToString(),
                evt.UsuarioAdministradorAnteriorId.ToString(),
                evt.Modificacao
            ));
    }
}
