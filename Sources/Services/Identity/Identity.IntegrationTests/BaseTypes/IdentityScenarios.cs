using Identity.IntegrationTests.Utils;
using System.Web;

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

        protected string BuildQueryString(object values)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            foreach (var prop in values.GetType().GetProperties())
            {
                var value = Convert.ToString(prop.GetValue(values));
                if (value is not null)
                    nameValueCollection[prop.Name] = value;
            }
            var queryString = nameValueCollection.ToString();
            if (!string.IsNullOrWhiteSpace(queryString))
                return $"?{queryString}";
            else
                return string.Empty;
        } 
    }
}
