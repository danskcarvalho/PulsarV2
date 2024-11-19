using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

public class PushNotificationData
{
	[JsonConstructor]
	public PushNotificationData(PushNotificationKey key, PushNotificationTarget? target, DateTime createdOn)
	{
		Target = target;
		CreatedOn = createdOn;
		Key = key;
	}

	public PushNotificationTarget? Target { get; private set; }
	public string? Title { get; set; }
	public string? Message { get; set; }
	public DateTime CreatedOn { get; private set; }
	public string? Data { get; set; }
	public PushNotificationKey Key { get; private set; }
	public PushNotificationDataAction? PrimaryAction { get; set; }
	public PushNotificationDataAction? SecondaryAction { get; set; }
	public PushNotificationDataAction? LabelAction { get; set; }
	public PushNotificationIntent? Intent { get; set; }
	public PushNotificationDisplay Display { get; set; }
	public PushNotificationToastDisplayOptions? ToastDisplayOptions { get; set; }
	public PushNotificationToastActionOptions? ToastActionOptions { get; set; }

	public PushNotificationData Clone()
	{
		var cloned = (PushNotificationData)this.MemberwiseClone();
		cloned.Target = this.Target?.Clone();
		cloned.PrimaryAction = this.PrimaryAction?.Clone();
		cloned.SecondaryAction = this.SecondaryAction?.Clone();
		cloned.LabelAction = this.LabelAction?.Clone();
		return cloned;
	}

	public PushNotificationDataWithId Clone(string id)
	{
		var cloned = (PushNotificationDataWithId)new PushNotificationDataWithId(id, this.Key, null, this.CreatedOn)
		{
			Title = this.Title,
			Message = this.Message,
			CreatedOn = this.CreatedOn,
			Data = this.Data,
			PrimaryAction = this.PrimaryAction?.Clone(),
			SecondaryAction = this.SecondaryAction?.Clone(),
			LabelAction = this.LabelAction?.Clone(),
			Intent = this.Intent,
			Display = this.Display,
			ToastDisplayOptions = this.ToastDisplayOptions,
			ToastActionOptions = this.ToastActionOptions,
		};
		return cloned;
	}
}

public class PushNotificationDataWithId : PushNotificationData
{
	[JsonConstructor]
	public PushNotificationDataWithId(string pushNotificationId, PushNotificationKey key, PushNotificationTarget? target, DateTime createdOn) : base(key, target, createdOn)
	{
		this.PushNotificationId = pushNotificationId;
	}

	public string? PushNotificationId { get; private set; }
}
