using Pulsar.Services.Identity.Contracts.Commands;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v1/esqueci_minha_senha")]
[ApiExplorerSettings(IgnoreApi = true)]
public class EsqueciMinhaSenhaController : IdentityController
{
    public EsqueciMinhaSenhaController(IdentityControllerContext context) : base(context)
    {
    }

    [HttpPost]
    public async Task<ActionResult> EsqueciMinhaSenha([FromBody] EsqueciMinhaSenhaCommand cmd)
    {
        await Mediator.Send(cmd);
        return Ok();
    }
}
