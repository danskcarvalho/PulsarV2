using Pulsar.BuildingBlocks.DDD;

namespace Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;

public partial class Session
{
	public class Indexes : IndexDescriptions<Session>
	{
		public static IX Token_v1 = Describe.Ascending(x => x.Token);
		public override string CollectionName => Constants.CollectionNames.SESSIONS;
	}
}
