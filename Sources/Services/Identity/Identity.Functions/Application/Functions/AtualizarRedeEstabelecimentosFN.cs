namespace Pulsar.Services.Identity.Functions.Application.Functions;

public class AtualizarRedeEstabelecimentosFN : IdentityFunction
{
    public AtualizarRedeEstabelecimentosFN(IMediator mediator) : base(mediator)
    {
    }

    [Function("AtualizarRedeEstabelecimentosFN")]
    public async Task Run([ServiceBusTrigger("%ServiceBusDeveloper%.Facility", "AtualizarRedeEstabelecimentosFN.Identity", Connection = "ServiceBus")] RedeEstabelecimentosModificadaIE evt)
    {
        var cmd = new AtualizarRedeEstabelecimentosCmd(evt.RedeEstabelecimentosId, evt.DominioId, evt.Nome, evt.CreationDate, evt.AuditInfo);
        await Mediator.Send(cmd);
    }
}
