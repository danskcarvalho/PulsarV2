using Microsoft.JSInterop;
using Pulsar.Web.Client.Services.Others;

namespace Pulsar.Web.Client.Clients.Base
{
	public class ClientContext(HttpClient httpClient, IJSRuntime jsRuntime, ConsistencyTokenManager consistencyTokenManager)
	{
		public HttpClient HttpClient { get; } = httpClient;
		public IJSRuntime JsRuntime { get; } = jsRuntime;
		public ConsistencyTokenManager ConsistencyTokenManager { get; } = consistencyTokenManager;
	}
}
