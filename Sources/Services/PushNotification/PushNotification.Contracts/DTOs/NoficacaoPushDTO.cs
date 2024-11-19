using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.Services.PushNotification.Contracts.DTOs;

public class NotificacaoPushDTO
{
	public required string Id { get; set; }
	public string? Title { get; set; }
	public string? Message { get; set; }
	public required DateTime CreatedOn { get; set; }
	public string? Data { get; set; }
	public required PushNotificationKey Key { get; set; }
	public PushNotificationActionDTO? PrimaryAction { get; set; }
	public PushNotificationActionDTO? SecondaryAction { get; set; }
	public PushNotificationActionDTO? LabelAction { get; set; }
	public PushNotificationIntent? Intent { get; set; }
	public PushNotificationToastDisplayOptions? ToastDisplayOptions { get; set; }
	public PushNotificationToastActionOptions? ToastActionOptions { get; set; }
	public required PushNotificationDisplay Display { get; set; }
	public required bool IsRead { get; set; }
}
