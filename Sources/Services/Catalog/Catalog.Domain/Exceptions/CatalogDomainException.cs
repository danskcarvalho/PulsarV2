using Pulsar.BuildingBlocks.DDD;

namespace Pulsar.Services.Identity.Domain.Exceptions;

public class CatalogDomainException : DomainException
{
    public ExceptionKey Key { get; }


    public CatalogDomainException(ExceptionKey key) : this(key, GetMessageFromKey(key))
    {
    }

    public CatalogDomainException(ExceptionKey key, string message)
        : base(key.ToString(), message)
    {
        Key = key;
    }

    public CatalogDomainException(ExceptionKey key, string message, Exception innerException)
        : base(key.ToString(), message, innerException)
    {
        Key = key;
    }
}
