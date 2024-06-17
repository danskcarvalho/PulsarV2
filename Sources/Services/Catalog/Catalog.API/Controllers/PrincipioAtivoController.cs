using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Enumerations;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.API.Controllers;

[ApiController]
[Route("v2/principiosativos")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class PrincipioAtivoController : CatalogController
{
    public PrincipioAtivoController(CatalogControllerContext context) : base(context)
    {
    }

    [HttpGet, ScopeAuthorize("catalog.principiosativos.listar")]
    public async Task<ActionResult<List<PrincipioAtivoDTO>>> Get(string? filtro)
    {
        return Ok(await PrincipioAtivoQueries.Find(filtro));
    }
}
