namespace Pulsar.Services.Catalog.Infrastructure.Repositories;

public class EtniaMongoRepository : MongoRepository<IEtniaRepository, Etnia>, IEtniaRepository
{
    public EtniaMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.ETNIAS;

    protected override IEtniaRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new EtniaMongoRepository(session, sessionFactory);
    }
}
