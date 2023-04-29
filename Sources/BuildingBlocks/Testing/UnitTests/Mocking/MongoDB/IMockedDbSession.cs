namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

public interface IMockedDbSession : IDbSession
{
    IMockedDatabase Database { get; }
    void TrackAggregateRoot(IAggregateRoot root);
}