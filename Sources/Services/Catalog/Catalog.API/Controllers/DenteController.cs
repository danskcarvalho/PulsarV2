using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Identity.Contracts.Commands.Convites;
using Pulsar.Services.Identity.Contracts.Enumerations;

namespace Pulsar.Services.Catalog.API.Controllers;

[ApiController]
[Route("v2/dentes")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class DenteController : CatalogController
{
    public DenteController(CatalogControllerContext context) : base(context)
    {
    }

    [HttpGet, ScopeAuthorize("catalog.dentes.listar")]
    public async Task<ActionResult<List<DenteDTO>>> Get(string? filtro)
    {
        return Ok(await DenteQueries.Find(filtro));
    }

    [HttpGet("todos"), ScopeAuthorize("catalog.dentes.listar")]
    public async Task<ActionResult<List<DenteDTO>>> GetAll()
    {
        return Ok(await DenteQueries.FindAll());
    }
}
