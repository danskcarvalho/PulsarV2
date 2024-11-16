namespace Pulsar.Services.PushNotification.API.Utils;

public class BaseController : ControllerBase
{
    private readonly BaseControllerContext _context;
    public IMediator Mediator => _context.Mediator;
    public IConfiguration Configuration => _context.Configuration;
    public IPushNotificationQueries EstabelecimentoQueries => _context.EstabelecimentoQueries;
    public BaseController(BaseControllerContext context)
    {
        _context = context;
    }
}
