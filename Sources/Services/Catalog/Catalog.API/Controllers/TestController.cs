using Pulsar.Services.Identity.Contracts.Commands.Convites;
using Pulsar.Services.Identity.Contracts.Enumerations;

namespace Pulsar.Services.Catalog.API.Controllers;

[ApiController]
[Route("v2/test")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class TestController : CatalogController
{
    public TestController(CatalogControllerContext context) : base(context)
    {
    }

    [HttpGet]
    public async Task<ActionResult> Test()
    {
        return Ok();
    }
}
