namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

class SessionFactory : IMockedDbSessionFactory, IDbSessionFactory
{
    private readonly IMockedDatabase _database;
    private readonly Func<IMediator> _mediator;

    public SessionFactory(IMockedDatabase database, Func<IMediator> mediator)
    {
        _database = database;
        _mediator = mediator;
    }

    public IMockedDatabase Database => _database;

    public IDbSession CreateSession(IMediator? mediator = null)
    {
        return new Session(_database, mediator ?? _mediator());
    }

    IMockedDbSession IMockedDbSessionFactory.CreateSession(IMediator? mediator)
    {
        return new Session(_database, mediator ?? _mediator());
    }
}
