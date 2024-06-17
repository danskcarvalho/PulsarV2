using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Enumerations;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.API.Controllers;

[ApiController]
[Route("v2/diagnosticos")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class DiagnosticoController : CatalogController
{
    public DiagnosticoController(CatalogControllerContext context) : base(context)
    {
    }

    /// <summary>
    /// Lista todos os diagnósticos.
    /// </summary>
    /// <param name="filtro">Filtro. Obrigatório.</param>
    /// <param name="sexo">Sexo.</param>
    /// <param name="tipo">Tipo.</param>
    /// <returns></returns>
    [HttpGet, ScopeAuthorize("catalog.diagnosticos.listar")]
    public async Task<ActionResult<List<DenteDTO>>> Get(string? filtro, Sexo? sexo, TipoDiagnostico? tipo)
    {
        return Ok(await DiagnosticoQueries.Find(filtro, sexo, tipo));
    }
}
