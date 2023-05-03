using Pulsar.Services.Identity.Domain.Aggregates.Dominios;

namespace Pulsar.Services.Identity.Infrastructure.Repositories;

public class MockedDominioRepository : Repository<IDominioRepository, Dominio>, IDominioRepository
{
    public MockedDominioRepository(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.DOMINIOS;

    protected override IDominioRepository Clone(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory)
    {
        return new MockedDominioRepository(session, sessionFactory);
    }
}
