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
        await EventLog.SaveEventAsync(new DominioModificadoIntegrationEvent {
                DominioId = evt.DominioId.ToString(),
                Nome = evt.Nome,
                IsAtivo = evt.IsAtivo,
                AuditInfo = evt.AuditInfo.ToDTO(),
                UsuarioAdministradorId = evt.UsuarioAdministradorId.ToString(),
                UsuarioAdministradorAnteriorId = evt.UsuarioAdministradorAnteriorId.ToString(),
                Modificacao = evt.Modificacao
            });
    }
}
