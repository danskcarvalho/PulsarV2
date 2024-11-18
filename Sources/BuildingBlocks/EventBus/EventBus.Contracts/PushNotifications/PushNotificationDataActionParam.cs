using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

public class PushNotificationDataActionParam
{
	public PushNotificationDataActionParam(PushNotificationRouteKey paramKey, string paramValue)
	{
		ParamKey = paramKey;
		ParamValue = paramValue;
	}

	public PushNotificationRouteKey ParamKey { get; private set; }
	public string ParamValue { get; private set; }
}