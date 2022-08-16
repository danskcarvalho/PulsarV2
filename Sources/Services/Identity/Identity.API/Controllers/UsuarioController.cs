using Microsoft.AspNetCore.Authorization;
using Pulsar.Services.Identity.Contracts.Commands;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v1/usuarios")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class UsuarioController : IdentityController
{
    private readonly ILogger<UsuarioController> _logger;
    private readonly IMediator _mediator;
    private readonly IUsuarioQueries _usuarioQueries;
    public UsuarioController(ILogger<UsuarioController> logger, IMediator mediator, IUsuarioQueries usuarioQueries)
    {
        _mediator = mediator;
        _usuarioQueries = usuarioQueries;
        _logger = logger;
    }

}