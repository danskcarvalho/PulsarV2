using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

namespace Pulsar.Services.PushNotification.Contracts.Commands.PushNotifications;

public class CriarNotificacaoPushCmd : IRequest<CriarNotificacaoPushResult>
{
	public CriarNotificacaoPushCmd(PushNotificationData pushNotificationData)
	{
		PushNotificationData = pushNotificationData;
	}

	public PushNotificationData PushNotificationData { get; private init; }
}

public class CriarNotificacaoPushResult
{
	public CriarNotificacaoPushResult(PushNotificationData? toPublish, List<(string PushNotificationId, string UserContextId)>? targets)
	{
		ToPublish = toPublish;
		Targets = targets;
	}

	public PushNotificationData? ToPublish { get; private init; }
	public List<(string PushNotificationId, string UserContextId)>? Targets { get; private set; }
}
