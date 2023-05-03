using Pulsar.Services.Identity.Domain.Aggregates.Others;

namespace Identity.UnitTests.Repositories;

public class MockedRedeEstabelecimentosRepository : Repository<IRedeEstabelecimentosRepository, RedeEstabelecimentos>, IRedeEstabelecimentosRepository
{
    public MockedRedeEstabelecimentosRepository(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.REDES_ESTABELECIMENTOS;

    protected override IRedeEstabelecimentosRepository Clone(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory)
    {
        return new MockedRedeEstabelecimentosRepository(session, sessionFactory);
    }
}

