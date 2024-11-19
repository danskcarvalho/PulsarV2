using Microsoft.AspNetCore.Mvc;
using Pulsar.Services.PushNotification.API.Models;
using Pulsar.Services.PushNotification.API.Utils;
using Pulsar.Services.PushNotification.Contracts.Commands.PushNotifications;
using Pulsar.Services.PushNotification.Contracts.Commands.Sessions;
using Pulsar.Services.PushNotification.Contracts.DTOs;
using Pulsar.Services.Shared.Commands;

namespace Pulsar.Services.PushNotification.API.Controllers;

[ApiController]
[Route("v2/pushnotifications")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class PushNotificationController : BaseController
{
    public PushNotificationController(BaseControllerContext context) : base(context)
    {
    }

    [HttpPost("sessions"), ScopeAuthorize("pushnotification.api.create_session")]
    public async Task<ActionResult<SessionModel>> StartSession()
	{
		var result = await Mediator.Send(new CriarSessaoCmd(User.Id())
		{
			EstabelecimentoId = User.EstabelecimentoId(),
			DominioId = User.DominioId()
		});
		var url = GetUrl();
		return Ok(new SessionModel(result.Token, url));
	}

	private string GetUrl()
	{
		var url = Configuration["Services:pushnotification-functions:http:0"]!;
		if (!url.EndsWith("/"))
			url += "/";

		return url + "api";
	}

	[HttpPost("read"), ScopeAuthorize("pushnotification.api.mark_as_read")]
	public async Task<ActionResult<CommandResult>> MarcarComoLida(MarcarNotificacoesComoLidaCmd cmd)
	{
		cmd.UsuarioId = User.Id();
		var result = await Mediator.Send(cmd);
		return Ok(result);
	}

    [HttpGet, ScopeAuthorize("pushnotification.api.read")]
    public async Task<ActionResult<List<NotificacaoPushDTO>>> Get([FromQuery] string? consistencyToken)
    {
        return Ok(
            await NotificacoesPushQueries.GetNoficacoes(User.Id(), User.DominioId(), User.EstabelecimentoId(), consistencyToken)
            );
    }
}
