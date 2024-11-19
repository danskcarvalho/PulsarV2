using Pulsar.Services.PushNotification.Contracts.Commands.PushNotifications;
using Pulsar.Services.PushNotification.Contracts.DTOs;
using Pulsar.Services.Shared.Commands;
using Pulsar.Web.Client.Clients.Base;
using Pulsar.Web.Client.Models.PushNotification;
using System.Collections.Generic;

namespace Pulsar.Web.Client.Clients.PushNotification
{
	public class PushNotificationClient : ClientBase, IPushNotificationClient
	{
		public PushNotificationClient(ClientContext clientContext) : base(clientContext)
		{
		}

		protected override string Section => "pushnotifications";

		protected override string Service => "pushnotification";

		public Task<List<NoficacaoPushDTO>> Get() => Get<List<NoficacaoPushDTO>>(null);

		public Task<CommandResult> MarcarComoLida(MarcarNotificacoesComoLidaCmd cmd) => Post<CommandResult>("read", cmd);

		public Task<SessionModel> StartSession() => Post<SessionModel>("sessions");
	}
}
