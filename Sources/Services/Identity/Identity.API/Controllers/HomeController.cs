namespace Pulsar.Services.Identity.API.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public ActionResult Index()
    {
        return Redirect("swagger/index.html");
    }
}
