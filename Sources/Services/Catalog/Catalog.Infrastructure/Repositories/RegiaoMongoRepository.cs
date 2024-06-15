namespace Pulsar.Services.Catalog.Infrastructure.Repositories;

public class RegiaoMongoRepository : MongoRepository<IRegiaoRepository, Regiao>, IRegiaoRepository
{
    public RegiaoMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.REGIOES;

    protected override IRegiaoRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new RegiaoMongoRepository(session, sessionFactory);
    }
}
