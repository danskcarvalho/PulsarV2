namespace Pulsar.Services.Identity.Domain.Exceptions;

public class IdentityDomainException : DomainException
{
    public IdentityExceptionKey Key { get; }

    public IdentityDomainException(IdentityExceptionKey key) : this(key, GetMessageFromKey(key))
    {
    }

    public IdentityDomainException(IdentityExceptionKey key, string message)
        : base(key.ToString(), message)
    {
        Key = key;
    }

    public IdentityDomainException(IdentityExceptionKey key, string message, Exception innerException)
        : base(key.ToString(), message, innerException)
    {
        Key = key;
    }
}
