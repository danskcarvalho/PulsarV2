using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

namespace Pulsar.Services.PushNotification.Functions.Application.Interfaces;

public interface IPushNotificationClient
{
	Task Publish(PushNotificationDataWithId pushNotification);
}
