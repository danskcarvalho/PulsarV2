using Blazored.SessionStorage;

namespace Pulsar.Web.Client.Services
{
    public class ConsistencyTokenManager
    {
        private const string CONSISTENCY_TOKEN = "ConsistencyTokenManager.ConsistencyToken";

        private readonly ISessionStorageService _sessionStorage;
        public ConsistencyTokenManager(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage;
            LoadToken();
        }

        public string? ConsistencyToken { get; private set; }

        public async void OnTokenObserved(string token)
        {
            this.ConsistencyToken = token;
            await _sessionStorage.SetItemAsync(CONSISTENCY_TOKEN, this.ConsistencyToken);
        }

        private async void LoadToken()
        {

            this.ConsistencyToken = await _sessionStorage.GetItemAsync<string>(CONSISTENCY_TOKEN);
        }
    }
}
