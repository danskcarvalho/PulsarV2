namespace Pulsar.Services.Catalog.Infrastructure.Repositories;

public class MaterialMongoRepository : MongoRepository<IMaterialRepository, Material>, IMaterialRepository
{
    public MaterialMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.MATERIAIS;

    protected override IMaterialRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new MaterialMongoRepository(session, sessionFactory);
    }
}
