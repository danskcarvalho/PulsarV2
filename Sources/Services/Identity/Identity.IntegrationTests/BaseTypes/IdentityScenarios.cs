using Identity.IntegrationTests.Utils;

namespace Identity.IntegrationTests.BaseTypes
{
    public class IdentityScenarios : IDisposable
    {
        private TestingWebApplicationFactory _webApplicationFactory = new TestingWebApplicationFactory();

        public IdentityScenarios()
        {
            _webApplicationFactory.RunMigrations().Wait();
        }

        public HttpClient GetClient(TestUser user)
        {
            return _webApplicationFactory.GetClient(user);
        }

        public void Dispose()
        {
            _webApplicationFactory.Dispose();
        }
    }
}
