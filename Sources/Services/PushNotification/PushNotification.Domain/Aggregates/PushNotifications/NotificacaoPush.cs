using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;
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

		public NotificacaoPush(ObjectId userContextId, ObjectId usuarioId, PushNotificationData data) : base(ObjectId.GenerateNewId())
		{
			this.UserContextId = userContextId;
			this.UsuarioId = usuarioId;
			this.Title = data.Title;
			this.Message = data.Message;
			this.CreatedOn = data.CreatedOn;
			this.Data = data.Data;
			this.Key = data.Key;
			this.ToastDisplayOptions = data.ToastDisplayOptions;
			this.ToastActionOptions = data.ToastActionOptions;
			if (data.PrimaryAction != null)
			{
				this.PrimaryAction = new PushNotificationAction(data.PrimaryAction.RouteKey, data.PrimaryAction.Text);
				foreach (var p in data.PrimaryAction.Parameters)
				{
					this.PrimaryAction.Parameters.Add(new PushNotificationActionParam(p.ParamKey, p.ParamValue));
				}
			}
			if (data.SecondaryAction != null)
			{
				this.SecondaryAction = new PushNotificationAction(data.SecondaryAction.RouteKey, data.SecondaryAction.Text);
				foreach (var p in data.SecondaryAction.Parameters)
				{
					this.SecondaryAction.Parameters.Add(new PushNotificationActionParam(p.ParamKey, p.ParamValue));
				}
			}
			if (data.LabelAction != null)
			{
				this.LabelAction = new PushNotificationAction(data.LabelAction.RouteKey, data.LabelAction.Text);
				foreach (var p in data.LabelAction.Parameters)
				{
					this.LabelAction.Parameters.Add(new PushNotificationActionParam(p.ParamKey, p.ParamValue));
				}
			}
			this.Intent = data.Intent;
			this.Display = data.Display;
		}

		public ObjectId UserContextId { get; private set; }
		public ObjectId UsuarioId { get; private set; }
		public string? Title { get; set; }
		public string? Message { get; set; }
		public DateTime CreatedOn { get; private set; }
		public string? Data { get; set; }
		public PushNotificationKey Key { get; private set; }
		public PushNotificationAction? PrimaryAction { get; set; }
		public PushNotificationAction? SecondaryAction { get; set; }
		public PushNotificationAction? LabelAction { get; set; }
		public PushNotificationIntent? Intent { get; set; }
		public PushNotificationDisplay Display { get; set; }
		public PushNotificationToastDisplayOptions? ToastDisplayOptions { get; set; }
		public PushNotificationToastActionOptions? ToastActionOptions { get; set; }
		public bool IsRead { get; set; }

		public void MarcarComoLida()
		{
			IsRead = true;
		}
	}
}
