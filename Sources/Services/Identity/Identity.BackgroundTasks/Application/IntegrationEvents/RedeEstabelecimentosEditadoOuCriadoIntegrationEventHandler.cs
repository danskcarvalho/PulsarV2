using Pulsar.Services.Identity.Contracts.Commands.Others;

namespace Pulsar.Services.Identity.BackgroundTasks.Application.IntegrationEvents;

public class RedeEstabelecimentosEditadoOuCriadoIntegrationEventHandler : IdentityIntegrationEventHandler<RedeEstabelecimentosEditadoOuCriadoIntegrationEvent>
{
    public RedeEstabelecimentosEditadoOuCriadoIntegrationEventHandler(ILogger<IdentityIntegrationEventHandler<RedeEstabelecimentosEditadoOuCriadoIntegrationEvent>> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    protected override async Task Handle(RedeEstabelecimentosEditadoOuCriadoIntegrationEvent evt)
    {
        var cmd = new RedeEstabelecimentosEditadoOuCriadoCommand(evt.RedeEstabelecimentosId, evt.DominioId, evt.Nome, evt.CreationDate);
        await Mediator.Send(cmd);
    }
}
