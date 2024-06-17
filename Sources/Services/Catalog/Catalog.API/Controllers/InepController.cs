using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Enumerations;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.API.Controllers;

[ApiController]
[Route("v2/ineps")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class InepController : CatalogController
{
    public InepController(CatalogControllerContext context) : base(context)
    {
    }

    [HttpGet, ScopeAuthorize("catalog.ineps.listar")]
    public async Task<ActionResult<List<InepDTO>>> Get(string? filtro)
    {
        return Ok(await InepQueries.Find(filtro));
    }
}
