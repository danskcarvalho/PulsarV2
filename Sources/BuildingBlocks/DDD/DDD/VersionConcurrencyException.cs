namespace Pulsar.BuildingBlocks.DDD;

public class VersionConcurrencyException : Exception
{
    public VersionConcurrencyException() { }
    public VersionConcurrencyException(string message) : base(message) { }
    public VersionConcurrencyException(string message, Exception inner) : base(message, inner) { }
}
