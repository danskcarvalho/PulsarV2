using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Shared.PushNotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications
{
	public partial class NotificacaoPush : AggregateRoot
	{
		[BsonConstructor]
		public NotificacaoPush(ObjectId id, ObjectId usuarioId, ObjectId userContextId, PushNotificationKey key, DateTime createdOn) : base(id)
		{
			CreatedOn = createdOn;
			Key = key;
			UserContextId = userContextId;
			UsuarioId = usuarioId;
		}

		public ObjectId UserContextId { get; private set; }
		public ObjectId UsuarioId { get; private set; }
		public string? Title { get; set; }
		public string? Message { get; set; }
		public DateTime CreatedOn { get; private set; }
		public string? Data { get; set; }
		public PushNotificationKey Key { get; private set; }
		public PushNotificationAction? PrimaryAction { get; set; }
		public List<PushNotificationAction> Actions { get; private set; } = new List<PushNotificationAction>();
		public PushNotificationIntent? Intent { get; set; }
		public PushNotificationDisplay Display { get; set; }
		public bool IsRead { get; set; }

		public void MarcarComoLida()
		{
			IsRead = true;
		}
	}
}
