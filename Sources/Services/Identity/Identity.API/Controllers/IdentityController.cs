namespace Pulsar.Services.Identity.API.Controllers;

public class IdentityController : ControllerBase
{
    public IUsuarioQueries UsuarioQueries { get; }
    public IMediator Mediator { get; }

    public IdentityController(IMediator mediator, IUsuarioQueries usuarioQueries)
    {
        Mediator = mediator;
        UsuarioQueries = usuarioQueries;
    }
}
