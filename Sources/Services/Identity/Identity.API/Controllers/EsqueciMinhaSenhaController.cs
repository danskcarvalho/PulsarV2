using Pulsar.Services.Identity.Contracts.Commands.Usuarios;

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

    [HttpPost("recuperar")]
    public async Task<ActionResult> RecuperarSenha([FromBody] RecuperarSenhaCommand cmd)
    {
        await Mediator.Send(cmd);
        return Ok();
    }
}
