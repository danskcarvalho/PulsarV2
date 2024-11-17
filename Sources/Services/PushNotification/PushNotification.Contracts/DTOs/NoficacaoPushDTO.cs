using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.Services.PushNotification.Contracts.DTOs;

public class NoficacaoPushDTO
{
	public required string Id { get; set; }
	public string? Title { get; set; }
	public string? Message { get; set; }
	public required DateTime CreatedOn { get; set; }
	public string? Data { get; set; }
	public required PushNotificationKey Key { get; set; }
	public PushNotificationActionDTO? PrimaryAction { get; set; }
	public required List<PushNotificationActionDTO> Actions { get; set; }
	public PushNotificationIntent? Intent { get; set; }
	public required PushNotificationDisplay Display { get; set; }
	public required bool IsRead { get; set; }
}
