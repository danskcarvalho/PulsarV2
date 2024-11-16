using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v2/usuarios")]
[Authorize(Policy = "InferAuthenticationSchemes")]
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
    [HttpGet, ScopeAuthorize("identity.usuarios.listar"), SuperUsuarioOrDominioAuthorize(PermissoesDominio.ListarUsuarios)]
    public async Task<ActionResult<PaginatedListDTO<UsuarioListadoDTO>>> FindUsuarios([FromQuery] string? filtro, [FromQuery] string? cursor, [FromQuery] int? limit, [FromQuery] string? consistencyToken)
    {
        var r = await UsuarioQueries.FindUsuarios(new UsuarioFiltroDTO()
        {
            ConsistencyToken = consistencyToken,
            CursorToken = cursor,
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
    [HttpGet("logado"), ScopeAuthorize("identity.usuarios.logado")]
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
    [HttpGet("dados_basicos"), ScopeAuthorize("identity.usuarios.dados_basicos"), SuperUsuarioOrDominioAuthorize(PermissoesDominio.ListarUsuarios)]
    public async Task<ActionResult<List<BasicUserInfoDTO>>> DadosBasicos([FromQuery(Name = "id")] string[] ids, [FromQuery] string? consistencyToken)
    {
        var r = await UsuarioQueries.GetBasicUsersInfo(ids, consistencyToken);
        return Ok(r);
    }

    /// <summary>
    /// Retorna dados detalhados dos usuários com os ids informados.
    /// </summary>
    /// <param name="ids">Ids dos usuários.</param>
    /// <param name="consistencyToken">Token de consistência. Opcional.</param>
    /// <returns>Dados detalhados.</returns>
    [HttpGet("detalhes"), ScopeAuthorize("identity.usuarios.detalhes"), SuperUsuarioOrDominioAuthorize(PermissoesDominio.DetalharUsuarios)]
    public async Task<ActionResult<List<UsuarioDetalhesDTO>>> Detalhes([FromQuery(Name = "id")] string[] ids, [FromQuery] string? consistencyToken)
    {
        var r = await UsuarioQueries.GetUsuarioDetalhes(ids, consistencyToken);
        return Ok(r);
    }

    /// <summary>
    /// Bloqueia o usuário globalmente. Apenas o superusuário pode realizar essa ação.
    /// </summary>
    /// <param name="usuarioId">Id do usuário a ser bloqueado.</param>
    /// <returns>Ok.</returns>
    [HttpPost("{usuarioId}/bloquear"), ScopeAuthorize("identity.usuarios.bloquear"), SuperUsuarioAuthorize]
    public async Task<ActionResult<CommandResult>> Bloquear(string usuarioId)
    {
        var r = await Mediator.Send(new BloquearOuDesbloquearUsuarioCmd(User.Id(), usuarioId, true));
        return Ok(r);
    }

    /// <summary>
    /// Desbloqueia o usuário globalmente. Apenas o superusuário pode realizar essa ação.
    /// </summary>
    /// <param name="usuarioId">Id do usuário a ser desbloqueado.</param>
    /// <returns></returns>
    [HttpPost("{usuarioId}/desbloquear"), ScopeAuthorize("identity.usuarios.desbloquear"), SuperUsuarioAuthorize]
    public async Task<ActionResult<CommandResult>> Desbloquear(string usuarioId)
    {
        var r = await Mediator.Send(new BloquearOuDesbloquearUsuarioCmd(User.Id(), usuarioId, false));
        return Ok(r);
    }

    /// <summary>
    /// Edita os dados (primeiro nome, etc..) do usuário logado.
    /// </summary>
    /// <param name="cmd">Dados.</param>
    /// <returns>Ok.</returns>
    [HttpPost("meus_dados"), ScopeAuthorize("identity.usuarios.editar_meus_dados")]
    public async Task<ActionResult<CommandResult>> EditarMeusDados([FromBody] EditarMeusDadosCmd cmd)
    {
        cmd.UsuarioId = User.Id();
        var r = await Mediator.Send(cmd);
        return Ok(r);
    }

    /// <summary>
    /// Muda a senha do usuário logado. É necessário informar a senha atual.
    /// </summary>
    /// <param name="cmd">Dados.</param>
    /// <returns>Ok.</returns>
    [HttpPost("minha_senha"), ScopeAuthorize("identity.usuarios.mudar_minha_senha")]
    public async Task<ActionResult<CommandResult>> MudarMinhaSenha([FromBody] MudarMinhaSenhaCmd cmd)
    {
        cmd.UsuarioId = User.Id();
        var r = await Mediator.Send(cmd);
        return Ok(r);
    }

    /// <summary>
    /// Muda o avatar do usuário. O avatar é uma imagem representando o usuário.
    /// </summary>
    /// <param name="viewModel">Dados.</param>
    /// <returns>Ok.</returns>
    [HttpPost("avatar"), ScopeAuthorize("identity.usuarios.mudar_meu_avatar"), RequestFormLimits(MultipartBodyLengthLimit = 4_194_304)]
    public async Task<ActionResult<CommandResult>> MudarMeuAvatar([FromForm] MudarMeuAvatarViewModel viewModel)
    {
        if (viewModel.Validate() is UnsupportedMediaTypeResult um)
            return um;

        return Ok(await Mediator.Send(new MudarMeuAvatarCmd(User.Id(), viewModel.Imagem!.OpenReadStream(), viewModel.Imagem.FileName)));
    }
}