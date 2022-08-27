using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Identity.Domain.Aggregates.Grupos;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Controllers
{
    [ApiController]
    [Route("v1/grupos")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class GrupoController : IdentityController
    {
        public GrupoController(IdentityControllerContext context) : base(context)
        {
        }

        /// <summary>
        /// Lista todos os grupos para o domínio logado.
        /// </summary>
        /// <param name="filtro">Filtro de texto. Opcional.</param>
        /// <param name="cursor">Cursor. Opcional.</param>
        /// <param name="limit">Limite de grupos retornados. Opcional.</param>
        /// <param name="consistencyToken">Token de consistência. Opcional.</param>
        /// <returns>Lista de grupos.</returns>
        [HttpGet, ScopeAuthorize("grupos.listar"), DominioAuthorize]
        public async Task<ActionResult<PaginatedListDTO<GrupoListadoDTO>>> Listar([FromQuery] string? filtro, [FromQuery] string? cursor, [FromQuery] int? limit, [FromQuery] string? consistencyToken)
        {
            var r = await GrupoQueries.FindGrupos(User.DominioId()!, filtro, cursor, limit, consistencyToken);
            return Ok(r);
        }

        /// <summary>
        /// Retorna detalhes dos grupos informados.
        /// </summary>
        /// <param name="ids">Ids dos grupos.</param>
        /// <param name="consistencyToken">Token de consistência. Opcional.</param>
        /// <returns>Detalhes dos grupos.</returns>
        [HttpGet("detalhes"), ScopeAuthorize("grupos.detalhes"), DominioAuthorize]
        public async Task<ActionResult<List<GrupoDetalhesDTO>>> Detalhes([FromQuery(Name = "id")] string[] ids, [FromQuery] string? consistencyToken)
        {
            var r = await GrupoQueries.GetGrupoDetalhes(User.DominioId()!, ids, consistencyToken);
            return Ok(r);
        }

        /// <summary>
        /// Retorna todos os usuários adicionados a um subgrupo e grupo.
        /// </summary>
        /// <param name="grupoId">Id do grupo.</param>
        /// <param name="subgrupoId">Id do subgrupo.</param>
        /// <param name="filtro">Filtro de texto. Opcional.</param>
        /// <param name="cursor">Cursor. Opcional.</param>
        /// <param name="limit">Limite de usuários retornados. Opcional.</param>
        /// <param name="consistencyToken">Token de consistência. Opcional.</param>
        /// <returns>Lista de usuários.</returns>
        [HttpGet("{grupoId}/subgrupos/{subgrupoId}/usuarios"), ScopeAuthorize("grupos.usuarios"), DominioAuthorize]
        public async Task<ActionResult<List<GrupoDetalhesDTO>>> Usuarios(string grupoId, string subgrupoId, [FromQuery] string? filtro, [FromQuery] string? cursor, [FromQuery] int? limit, [FromQuery] string? consistencyToken)
        {
            var r = await GrupoQueries.FindUsuarios(User.DominioId()!, grupoId, subgrupoId, filtro, cursor, limit, consistencyToken);
            return Ok(r);
        }

        /// <summary>
        /// Cria um novo grupo dentro do domínio logado.
        /// </summary>
        /// <param name="cmd">Dados.</param>
        /// <returns>Id do grupo criado.</returns>
        [HttpPut, ScopeAuthorize("grupos.criar"), DominioAuthorize(PermissoesDominio.EditarGrupos)]
        public async Task<ActionResult<CreatedCommandResult>> Criar([FromBody] CriarGrupoCommand cmd)
        {
            cmd.UsuarioLogadoId = User.Id();
            cmd.DominioId = User.DominioId();
            var r = await Mediator.Send(cmd);
            return Ok(r);
        }
        /// <summary>
        /// Cria um subgrupo dentro de um grupo.
        /// </summary>
        /// <param name="grupoId">Id do grupo.</param>
        /// <param name="cmd">Dados.</param>
        /// <returns>Id do subgrupo criado.</returns>

        [HttpPut("{grupoId}/subgrupos"), ScopeAuthorize("grupos.criar_subgrupo"), DominioAuthorize(PermissoesDominio.EditarGrupos)]
        public async Task<ActionResult<CreatedCommandResult>> CriarSubgrupo(string grupoId, [FromBody] CriarSubGrupoCommand cmd)
        {
            cmd.UsuarioLogadoId = User.Id();
            cmd.DominioId = User.DominioId();
            cmd.GrupoId = grupoId ?? throw new ArgumentNullException(nameof(grupoId));
            var r = await Mediator.Send(cmd);
            return Ok(r);
        }

        /// <summary>
        /// Edita um grupo.
        /// </summary>
        /// <param name="grupoId">Id do grupo.</param>
        /// <param name="cmd">Dados.</param>
        /// <returns>Ok.</returns>
        [HttpPost("{grupoId}"), ScopeAuthorize("grupos.editar"), DominioAuthorize(PermissoesDominio.EditarGrupos)]
        public async Task<ActionResult<CommandResult>> Editar(string grupoId, [FromBody] EditarGrupoCommand cmd)
        {
            cmd.UsuarioLogadoId = User.Id();
            cmd.DominioId = User.DominioId();
            cmd.GrupoId = grupoId ?? throw new ArgumentNullException(nameof(grupoId));
            var r = await Mediator.Send(cmd);
            return Ok(r);
        }

        /// <summary>
        /// Edita um subgrupo dentro de um grupo.
        /// </summary>
        /// <param name="grupoId">Id do grupo.</param>
        /// <param name="subgrupoId">Id do subgrupo.</param>
        /// <param name="cmd">Dados.</param>
        /// <returns>Ok.</returns>
        [HttpPost("{grupoId}/subgrupos/{subgrupoId}"), ScopeAuthorize("grupos.editar_subgrupo"), DominioAuthorize(PermissoesDominio.EditarGrupos)]
        public async Task<ActionResult<CommandResult>> EditarSubgrupo(string grupoId, string subgrupoId, [FromBody] EditarSubGrupoCommand cmd)
        {
            cmd.UsuarioLogadoId = User.Id();
            cmd.DominioId = User.DominioId();
            cmd.GrupoId = grupoId ?? throw new ArgumentNullException(nameof(grupoId));
            cmd.SubGrupoId = subgrupoId ?? throw new ArgumentNullException(nameof(subgrupoId));
            var r = await Mediator.Send(cmd);
            return Ok(r);
        }

        /// <summary>
        /// Remove um grupo.
        /// </summary>
        /// <param name="grupoId">Id do grupo.</param>
        /// <returns>Ok.</returns>
        [HttpDelete("{grupoId}"), ScopeAuthorize("grupos.remover"), DominioAuthorize(PermissoesDominio.EditarGrupos)]
        public async Task<ActionResult<CommandResult>> Remover(string grupoId)
        {
            var cmd = new RemoverGrupoCommand();
            cmd.UsuarioLogadoId = User.Id();
            cmd.DominioId = User.DominioId();
            cmd.GrupoId = grupoId ?? throw new ArgumentNullException(nameof(grupoId));
            var r = await Mediator.Send(cmd);
            return Ok(r);
        }

        /// <summary>
        /// Remove um subgrupo.
        /// </summary>
        /// <param name="grupoId">Id do grupo.</param>
        /// <param name="subgrupoId">Id do subgrupo.</param>
        /// <returns>Ok.</returns>
        [HttpDelete("{grupoId}/subgrupos/{subgrupoId}"), ScopeAuthorize("grupos.remover_subgrupo"), DominioAuthorize(PermissoesDominio.EditarGrupos)]
        public async Task<ActionResult<CommandResult>> RemoverSubgrupo(string grupoId, string subgrupoId)
        {
            var cmd = new RemoverSubGrupoCommand();
            cmd.UsuarioLogadoId = User.Id();
            cmd.DominioId = User.DominioId();
            cmd.GrupoId = grupoId ?? throw new ArgumentNullException(nameof(grupoId));
            cmd.SubGrupoId = subgrupoId ?? throw new ArgumentNullException(nameof(subgrupoId));
            var r = await Mediator.Send(cmd);
            return Ok(r);
        }

        /// <summary>
        /// Adiciona usuários em um subgrupo e grupo.
        /// </summary>
        /// <param name="grupoId">Id do grupo.</param>
        /// <param name="subgrupoId">Id do subgrupo.</param>
        /// <param name="cmd">Dados.</param>
        /// <returns>Ok.</returns>
        [HttpPut("{grupoId}/subgrupos/{subgrupoId}/usuarios"), ScopeAuthorize("grupos.adicionar_usuarios"), DominioAuthorize(PermissoesDominio.EditarGrupos)]
        public async Task<ActionResult<CreatedCommandResult>> AdicionarUsuarios(string grupoId, string subgrupoId, [FromBody] AdicionarUsuariosSubGrupoCommand cmd)
        {
            cmd.UsuarioLogadoId = User.Id();
            cmd.DominioId = User.DominioId();
            cmd.GrupoId = grupoId ?? throw new ArgumentNullException(nameof(grupoId));
            cmd.SubGrupoId = subgrupoId ?? throw new ArgumentNullException(nameof(subgrupoId));
            var r = await Mediator.Send(cmd);
            return Ok(r);
        }

        /// <summary>
        /// Remove usuários de um subgrupo e grupo.
        /// </summary>
        /// <param name="grupoId">Id do grupo.</param>
        /// <param name="subgrupoId">Id do subgrupo.</param>
        /// <param name="cmd">Dados.</param>
        /// <returns>Ok.</returns>
        [HttpDelete("{grupoId}/subgrupos/{subgrupoId}/usuarios"), ScopeAuthorize("grupos.remover_usuarios"), DominioAuthorize(PermissoesDominio.EditarGrupos)]
        public async Task<ActionResult<CreatedCommandResult>> RemoverUsuarios(string grupoId, string subgrupoId, [FromBody] RemoverUsuariosSubGrupoCommand cmd)
        {
            cmd.UsuarioLogadoId = User.Id();
            cmd.DominioId = User.DominioId();
            cmd.GrupoId = grupoId ?? throw new ArgumentNullException(nameof(grupoId));
            cmd.SubGrupoId = subgrupoId ?? throw new ArgumentNullException(nameof(subgrupoId));
            var r = await Mediator.Send(cmd);
            return Ok(r);
        }
    }
}
