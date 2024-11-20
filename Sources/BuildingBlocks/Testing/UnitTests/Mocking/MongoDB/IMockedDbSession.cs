namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

public interface IMockedDbSession : IDbSession
{
    IMockedDatabase Database { get; }
    Task DispatchDomainEvents(IAggregateRoot root);
}