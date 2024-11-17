namespace Pulsar.Services.PushNotification.API.Utils;

public class BaseController : ControllerBase
{
    private readonly BaseControllerContext _context;
    public IMediator Mediator => _context.Mediator;
    public IConfiguration Configuration => _context.Configuration;
    public INotificacaoPushQueries NotificacoesPushQueries => _context.NotificacoesPushQueries;
    public BaseController(BaseControllerContext context)
    {
        _context = context;
    }
}
