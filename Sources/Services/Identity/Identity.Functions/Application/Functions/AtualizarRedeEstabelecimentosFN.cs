namespace Pulsar.Services.Identity.Functions.Application.Functions;

public class AtualizarRedeEstabelecimentosFN : IdentityFunction
{
    private readonly ILogger _logger;

    public AtualizarRedeEstabelecimentosFN(ILoggerFactory loggerFactory, IMediator mediator) : base(mediator)
    {
        _logger = loggerFactory.CreateLogger<AtualizarRedeEstabelecimentosFN>();
    }

    [Function("AtualizarRedeEstabelecimentosFN")]
    public async Task Run([ServiceBusTrigger("%ServiceBusDeveloper%.Estabelecimentos", "AtualizarRedeEstabelecimentosFN.Identity", Connection = "ServiceBus")] RedeEstabelecimentosModificadaIE evt)
    {
        var cmd = new AtualizarRedeEstabelecimentosCmd(evt.RedeEstabelecimentosId, evt.DominioId, evt.Nome, evt.CreationDate, evt.AuditInfo);
        await Mediator.Send(cmd);
    }
}
