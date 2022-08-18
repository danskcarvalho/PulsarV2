using Pulsar.Services.Identity.Contracts.Commands.Others;

namespace Pulsar.Services.Identity.BackgroundTasks.Application.IntegrationEvents;

public class EstabelecimentoEditadoOuCriadoIntegrationEventHandler : IdentityIntegrationEventHandler<EstabelecimentoEditadoOuCriadoIntegrationEvent>
{
    public EstabelecimentoEditadoOuCriadoIntegrationEventHandler(ILogger<IdentityIntegrationEventHandler<EstabelecimentoEditadoOuCriadoIntegrationEvent>> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    protected override async Task Handle(EstabelecimentoEditadoOuCriadoIntegrationEvent evt)
    {
        var cmd = new EstabelecimentoEditadoOuCriadoCommand(evt.EstabelecimentoId, evt.DominioId, evt.Nome, evt.Cnes, evt.Redes, evt.IsAtivo, evt.CreationDate);
        await Mediator.Send(cmd);
    }
}
