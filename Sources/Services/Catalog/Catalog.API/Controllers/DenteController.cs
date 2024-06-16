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

    /// <summary>
    /// Lista os dentes de acordo com o filtro. Se o filtro for vazio, nenhum dente será retornado.
    /// </summary>
    /// <param name="filtro">Filtro ou código do dente.</param>
    /// <returns>Dentes.</returns>
    [HttpGet, ScopeAuthorize("catalog.dentes.listar")]
    public async Task<ActionResult<List<DenteDTO>>> Get(string? filtro)
    {
        return Ok(await DenteQueries.Find(filtro));
    }

    /// <summary>
    /// Lista todos os dentes.
    /// </summary>
    /// <returns>Todos os dentes.</returns>
    [HttpGet("todos"), ScopeAuthorize("catalog.dentes.listar")]
    public async Task<ActionResult<List<DenteDTO>>> GetAll()
    {
        return Ok(await DenteQueries.FindAll());
    }
}
