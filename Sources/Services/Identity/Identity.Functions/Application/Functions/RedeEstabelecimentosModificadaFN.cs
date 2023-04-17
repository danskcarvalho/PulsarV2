namespace Pulsar.Services.Identity.Functions.Application.Functions;

public class RedeEstabelecimentosModificadaFN : IdentityFunction
{
    private readonly ILogger _logger;

    public RedeEstabelecimentosModificadaFN(ILoggerFactory loggerFactory, IMediator mediator) : base(mediator)
    {
        _logger = loggerFactory.CreateLogger<RedeEstabelecimentosModificadaFN>();
    }

    [Function("RedeEstabelecimentosModificadaFN")]
    public async Task Run([ServiceBusTrigger("%ServiceBusDeveloper%.Facilities", "RedeEstabelecimentosModificadaFN.Identity", Connection = "ServiceBus")] RedeEstabelecimentosModificadaIE evt)
    {
        var cmd = new RedeEstabelecimentosEditadaOuCriadaCommand(evt.RedeEstabelecimentosId, evt.DominioId, evt.Nome, evt.CreationDate, evt.AuditInfo);
        await Mediator.Send(cmd);
    }
}
