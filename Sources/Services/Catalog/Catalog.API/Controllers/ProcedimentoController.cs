using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Enumerations;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.API.Controllers;

[ApiController]
[Route("v2/procedimentos")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class ProcedimentoController : CatalogController
{
    public ProcedimentoController(CatalogControllerContext context) : base(context)
    {
    }

    [HttpGet, ScopeAuthorize("catalog.procedimentos.listar")]
    public async Task<ActionResult<List<ProcedimentoDTO>>> Get(string? filtro, Sexo? sexo, Complexidade? complexidade, int? idadeEmDias, bool? procedimentoAb)
    {
        return Ok(await ProcedimentoQueries.Find(filtro, sexo, complexidade, idadeEmDias, procedimentoAb));
    }
}
