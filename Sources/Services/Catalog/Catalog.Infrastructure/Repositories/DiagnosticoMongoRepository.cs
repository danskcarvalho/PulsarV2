namespace Pulsar.Services.Catalog.Infrastructure.Repositories;

public class DiagnosticoMongoRepository : MongoRepository<IDiagnosticoRepository, Diagnostico>, IDiagnosticoRepository
{
    public DiagnosticoMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.DIAGNOSTICOS;

    protected override IDiagnosticoRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new DiagnosticoMongoRepository(session, sessionFactory);
    }
}
