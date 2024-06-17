using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Enumerations;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.API.Controllers;

[ApiController]
[Route("v2/regioes")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class RegiaoController : CatalogController
{
    public RegiaoController(CatalogControllerContext context) : base(context)
    {
    }

    [HttpGet, ScopeAuthorize("catalog.regioes.listar")]
    public async Task<ActionResult<List<RegiaoDTO>>> Get(string? filtro, TipoLocal? tipo, string? regiaoPaiId)
    {
        return Ok(await RegiaoQueries.Find(filtro, tipo, regiaoPaiId));
    }
}
