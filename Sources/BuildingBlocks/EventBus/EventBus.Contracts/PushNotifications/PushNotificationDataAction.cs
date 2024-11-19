using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

public class PushNotificationDataAction
{
	public PushNotificationDataAction(PushNotificationRouteKey routeKey, string text)
	{
		Text = text;
		RouteKey = routeKey;
	}

	/// <summary>
	/// The link should be in {} if not a button. Ex.: "Click {here} to stop migrating.", "{Yes}", "{No}"
	/// </summary>
	public string Text { get; private set; }
	public PushNotificationRouteKey RouteKey { get; private set; }
	public List<PushNotificationDataActionParam> Parameters { get; private set; } = new List<PushNotificationDataActionParam>();

	internal PushNotificationDataAction Clone()
	{
		var cloned = (PushNotificationDataAction)this.MemberwiseClone();
		cloned.Parameters = this.Parameters.Select(p => new PushNotificationDataActionParam(p.ParamKey, p.ParamValue)).ToList();
		return cloned;
	}
}
