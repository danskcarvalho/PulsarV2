using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications
{
	public class PushNotificationAction : AggregateComponent
	{
		[BsonConstructor]
		public PushNotificationAction(PushNotificationRouteKey routeKey, string linkText)
		{
			LinkText = linkText;
			RouteKey = routeKey;
		}

		/// <summary>
		/// The link should be in {} if not a button. Ex.: "Click {here} to stop migrating.", "{Yes}", "{No}"
		/// </summary>
		public string LinkText { get; private set; }
		public PushNotificationRouteKey RouteKey { get; private set; }
		public List<PushNotificationActionParam> Parameters { get; private set; } = new List<PushNotificationActionParam>();
		public PushNotificationIntent? Intent { get; set; }
		public PushNotificationActionPlacement? Placement { get; set; }
		public PushNotificationButtonStyle? ButtonStyle { get; set; }

	}
}
