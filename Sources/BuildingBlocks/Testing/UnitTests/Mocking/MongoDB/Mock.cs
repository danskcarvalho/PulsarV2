namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

public static class Mock
{
    public static IMockedDatabase Database() => new Database();
    public static IMockedDbSessionFactory SessionFactory(IMockedDatabase db, Func<IMediator> mediator) => new SessionFactory(db, mediator);
}
