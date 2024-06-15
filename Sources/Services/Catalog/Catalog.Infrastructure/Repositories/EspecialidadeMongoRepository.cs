namespace Pulsar.Services.Catalog.Infrastructure.Repositories;

public class EspecialidadeMongoRepository : MongoRepository<IEspecialidadeRepository, Especialidade>, IEspecialidadeRepository
{
    public EspecialidadeMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.ESPECIALIDADES;

    protected override IEspecialidadeRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new EspecialidadeMongoRepository(session, sessionFactory);
    }
}
