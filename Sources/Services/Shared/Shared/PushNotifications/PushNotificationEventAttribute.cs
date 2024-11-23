namespace Pulsar.Services.Shared.PushNotifications;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PushNotificationEventAttribute : Attribute
{
	public PushNotificationEventAttribute(PushNotificationKey key)
	{
		Key = key;
	}

	public PushNotificationKey Key { get; private init; }
}
