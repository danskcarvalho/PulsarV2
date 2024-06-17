using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Enumerations;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.API.Controllers;

[ApiController]
[Route("v2/especialidades")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class EspecialidadeController : CatalogController
{
    public EspecialidadeController(CatalogControllerContext context) : base(context)
    {
    }

    [HttpGet, ScopeAuthorize("catalog.especialidades.listar")]
    public async Task<ActionResult<List<EspecialidadeDTO>>> Get(string? filtro)
    {
        return Ok(await EspecialidadeQueries.Find(filtro));
    }
}
