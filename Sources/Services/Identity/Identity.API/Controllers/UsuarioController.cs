using Microsoft.AspNetCore.Authorization;
using Pulsar.Services.Identity.Contracts.Commands;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v1/usuarios")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class UsuarioController : IdentityController
{
    private readonly ILogger<UsuarioController> _logger;
    public UsuarioController(ILogger<UsuarioController> logger, IMediator mediator, IUsuarioQueries usuarioQueries) : base(mediator, usuarioQueries)
    {
        _logger = logger;
    }

}