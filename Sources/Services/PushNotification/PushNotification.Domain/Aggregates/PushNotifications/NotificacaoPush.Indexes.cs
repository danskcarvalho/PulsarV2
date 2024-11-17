using Pulsar.BuildingBlocks.DDD;

namespace Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications
{
	public partial class NotificacaoPush
	{
		public class Indexes : IndexDescriptions<NotificacaoPush>
		{
			public static IX UserContextId_v1 = Describe.Ascending(x => x.UserContextId).Descending(x => x.CreatedOn);
			public static IX UserContextId_Display_v1 = Describe.Ascending(x => x.UserContextId).Ascending(x => x.Display).Descending(x => x.CreatedOn);
			public override string CollectionName => Constants.CollectionNames.NOTIFICACOES_PUSH;
		}
	}
}
