using Pulsar.Services.Identity.Domain.Aggregates.Grupos;
using Pulsar.Services.Identity.Infrastructure;
using Pulsar.Services.Identity.Infrastructure.Repositories;

namespace Identity.UnitTests.Repositories
{
    public class MockedGrupoRepository : Repository<IGrupoRepository, Grupo>, IGrupoRepository
    {
        public MockedGrupoRepository(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory) : base(session, sessionFactory)
        {
        }

        protected override string CollectionName => Constants.CollectionNames.GRUPOS;

        protected override IGrupoRepository Clone(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory)
        {
            return new MockedGrupoRepository(session, sessionFactory);
        }
    }
}
