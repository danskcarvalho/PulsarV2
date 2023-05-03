using Pulsar.Services.Identity.Domain.Aggregates.Others;

namespace Pulsar.Services.Identity.Infrastructure.Repositories
{
    public class MockedEstabelecimentoRepository : Repository<IEstabelecimentoRepository, Estabelecimento>, IEstabelecimentoRepository
    {
        public MockedEstabelecimentoRepository(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory) : base(session, sessionFactory)
        {
        }

        protected override string CollectionName => Constants.CollectionNames.ESTABELECIMENTOS;

        protected override IEstabelecimentoRepository Clone(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory)
        {
            return new MockedEstabelecimentoRepository(session, sessionFactory);
        }
    }
}
