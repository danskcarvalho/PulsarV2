namespace Pulsar.Services.Identity.Infrastructure.Repositories
{
    public class UsuarioMongoRepository : MongoRepository<IUsuarioRepository, Usuario>, IUsuarioRepository
    {
        public UsuarioMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
        {
        }

        protected override string CollectionName => Constants.CollectionNames.USUARIOS;

        protected override IUsuarioRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
        {
            return new UsuarioMongoRepository(session, sessionFactory);
        }
    }
}
