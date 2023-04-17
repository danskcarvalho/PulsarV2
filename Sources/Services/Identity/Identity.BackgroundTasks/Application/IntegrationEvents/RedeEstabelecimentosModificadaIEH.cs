//using Pulsar.Services.Identity.Contracts.Commands.Others;

//namespace Pulsar.Services.Identity.BackgroundTasks.Application.IntegrationEvents;

//public class RedeEstabelecimentosModificadaIEH : IdentityIntegrationEventHandler<RedeEstabelecimentosModificadaIE>
//{
//    public RedeEstabelecimentosModificadaIEH(ILogger<IdentityIntegrationEventHandler<RedeEstabelecimentosModificadaIE>> logger, IMediator mediator) : base(logger, mediator)
//    {
//    }

//    protected override async Task Handle(RedeEstabelecimentosModificadaIE evt)
//    {
//        var cmd = new RedeEstabelecimentosEditadaOuCriadaCommand(evt.RedeEstabelecimentosId, evt.DominioId, evt.Nome, evt.CreationDate, evt.AuditInfo);
//        await Mediator.Send(cmd);
//    }
//}
