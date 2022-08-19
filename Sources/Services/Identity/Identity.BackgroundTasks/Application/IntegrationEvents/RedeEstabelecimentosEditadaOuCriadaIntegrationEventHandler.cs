using Pulsar.Services.Identity.Contracts.Commands.Others;

namespace Pulsar.Services.Identity.BackgroundTasks.Application.IntegrationEvents;

public class RedeEstabelecimentosEditadaOuCriadaIntegrationEventHandler : IdentityIntegrationEventHandler<RedeEstabelecimentosEditadaOuCriadaIntegrationEvent>
{
    public RedeEstabelecimentosEditadaOuCriadaIntegrationEventHandler(ILogger<IdentityIntegrationEventHandler<RedeEstabelecimentosEditadaOuCriadaIntegrationEvent>> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    protected override async Task Handle(RedeEstabelecimentosEditadaOuCriadaIntegrationEvent evt)
    {
        var cmd = new RedeEstabelecimentosEditadaOuCriadaCommand(evt.RedeEstabelecimentosId, evt.DominioId, evt.Nome, evt.CreationDate, evt.AuditInfo);
        await Mediator.Send(cmd);
    }
}
