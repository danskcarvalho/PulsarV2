using Pulsar.Services.Identity.Contracts.Commands.Convites;

namespace Pulsar.Services.Identity.API.Controllers;
[ApiController]
[Route("v2/aceitar_convite")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AceitarConviteController : IdentityController
{
    public AceitarConviteController(IdentityControllerContext context) : base(context)
    {
    }

    [HttpPost]
    public async Task<ActionResult> Aceitar([FromBody] AceitarConviteCommand cmd)
    {
        await Mediator.Send(cmd);
        return Ok();
    }
}
