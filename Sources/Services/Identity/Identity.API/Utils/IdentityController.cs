namespace Pulsar.Services.Identity.API.Controllers;

public class IdentityController : ControllerBase
{
    private readonly IdentityControllerContext _context;
    public IMediator Mediator => _context.Mediator;
    public IConfiguration Configuration => _context.Configuration; 
    public IUsuarioQueries UsuarioQueries => _context.UsuarioQueries;
    public IDominioQueries DominioQueries => _context.DominioQueries;
    public IGrupoQueries GrupoQueries => _context.GrupoQueries;
    public IdentityController(IdentityControllerContext context)
    {
        _context = context;
    }
}

public class IdentityControllerContext
{
    public IMediator Mediator { get; }
    public IUsuarioQueries UsuarioQueries { get; }
    public IDominioQueries DominioQueries { get; }
    public IGrupoQueries GrupoQueries { get; }
    public IConfiguration Configuration { get; }

    public IdentityControllerContext(IMediator mediator, IUsuarioQueries usuarioQueries, IDominioQueries dominioQueries, IGrupoQueries grupoQueries, IConfiguration configuration)
    {
        Mediator = mediator;
        UsuarioQueries = usuarioQueries;
        DominioQueries = dominioQueries;
        Configuration = configuration;
        GrupoQueries = grupoQueries;
    }
}
