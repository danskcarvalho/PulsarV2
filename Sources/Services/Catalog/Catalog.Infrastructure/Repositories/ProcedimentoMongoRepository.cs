namespace Pulsar.Services.Catalog.Infrastructure.Repositories;

public class ProcedimentoMongoRepository : MongoRepository<IProcedimentoRepository, Procedimento>, IProcedimentoRepository
{
    public ProcedimentoMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.PROCEDIMENTOS;

    protected override IProcedimentoRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new ProcedimentoMongoRepository(session, sessionFactory);
    }
}
