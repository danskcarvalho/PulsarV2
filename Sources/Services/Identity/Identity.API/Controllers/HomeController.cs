namespace Pulsar.Services.Identity.API.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : ControllerBase
{
    public HomeController()
    {
    }

    public ActionResult Index()
    {
        return Redirect("swagger/index.html");
    }
}
