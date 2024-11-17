using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.Services.PushNotification.Contracts.DTOs;

public class PushNotificationActionDTO
{
	/// <summary>
	/// The link should be in {} if not a button. Ex.: "Click {here} to stop migrating.", "{Yes}", "{No}"
	/// </summary>
	public required string LinkText { get; set; }
	public required PushNotificationRouteKey RouteKey { get; set; }
	public required List<PushNotificationActionParamDTO> Parameters { get; set; }
	public PushNotificationIntent? Intent { get; set; }
	public PushNotificationActionPlacement? Placement { get; set; }
	public PushNotificationButtonStyle? ButtonStyle { get; set; }

}
