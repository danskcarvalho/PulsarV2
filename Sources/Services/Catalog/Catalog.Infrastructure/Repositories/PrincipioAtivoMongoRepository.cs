namespace Pulsar.Services.Catalog.Infrastructure.Repositories;

public class PrincipioAtivoMongoRepository : MongoRepository<IPrincipioAtivoRepository, PrincipioAtivo>, IPrincipioAtivoRepository
{
    public PrincipioAtivoMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.PRINCIPIOSATIVOS;

    protected override IPrincipioAtivoRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new PrincipioAtivoMongoRepository(session, sessionFactory);
    }
}
