namespace Pulsar.Services.Identity.Domain.Exceptions;

public class IdentityDomainException : DomainException
{
    public ExceptionKey Key { get; }

    public IdentityDomainException(ExceptionKey key) : this(key, GetMessageFromKey(key))
    {
    }

    public IdentityDomainException(ExceptionKey key, string message)
        : base(key.ToString(), message)
    {
        Key = key;
    }

    public IdentityDomainException(ExceptionKey key, string message, Exception innerException)
        : base(key.ToString(), message, innerException)
    {
        Key = key;
    }
}
