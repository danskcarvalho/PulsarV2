namespace Identity.UnitTests.Repositories
{
    public class MockedUsuarioRepository : Repository<IUsuarioRepository, Usuario>, IUsuarioRepository
    {
        public MockedUsuarioRepository(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory) : base(session, sessionFactory)
        {
        }

        protected override string CollectionName => Constants.CollectionNames.USUARIOS;

        protected override IUsuarioRepository Clone(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory)
        {
            return new MockedUsuarioRepository(session, sessionFactory);
        }
    }
}
