using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;
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

		public async Task<List<PushNotificationDataWithId>> Get(bool excluirLidas)
		{
			var notifications = await Get<List<NotificacaoPushDTO>>(null, new
			{
				excluirLidas
			});
			return notifications.Select(n => new PushNotificationDataWithId(n.Id, n.Key, null, n.CreatedOn)
			{
				LabelAction = ConvertAction(n.LabelAction),
				PrimaryAction = ConvertAction(n.PrimaryAction),
				SecondaryAction = ConvertAction(n.SecondaryAction),
				ToastActionOptions = n.ToastActionOptions,
				Data = n.Data,
				Display = n.Display,
				Intent = n.Intent,
				Message = n.Message,
				Title = n.Title,
				ToastDisplayOptions = n.ToastDisplayOptions,
				IsRead = n.IsRead,
			}).ToList();
		}

		private PushNotificationDataAction? ConvertAction(PushNotificationActionDTO? action)
		{
			if (action == null)
			{
				return null;
			}
			var a = new PushNotificationDataAction(action.RouteKey, action.Text);
			a.Parameters.AddRange(action.Parameters.Select(p => new PushNotificationDataActionParam(p.ParamKey, p.ParamValue)));
			return a;
		}

		public Task<CommandResult> MarcarComoLida(MarcarNotificacoesComoLidaCmd cmd) => Post<CommandResult>("read", cmd);

		public Task<SessionModel> StartSession() => Post<SessionModel>("sessions");
	}
}
