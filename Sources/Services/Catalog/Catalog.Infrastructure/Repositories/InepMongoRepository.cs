namespace Pulsar.Services.Catalog.Infrastructure.Repositories;

public class InepMongoRepository : MongoRepository<IInepRepository, Inep>, IInepRepository
{
    public InepMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.INEPS;

    protected override IInepRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new InepMongoRepository(session, sessionFactory);
    }
}
