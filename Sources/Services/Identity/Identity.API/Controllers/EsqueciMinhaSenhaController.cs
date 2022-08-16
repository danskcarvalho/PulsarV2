using Pulsar.Services.Identity.Contracts.Commands;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v1/esqueci_minha_senha")]
[ApiExplorerSettings(IgnoreApi = true)]
public class EsqueciMinhaSenhaController : IdentityController
{
    private IMediator _mediator;
    public EsqueciMinhaSenhaController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult> EsqueciMinhaSenha([FromBody] EsqueciMinhaSenhaCommand cmd)
    {
        await _mediator.Send(cmd);
        return Ok();
    }
}
