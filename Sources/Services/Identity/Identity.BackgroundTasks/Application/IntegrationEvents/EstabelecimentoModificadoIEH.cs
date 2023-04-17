//using Pulsar.Services.Identity.Contracts.Commands.Others;

//namespace Pulsar.Services.Identity.BackgroundTasks.Application.IntegrationEvents;

//public class EstabelecimentoModificadoIEH : IdentityIntegrationEventHandler<EstabelecimentoModificadoIE>
//{
//    public EstabelecimentoModificadoIEH(ILogger<IdentityIntegrationEventHandler<EstabelecimentoModificadoIE>> logger, IMediator mediator) : base(logger, mediator)
//    {
//    }

//    protected override async Task Handle(EstabelecimentoModificadoIE evt)
//    {
//        var cmd = new EstabelecimentoModificadoCmd(evt.EstabelecimentoId, evt.DominioId, evt.Nome, evt.Cnes, evt.Redes, evt.IsAtivo, evt.CreationDate);
//        await Mediator.Send(cmd);
//    }
//}
