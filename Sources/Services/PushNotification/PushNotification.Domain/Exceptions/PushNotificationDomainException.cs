using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.PushNotification.Contracts.Enumerations;

public class PushNotificationDomainException : DomainException
{
    public PushNotificationExceptionKey Key { get; }

    public PushNotificationDomainException(PushNotificationExceptionKey key) : this(key, GetMessageFromKey(key))
    {
    }

    public PushNotificationDomainException(PushNotificationExceptionKey key, string message)
        : base(key.ToString(), message)
    {
        Key = key;
    }

    public PushNotificationDomainException(PushNotificationExceptionKey key, string message, Exception innerException)
        : base(key.ToString(), message, innerException)
    {
        Key = key;
    }
}
