using Pulsar.BuildingBlocks.EventBus.Events;

namespace Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

[EventName(EVENT_NAME)]
public record PushNotificationIE : IntegrationEvent
{
	public const string EVENT_NAME = "PushNotifications:DispatchNotification";

	[JsonConstructor]
	public PushNotificationIE(PushNotificationData pushNotificationData) : base()
	{
		this.PushNotificationData = pushNotificationData;
	}

	public PushNotificationData PushNotificationData { get; private init; }
}
