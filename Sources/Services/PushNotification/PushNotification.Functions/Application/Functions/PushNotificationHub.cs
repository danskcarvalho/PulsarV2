using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.SignalRService;
using Microsoft.DurableTask.Client;
using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Sync.Functions.Implementations;
using Pulsar.Services.PushNotification.Contracts.Commands.PushNotifications;
using Pulsar.Services.PushNotification.Contracts.Commands.UserContexts;
using Pulsar.Services.PushNotification.Functions.Application.Interfaces;
using System.Net;

namespace Pulsar.Services.PushNotification.Functions.Application.Functions;

[SignalRConnection("PulsarSignalRConnectionString")]
public class PushNotificationHub : ServerlessHub<IPushNotificationClient>
{
	readonly IMediator Mediator;
	public PushNotificationHub(
		IMediator mediator,
		IServiceProvider serviceProvider) : base(serviceProvider)
	{
		this.Mediator = mediator;
	}

	[Function("negotiate")]
	public async Task<HttpResponseData> Negotiate(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
	{
		string? auth;
		HttpResponseData? failedAuth;
		if ((failedAuth = GetAuthorizationToken(req, out auth)) != null)
		{
			return failedAuth;
		}
		var result = await Mediator.Send(new GetUserContextByTokenCmd(auth!));
		if (result.UserContextId == null)
		{
			failedAuth = req.CreateResponse(HttpStatusCode.BadRequest);
			return failedAuth;
		}

		var negotiateResponse = await NegotiateAsync(new() { UserId = result.UserContextId.ToString() });
		var response = req.CreateResponse();
		await response.WriteBytesAsync(negotiateResponse.ToArray());
		return response;
	}

	private static HttpResponseData? GetAuthorizationToken(HttpRequestData req, out string? auth)
	{
		req.Headers.TryGetValues("Authorization", out var values);
		auth = values?.FirstOrDefault()?.Trim();
		if (auth == null)
		{
			var noAuth = req.CreateResponse(HttpStatusCode.BadRequest);
			return noAuth;
		}
		if (auth.StartsWith("Bearer", StringComparison.InvariantCultureIgnoreCase))
		{
			auth = auth.Substring("Bearer".Length);
			auth = auth.Trim();
		}
		return null;
	}

	[Function("notify")]
	public async Task Notify(
		[ServiceBusTrigger("%ServiceBusDeveloper%.PushNotification", "notify.PushNotification", Connection = "ServiceBus")] PushNotificationIE evt)
	{
		var result = await Mediator.Send(new CriarNotificacaoPushCmd(evt.PushNotificationData));
		await PublishNotifications(result);
	}

	private async Task PublishNotifications(CriarNotificacaoPushResult result)
	{
		if (result.ToPublish != null && result.Targets != null && result.Targets.Count > 0)
		{
			List<Task> published = new List<Task>();
			foreach (var target in result.Targets)
			{
				var pn = result.ToPublish.Clone(target.PushNotificationId);
				published.Add(Clients.Users(target.UserContextId).Publish(pn));
			}

			await Task.WhenAll(published);
		}
	}
}
