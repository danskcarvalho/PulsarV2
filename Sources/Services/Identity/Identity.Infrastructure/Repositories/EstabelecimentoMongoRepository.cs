namespace Pulsar.Services.Identity.Infrastructure.Repositories
{
    public class EstabelecimentoMongoRepository : MongoRepository<IEstabelecimentoRepository, Estabelecimento>, IEstabelecimentoRepository
    {
        public EstabelecimentoMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
        {
        }

        protected override string CollectionName => Constants.CollectionNames.Estabelecimentos;

        protected override IEstabelecimentoRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
        {
            return new EstabelecimentoMongoRepository(session, sessionFactory);
        }
    }
}
