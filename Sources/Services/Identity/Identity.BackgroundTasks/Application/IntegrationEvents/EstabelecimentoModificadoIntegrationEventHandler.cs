using Pulsar.Services.Identity.Contracts.Commands.Others;

namespace Pulsar.Services.Identity.BackgroundTasks.Application.IntegrationEvents;

public class EstabelecimentoModificadoIntegrationEventHandler : IdentityIntegrationEventHandler<EstabelecimentoModificadoIntegrationEvent>
{
    public EstabelecimentoModificadoIntegrationEventHandler(ILogger<IdentityIntegrationEventHandler<EstabelecimentoModificadoIntegrationEvent>> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    protected override async Task Handle(EstabelecimentoModificadoIntegrationEvent evt)
    {
        var cmd = new EstabelecimentoModificadoCommand(evt.EstabelecimentoId, evt.DominioId, evt.Nome, evt.Cnes, evt.Redes, evt.IsAtivo, evt.CreationDate);
        await Mediator.Send(cmd);
    }
}
