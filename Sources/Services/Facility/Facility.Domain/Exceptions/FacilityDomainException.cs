using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Facility.Contracts.Enumerations;

namespace Pulsar.Services.Facility.Domain.Exceptions;

public class FacilityDomainException : DomainException
{
    public FacilityExceptionKey Key { get; }

    public FacilityDomainException(FacilityExceptionKey key) : this(key, GetMessageFromKey(key))
    {
    }

    public FacilityDomainException(FacilityExceptionKey key, string message)
        : base(key.ToString(), message)
    {
        Key = key;
    }

    public FacilityDomainException(FacilityExceptionKey key, string message, Exception innerException)
        : base(key.ToString(), message, innerException)
    {
        Key = key;
    }
}
