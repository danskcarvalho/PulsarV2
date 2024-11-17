using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications
{
	public class PushNotificationActionParam : ValueObject
	{
		[BsonConstructor]
		public PushNotificationActionParam(PushNotificationRouteKey paramKey, string paramValue)
		{
			ParamKey = paramKey;
			ParamValue = paramValue;
		}

		public PushNotificationRouteKey ParamKey { get; private set; }
		public string ParamValue { get; private set; }

		protected override IEnumerable<object?> GetEqualityComponents()
		{
			yield return ParamKey;
			yield return ParamValue;
		}
	}
}
