using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.Services.PushNotification.Contracts.DTOs;

public class PushNotificationActionParamDTO
{
	public required PushNotificationRouteKey ParamKey { get; set; }
	public required string ParamValue { get; set; }
}
