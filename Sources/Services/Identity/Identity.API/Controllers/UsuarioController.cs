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

    /// <summary>
    /// Lista todos os usuários. Exceto o usuário administrador.
    /// </summary>
    /// <param name="filtro">Filtro. Opcional.</param>
    /// <param name="cursor">Cursor.Opcional.</param>
    /// <param name="limit">Limite.Opcional.</param>
    /// <param name="consistencyToken">Token de consistência. Opcional.</param>
    /// <returns>Lista de todos os usuários.</returns>
    [HttpGet, ScopeAuthorize("usuarios.listar"), SuperUsuarioOrDominioAuthorize(PermissoesDominio.ListarUsuarios)]
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

    /// <summary>
    /// Retorna o usuário logado.
    /// </summary>
    /// <param name="consistencyToken">Token de consistência. Opcional.</param>
    /// <returns>Usuário logado.</returns>
    [HttpGet("logado"), ScopeAuthorize("usuarios.logado")]
    public async Task<ActionResult<BasicUserInfoDTO>> Logado([FromQuery] string? consistencyToken)
    {
        var r = await UsuarioQueries.GetBasicUserInfo(User.Id(), consistencyToken);
        return Ok(r);
    }

    /// <summary>
    /// Retorna todos os usuários com os dados básicos informados.
    /// </summary>
    /// <param name="ids">Ids dos usuários.</param>
    /// <param name="consistencyToken">Token de consistência. Opcional.</param>
    /// <returns>Dados básicos.</returns>
    [HttpGet("dados_basicos"), ScopeAuthorize("usuarios.dados_basicos"), SuperUsuarioOrDominioAuthorize(PermissoesDominio.ListarUsuarios)]
    public async Task<ActionResult<List<BasicUserInfoDTO>>> DadosBasicos([FromQuery(Name = "id")] string[] ids, [FromQuery] string? consistencyToken)
    {
        var r = await UsuarioQueries.GetBasicUsersInfo(ids, consistencyToken);
        return Ok(r);
    }

    /// <summary>
    /// Retorna dados detalhados de todos os usuários.
    /// </summary>
    /// <param name="ids">Ids dos usuários.</param>
    /// <param name="consistencyToken">Token de consistência. Opcional.</param>
    /// <returns>Dados detalhados.</returns>
    [HttpGet("detalhes"), ScopeAuthorize("usuarios.detalhes"), SuperUsuarioOrDominioAuthorize(PermissoesDominio.ListarUsuarios)]
    public async Task<ActionResult<List<UsuarioDetalhesDTO>>> Detalhes([FromQuery(Name = "id")] string[] ids, [FromQuery] string? consistencyToken)
    {
        var r = await UsuarioQueries.GetUsuarioDetalhes(ids, consistencyToken);
        return Ok(r);
    }
}