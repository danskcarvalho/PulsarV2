using Pulsar.BuildingBlocks.EventBus.Events;

namespace Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

[EventName(EVENT_NAME)]
public record PushNotificationEvent : IntegrationEvent
{
	public const string EVENT_NAME = "PushNotifications:DispatchNotification";

	public PushNotificationEvent(PushNotificationData data) : base()
	{
		this.PushNotificationData = data;
	}

	public PushNotificationData PushNotificationData { get; private init; }
}
