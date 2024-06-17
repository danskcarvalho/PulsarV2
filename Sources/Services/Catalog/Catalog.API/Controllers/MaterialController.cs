using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Enumerations;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.API.Controllers;

[ApiController]
[Route("v2/materiais")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class MaterialController : CatalogController
{
    public MaterialController(CatalogControllerContext context) : base(context)
    {
    }

    [HttpGet, ScopeAuthorize("catalog.materiais.listar")]
    public async Task<ActionResult<List<MaterialDTO>>> Get(string? filtro, TipoMaterial? tipo, string? principioAtivoId)
    {
        return Ok(await MaterialQueries.Find(filtro, tipo, principioAtivoId));
    }
}
