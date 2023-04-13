using Pulsar.Services.Identity.Contracts.Commands.Dominios;
using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v2/dominios")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class DominioController : IdentityController
{
    public DominioController(IdentityControllerContext context) : base(context)
    {
    }

    /// <summary>
    /// Lista todos os domínios.
    /// </summary>
    /// <param name="filtro">Filtro de texto. Opcional.</param>
    /// <param name="cursor">Cursor. Opcional.</param>
    /// <param name="limit">Limite de domínios retornados. Opcional.</param>
    /// <param name="consistencyToken">Token de consistência. Opcional.</param>
    /// <returns>Domínios.</returns>

    [HttpGet, ScopeAuthorize("dominios.listar"), SuperUsuarioAuthorize]
    public async Task<ActionResult<PaginatedListDTO<DominioListadoDTO>>> FindDominios([FromQuery] string? filtro, [FromQuery] string? cursor, [FromQuery] int? limit, [FromQuery] string? consistencyToken)
    {
        var r = await DominioQueries.FindDominios(filtro, cursor, limit, consistencyToken);
        return Ok(r);
    }

    /// <summary>
    /// Retorna os detalhes dos domínios com os ids informados.
    /// </summary>
    /// <param name="ids">Ids dos usuários.</param>
    /// <param name="consistencyToken">Token de consistência. Opcional.</param>
    /// <returns></returns>
    [HttpGet("detalhes"), ScopeAuthorize("dominios.detalhes"), SuperUsuarioAuthorize]
    public async Task<ActionResult<List<DominioDetalhesDTO>>> Detalhes([FromQuery(Name = "id")] string[] ids, [FromQuery] string? consistencyToken)
    {
        var r = await DominioQueries.GetDominioDetalhes(ids, consistencyToken);
        return Ok(r);
    }

    /// <summary>
    /// Retorna os detalhes do domínio logado.
    /// </summary>
    /// <returns></returns>
    [HttpGet("logado")]
    public async Task<ActionResult<List<DominioDetalhesDTO>>> Logado()
    {
        var dominioId = User.DominioId();
        if (dominioId == null)
            throw new IdentityDomainException(ExceptionKey.DominioNaoLogado);
        var r = await DominioQueries.GetDominioDetalhes(new string[] { dominioId }, null);
        return Ok(r);
    }

    /// <summary>
    /// Retorna os usuários bloqueados no domínio informado.
    /// </summary>
    /// <param name="dominioId">Id do domínio.</param>
    /// <param name="filtro">Filtro de texto. Opcional.</param>
    /// <param name="cursor">Cursor. Opcional.</param>
    /// <param name="limit">Limite de domínios retornados. Opcional.</param>
    /// <param name="consistencyToken">Token de consistência. Opcional.</param>
    /// <returns>Ok.</returns>
    [HttpGet("{dominioId}/usuarios_bloqueados"), ScopeAuthorize("dominios.usuarios_bloqueados"), SuperUsuarioAuthorize]
    public async Task<ActionResult<PaginatedListDTO<BasicUserInfoDTO>>> UsuariosBloqueados(string dominioId, [FromQuery] string? filtro, [FromQuery] string? cursor, [FromQuery] int? limit, [FromQuery] string? consistencyToken)
    {
        var r = await DominioQueries.FindUsuariosBloqueados(dominioId, filtro, cursor, limit, consistencyToken);
        return Ok(r);
    }

    /// <summary>
    /// Retorna os usuários bloqueados no domínio logado.
    /// </summary>
    /// <param name="filtro">Filtro de texto. Opcional.</param>
    /// <param name="cursor">Cursor. Opcional.</param>
    /// <param name="limit">Limite de domínios retornados. Opcional.</param>
    /// <param name="consistencyToken">Token de consistência. Opcional.</param>
    /// <returns>Ok.</returns>
    [HttpGet("usuarios_bloqueados"), ScopeAuthorize("dominios.usuarios_bloqueados"), DominioAuthorize(PermissoesDominio.BloquearUsuarios)]
    public async Task<ActionResult<PaginatedListDTO<BasicUserInfoDTO>>> UsuariosBloqueados([FromQuery] string? filtro, [FromQuery] string? cursor, [FromQuery] int? limit, [FromQuery] string? consistencyToken)
    {
        var r = await DominioQueries.FindUsuariosBloqueados(User.DominioId()!, filtro, cursor, limit, consistencyToken);
        return Ok(r);
    }

    /// <summary>
    /// Bloqueia um domínio.
    /// </summary>
    /// <param name="dominioId">Id do domínio a ser bloqueado.</param>
    /// <returns>Ok.</returns>
    /// <exception cref="ArgumentNullException">dominioId is null</exception>
    [HttpPost("{dominioId}/bloquear"), ScopeAuthorize("dominios.bloquear"), SuperUsuarioAuthorize]
    public async Task<ActionResult<CommandResult>> Bloquear(string dominioId)
    {
        var r = await Mediator.Send(new BloquearOuDesbloquearDominioCmd(User.Id(), dominioId ?? throw new ArgumentNullException(nameof(dominioId)), true));
        return Ok(r);
    }

    /// <summary>
    /// Desbloqueia um domínio.
    /// </summary>
    /// <param name="dominioId">Id do domínio a ser desbloqueado.</param>
    /// <returns>Ok.</returns>
    /// <exception cref="ArgumentNullException">dominioId is null</exception>
    [HttpPost("{dominioId}/desbloquear"), ScopeAuthorize("dominios.desbloquear"), SuperUsuarioAuthorize]
    public async Task<ActionResult<CommandResult>> Desbloquear(string dominioId)
    {
        var r = await Mediator.Send(new BloquearOuDesbloquearDominioCmd(User.Id(), dominioId ?? throw new ArgumentNullException(nameof(dominioId)), false));
        return Ok(r);
    }

    /// <summary>
    /// Bloqueia usuários em um domínio.
    /// </summary>
    /// <param name="dominioId">Id do domínio.</param>
    /// <param name="cmd">Dados.</param>
    /// <returns>Ok.</returns>
    /// <exception cref="ArgumentNullException">dominioId is null</exception>
    [HttpPost("{dominioId}/bloquear_usuarios"), ScopeAuthorize("dominios.bloquear_usuarios"), SuperUsuarioAuthorize]
    public async Task<ActionResult<CommandResult>> BloquearUsuarios(string dominioId, [FromBody]BloquearOuDesbloquearUsuariosNoDominioCmd cmd)
    {
        cmd.Bloquear = true;
        cmd.UsuarioLogadoId = User.Id();
        cmd.DominioId = dominioId ?? throw new ArgumentNullException(nameof(dominioId));
        var r = await Mediator.Send(cmd);
        return Ok(r);
    }

    /// <summary>
    /// Desbloqueia usuários em um domínio.
    /// </summary>
    /// <param name="dominioId">Id do domínio.</param>
    /// <param name="cmd">Dados.</param>
    /// <returns>Ok.</returns>
    /// <exception cref="ArgumentNullException">dominioId is null</exception>
    [HttpPost("{dominioId}/desbloquear_usuarios"), ScopeAuthorize("dominios.desbloquear_usuarios"), SuperUsuarioAuthorize]
    public async Task<ActionResult<CommandResult>> DesbloquearUsuarios(string dominioId, [FromBody] BloquearOuDesbloquearUsuariosNoDominioCmd cmd)
    {
        cmd.Bloquear = false;
        cmd.UsuarioLogadoId = User.Id();
        cmd.DominioId = dominioId ?? throw new ArgumentNullException(nameof(dominioId));
        var r = await Mediator.Send(cmd);
        return Ok(r);
    }

    /// <summary>
    /// Edita um domínio.
    /// </summary>
    /// <param name="dominioId">Id do domínio.</param>
    /// <param name="cmd">Dados.</param>
    /// <returns>Ok.</returns>
    /// <exception cref="ArgumentNullException">dominioId is null</exception>
    [HttpPost("{dominioId}"), ScopeAuthorize("dominios.editar"), SuperUsuarioAuthorize]
    public async Task<ActionResult<CommandResult>> Editar(string dominioId, [FromBody] EditarDominioCmd cmd)
    {
        cmd.UsuarioLogadoId = User.Id();
        cmd.DominioId = dominioId ?? throw new ArgumentNullException(nameof(dominioId));
        var r = await Mediator.Send(cmd);
        return Ok(r);
    }

    /// <summary>
    /// Cria um novo domínio.
    /// </summary>
    /// <param name="cmd">Dados.</param>
    /// <returns>Id do domínio criado.</returns>

    [HttpPut, ScopeAuthorize("dominios.criar"), SuperUsuarioAuthorize]
    public async Task<ActionResult<CreatedCommandResult>> Criar([FromBody] CriarDominioCmd cmd)
    {
        cmd.UsuarioLogadoId = User.Id();
        var r = await Mediator.Send(cmd);
        return Ok(r);
    }

    /// <summary>
    /// Bloqueia usuários no domínio logado.
    /// </summary>
    /// <param name="cmd">Dados.</param>
    /// <returns>Ok.</returns>
    [HttpPost("bloquear_usuarios"), ScopeAuthorize("dominios.bloquear_usuarios"), DominioAuthorize(PermissoesDominio.BloquearUsuarios)]
    public async Task<ActionResult<CommandResult>> BloquearUsuarios([FromBody] BloquearOuDesbloquearUsuariosNoDominioCmd cmd)
    {
        cmd.Bloquear = true;
        cmd.UsuarioLogadoId = User.Id();
        cmd.DominioId = User.DominioId();
        var r = await Mediator.Send(cmd);
        return Ok(r);
    }

    /// <summary>
    /// Desbloqueia usuários no domínio logado.
    /// </summary>
    /// <param name="cmd">Dados.</param>
    /// <returns>Ok.</returns>
    [HttpPost("desbloquear_usuarios"), ScopeAuthorize("dominios.desbloquear_usuarios"), DominioAuthorize(PermissoesDominio.BloquearUsuarios)]
    public async Task<ActionResult<CommandResult>> DesbloquearUsuarios([FromBody] BloquearOuDesbloquearUsuariosNoDominioCmd cmd)
    {
        cmd.Bloquear = false;
        cmd.UsuarioLogadoId = User.Id();
        cmd.DominioId = User.DominioId();
        var r = await Mediator.Send(cmd);
        return Ok(r);
    }
}
