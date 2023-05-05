namespace Pulsar.Services.Identity.Functions.Application.Functions;

public class AtualizarEstabelecimentoFN : IdentityFunction
{
    public AtualizarEstabelecimentoFN(IMediator mediator) : base(mediator)
    {
    }

    [Function("AtualizarEstabelecimentoFN")]
    public async Task Run([ServiceBusTrigger("%ServiceBusDeveloper%.Estabelecimentos", "AtualizarEstabelecimentoFN.Identity", Connection = "ServiceBus")] EstabelecimentoModificadoIE evt)
    {
        var cmd = new AtualizarEstabelecimentoCmd(evt.EstabelecimentoId, evt.DominioId, evt.Nome, evt.Cnes, evt.Redes, evt.IsAtivo, evt.CreationDate, evt.AuditInfo);
        await Mediator.Send(cmd);
    }
}
