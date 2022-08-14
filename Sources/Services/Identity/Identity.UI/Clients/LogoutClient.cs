using Microsoft.JSInterop;
using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.UI.Models;
using System.Net.Http.Json;

namespace Pulsar.Services.Identity.UI.Clients
{
    public class LogoutClient
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public LogoutClient(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<LogoutResultDTO> TryLogout(string? logoutId, CancellationToken ct = default)
        {
            var data = new Dictionary<string, string>();
            if (logoutId is not null)
                data.Add("logoutId", logoutId);
            var r = await _httpClient.PostAsync("v1/logout/try", new FormUrlEncodedContent(data), ct);
            if (r.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exception = (await r.Content.ReadFromJsonAsync<ExceptionDTO>(cancellationToken: ct))!;
                await _jsRuntime.InvokeVoidAsync("console.error", exception);
                throw new BackendException(exception);
            }
            return (await r.Content.ReadFromJsonAsync<LogoutResultDTO>(cancellationToken: ct))!;
        }

        public async Task<LogoutResultDTO> Logout(string? logoutId, CancellationToken ct = default)
        {
            var data = new Dictionary<string, string>();
            if (logoutId is not null)
                data.Add("logoutId", logoutId);
            var r = await _httpClient.PostAsync("v1/logout", new FormUrlEncodedContent(data), ct);
            if (r.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exception = (await r.Content.ReadFromJsonAsync<ExceptionDTO>(cancellationToken: ct))!;
                await _jsRuntime.InvokeVoidAsync("console.error", exception);
                throw new BackendException(exception);
            }
            return (await r.Content.ReadFromJsonAsync<LogoutResultDTO>(cancellationToken: ct))!;
        }
    }
}
