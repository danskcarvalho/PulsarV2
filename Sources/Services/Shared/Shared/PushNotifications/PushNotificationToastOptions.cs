namespace Pulsar.Services.Shared.PushNotifications;

public enum PushNotificationToastDisplayOptions
{
	UseTitle,
	UseMessage,
	UseBoth
}

public enum PushNotificationToastActionOptions
{
	NoAction,
	UseLabel,
	UsePrimaryAction,
	UseSecondaryAction,
	UsePrimaryAndSecondaryAction
}
