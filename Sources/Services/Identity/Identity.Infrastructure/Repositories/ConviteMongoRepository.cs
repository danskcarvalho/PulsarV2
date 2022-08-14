namespace Pulsar.Services.Identity.Infrastructure.Repositories;

public class ConviteMongoRepository : MongoRepository<IConviteRepository, Convite>, IConviteRepository
{
    public ConviteMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.Convites;

    protected override IConviteRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new ConviteMongoRepository(session, sessionFactory);
    }
}
