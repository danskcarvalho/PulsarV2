using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

public class PushNotificationDataActionParam
{
	public PushNotificationDataActionParam(PushNotificationRouteParamKey paramKey, string paramValue)
	{
		ParamKey = paramKey;
		ParamValue = paramValue;
	}

	public PushNotificationRouteParamKey ParamKey { get; private set; }
	public string ParamValue { get; private set; }
}