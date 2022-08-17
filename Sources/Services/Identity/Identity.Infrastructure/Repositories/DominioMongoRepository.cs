namespace Pulsar.Services.Identity.Infrastructure.Repositories;

public class DominioMongoRepository : MongoRepository<IDominioRepository, Dominio>, IDominioRepository
{
    public DominioMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.Dominios;

    protected override IDominioRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new DominioMongoRepository(session, sessionFactory);
    }
}
