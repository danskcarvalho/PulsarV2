using Microsoft.AspNetCore.Authorization;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v1/usuarios")]
public class UsuarioController : ControllerBase
{
    private readonly ILogger<UsuarioController> _logger;
    private readonly IUsuarioQueries _usuarioQueries;

    public UsuarioController(ILogger<UsuarioController> logger, IUsuarioQueries usuarioQueries)
    {
        _logger = logger;
        _usuarioQueries = usuarioQueries;
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("test")]
    [HttpPost]
    public ActionResult Test()
    {
        return Ok();
    }
}