using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications
{
	public class PushNotificationAction : AggregateComponent
	{
		[BsonConstructor]
		public PushNotificationAction(PushNotificationRouteKey routeKey, string text)
		{
			Text = text;
			RouteKey = routeKey;
		}

		/// <summary>
		/// The link should be in {} if not a button. Ex.: "Click {here} to stop migrating.", "{Yes}", "{No}"
		/// </summary>
		public string Text { get; private set; }
		public PushNotificationRouteKey RouteKey { get; private set; }
		public List<PushNotificationActionParam> Parameters { get; private set; } = new List<PushNotificationActionParam>();

	}
}
