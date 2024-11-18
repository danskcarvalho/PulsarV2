using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

public class PushNotificationData
{
	public PushNotificationData(PushNotificationKey key, PushNotificationTarget target, DateTime createdOn)
	{
		Target = target;
		CreatedOn = createdOn;
		Key = key;
	}

	public PushNotificationTarget Target { get; private set; }
	public string? Title { get; set; }
	public string? Message { get; set; }
	public DateTime CreatedOn { get; private set; }
	public string? Data { get; set; }
	public PushNotificationKey Key { get; private set; }
	public PushNotificationDataAction? PrimaryAction { get; set; }
	public List<PushNotificationDataAction> Actions { get; private set; } = new List<PushNotificationDataAction>();
	public PushNotificationIntent? Intent { get; set; }
	public PushNotificationDisplay Display { get; set; }
}
