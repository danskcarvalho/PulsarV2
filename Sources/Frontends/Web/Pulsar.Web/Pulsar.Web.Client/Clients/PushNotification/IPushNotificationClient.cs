using Pulsar.Services.PushNotification.Contracts.Commands.PushNotifications;
using Pulsar.Services.PushNotification.Contracts.DTOs;
using Pulsar.Services.Shared.Commands;
using Pulsar.Web.Client.Models.PushNotification;

namespace Pulsar.Web.Client.Clients.PushNotification
{
	public interface IPushNotificationClient
	{
		Task<SessionModel> StartSession();
		Task<CommandResult> MarcarComoLida(MarcarNotificacoesComoLidaCmd cmd);
		Task<List<NoficacaoPushDTO>> Get();
	}
}
