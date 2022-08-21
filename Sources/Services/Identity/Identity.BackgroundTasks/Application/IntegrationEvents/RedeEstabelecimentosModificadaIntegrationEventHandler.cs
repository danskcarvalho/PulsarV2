using Pulsar.Services.Identity.Contracts.Commands.Others;

namespace Pulsar.Services.Identity.BackgroundTasks.Application.IntegrationEvents;

public class RedeEstabelecimentosModificadaIntegrationEventHandler : IdentityIntegrationEventHandler<RedeEstabelecimentosModificadaIntegrationEvent>
{
    public RedeEstabelecimentosModificadaIntegrationEventHandler(ILogger<IdentityIntegrationEventHandler<RedeEstabelecimentosModificadaIntegrationEvent>> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    protected override async Task Handle(RedeEstabelecimentosModificadaIntegrationEvent evt)
    {
        var cmd = new RedeEstabelecimentosEditadaOuCriadaCommand(evt.RedeEstabelecimentosId, evt.DominioId, evt.Nome, evt.CreationDate, evt.AuditInfo);
        await Mediator.Send(cmd);
    }
}
