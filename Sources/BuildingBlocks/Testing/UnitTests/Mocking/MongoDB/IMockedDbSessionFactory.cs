namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

public interface IMockedDbSessionFactory : IDbSessionFactory
{
    public IMockedDatabase Database { get; }

    new IMockedDbSession CreateSession(IMediator? mediator = null);
}