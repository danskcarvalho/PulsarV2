using Microsoft.AspNetCore.Authorization;
using Pulsar.Services.Identity.Contracts.Commands;
using Pulsar.Services.Shared.DTOs;
using System.Security.Claims;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v1/usuarios")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class UsuarioController : IdentityController
{
    public UsuarioController(IdentityControllerContext context) : base(context)
    {
    }

    [HttpGet, ReadAuthorize, SuperUsuarioOrDominioAuthorize(PermissoesDominio.ListarUsuarios)]
    public async Task<ActionResult<PaginatedListDTO<UsuarioListadoDTO>>> FindUsuarios([FromQuery] string? filtro, [FromQuery] string? cursor, [FromQuery] int? limit, [FromQuery] string? consistencyToken)
    {
        var r = await UsuarioQueries.FindUsuarios(new UsuarioFiltroDTO()
        {
            ConsistencyToken = consistencyToken,
            Cursor = cursor,
            Filtro = filtro,
            Limit = limit
        });
        return Ok(r);
    }
}