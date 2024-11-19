using MediatR;
using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

namespace Pulsar.Web.Client.Services.PushNotifications;

public class PushNotificationActionEvent : INotification
{
	public PushNotificationActionEvent(PushNotificationDataWithId pushNotificationData, PushNotificationDataAction action)
	{
		PushNotificationData = pushNotificationData;
		Action = action;
	}

	public PushNotificationDataWithId PushNotificationData { get; private init; }
	public PushNotificationDataAction Action { get; private init; }
}
