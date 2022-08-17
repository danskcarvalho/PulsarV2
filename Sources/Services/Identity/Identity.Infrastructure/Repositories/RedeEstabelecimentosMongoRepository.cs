namespace Pulsar.Services.Identity.Infrastructure.Repositories
{
    public class RedeEstabelecimentosMongoRepository : MongoRepository<IRedeEstabelecimentosRepository, RedeEstabelecimentos>, IRedeEstabelecimentosRepository
    {
        public RedeEstabelecimentosMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
        {
        }

        protected override string CollectionName => Constants.CollectionNames.RedesEstabelecimentos;

        protected override IRedeEstabelecimentosRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
        {
            return new RedeEstabelecimentosMongoRepository(session, sessionFactory);
        }
    }
}
