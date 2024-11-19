namespace Pulsar.Services.Shared.PushNotifications;

public class PushNotificationEventAttribute : Attribute
{
	public PushNotificationEventAttribute(PushNotificationKey key)
	{
		Key = key;
	}

	public PushNotificationKey Key { get; private init; }
}
