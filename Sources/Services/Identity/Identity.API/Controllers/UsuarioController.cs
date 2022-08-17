using Microsoft.AspNetCore.Authorization;
using Pulsar.Services.Identity.Contracts.Commands;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v1/usuarios")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class UsuarioController : IdentityController
{
    public UsuarioController(IdentityControllerContext context) : base(context)
    {
    }

    //TODO: Remove later...
    [HttpGet]
    public ActionResult GetOk()
    {
        return Ok();
    }
}