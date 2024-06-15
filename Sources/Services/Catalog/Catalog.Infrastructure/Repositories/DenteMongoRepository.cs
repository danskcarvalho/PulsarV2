namespace Pulsar.Services.Catalog.Infrastructure.Repositories;

public class DenteMongoRepository : MongoRepository<IDenteRepository, Dente>, IDenteRepository
{
    public DenteMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.DENTES;

    protected override IDenteRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new DenteMongoRepository(session, sessionFactory);
    }
}
