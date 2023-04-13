using Pulsar.Services.Identity.Contracts.Commands.Usuarios;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v2/esqueci_minha_senha")]
[ApiExplorerSettings(IgnoreApi = true)]
public class EsqueciMinhaSenhaController : IdentityController
{
    public EsqueciMinhaSenhaController(IdentityControllerContext context) : base(context)
    {
    }

    [HttpPost]
    public async Task<ActionResult> EsqueciMinhaSenha([FromBody] EsqueciMinhaSenhaCmd cmd)
    {
        await Mediator.Send(cmd);
        return Ok();
    }

    [HttpPost("recuperar")]
    public async Task<ActionResult> RecuperarSenha([FromBody] RecuperarSenhaCmd cmd)
    {
        await Mediator.Send(cmd);
        return Ok();
    }
}
