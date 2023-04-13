using Pulsar.Services.Identity.Contracts.IntegrationEvents;
using Pulsar.Services.Identity.Domain.Events.Dominios;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Dominios;

public class EnviarDominioModificadoEventoIntegracaoDEH : IdentityDomainEventHandler<DominioModificadoDE>
{
    public EnviarDominioModificadoEventoIntegracaoDEH(IdentityDomainEventHandlerContext<DominioModificadoDE> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(DominioModificadoDE evt, CancellationToken ct)
    {
        await EventLog.SaveEventAsync(new DominioModificadoIE {
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
