namespace Pulsar.Services.Identity.Functions.Application.Functions;

public class EstabelecimentoModificadoFN : IdentityFunction
{
    private readonly ILogger _logger;

    public EstabelecimentoModificadoFN(ILoggerFactory loggerFactory, IMediator mediator) : base(mediator)
    {
        _logger = loggerFactory.CreateLogger<EstabelecimentoModificadoFN>();
    }

    [Function("EstabelecimentoModificadoFN")]
    public async Task Run([ServiceBusTrigger("%ServiceBusDeveloper%.Estabelecimentos", "EstabelecimentoModificadoFN.Identity", Connection = "ServiceBus")] EstabelecimentoModificadoIE evt)
    {
        var cmd = new EstabelecimentoModificadoCmd(evt.EstabelecimentoId, evt.DominioId, evt.Nome, evt.Cnes, evt.Redes, evt.IsAtivo, evt.CreationDate);
        await Mediator.Send(cmd);
    }
}
