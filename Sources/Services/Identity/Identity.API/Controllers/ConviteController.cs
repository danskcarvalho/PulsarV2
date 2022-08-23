using Microsoft.AspNetCore.Authorization;
using Pulsar.Services.Identity.API.Utils;
using Pulsar.Services.Identity.Contracts.Commands.Convites;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v1/convites")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ConviteController : IdentityController
{
    public ConviteController(IdentityControllerContext context) : base(context)
    {
    }

    /// <summary>
    /// Convida um usuário para fazer parte do Pulsar.
    /// </summary>
    /// <param name="cmd">Dados.</param>
    /// <returns>Ok.</returns>

    [HttpPut, ScopeAuthorize("convites.criar"), SuperUsuarioOrDominioAuthorize(PermissoesDominio.ConvidarUsuario)]
    public async Task<ActionResult> Criar([FromBody]CriarConviteCommand cmd)
    {
        cmd.UsuarioLogadoId = User.Id();
        await Mediator.Send(cmd);
        return Ok();
    }
}
