using Pulsar.Services.Identity.Domain.Aggregates.Convites;
using System.Diagnostics;

namespace Pulsar.Services.Identity.Infrastructure.Repositories;

public class MockedConviteRepository : Repository<IConviteRepository, Convite>, IConviteRepository
{
    public MockedConviteRepository(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.CONVITES;

    protected override IConviteRepository Clone(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory)
    {
        return new MockedConviteRepository(session, sessionFactory);
    }
}
