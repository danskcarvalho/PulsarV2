using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

public class PushNotificationDataAction
{
	public PushNotificationDataAction(PushNotificationRouteKey routeKey, string linkText)
	{
		LinkText = linkText;
		RouteKey = routeKey;
	}

	/// <summary>
	/// The link should be in {} if not a button. Ex.: "Click {here} to stop migrating.", "{Yes}", "{No}"
	/// </summary>
	public string LinkText { get; private set; }
	public PushNotificationRouteKey RouteKey { get; private set; }
	public List<PushNotificationDataActionParam> Parameters { get; private set; } = new List<PushNotificationDataActionParam>();
	public PushNotificationIntent? Intent { get; set; }
	public PushNotificationActionPlacement? Placement { get; set; }
	public PushNotificationButtonStyle? ButtonStyle { get; set; }

}
