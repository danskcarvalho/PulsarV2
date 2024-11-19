using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.Services.PushNotification.Contracts.DTOs;

public class PushNotificationActionDTO
{
	public required string Text { get; set; }
	public required PushNotificationRouteKey RouteKey { get; set; }
	public required List<PushNotificationActionParamDTO> Parameters { get; set; }

}
