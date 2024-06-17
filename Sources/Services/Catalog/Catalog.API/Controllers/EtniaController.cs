using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Enumerations;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.API.Controllers;

[ApiController]
[Route("v2/etnias")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class EtniaController : CatalogController
{
    public EtniaController(CatalogControllerContext context) : base(context)
    {
    }

    [HttpGet, ScopeAuthorize("catalog.etnias.listar")]
    public async Task<ActionResult<List<EtniaDTO>>> Get(string? filtro)
    {
        return Ok(await EtniaQueries.Find(filtro));
    }
}
