using MongoDB.Bson.Serialization.Attributes;

namespace Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

public interface IPushNotificationEvent
{
	PushNotificationData? GetPushNotificationData();
}
