using Microsoft.JSInterop;
using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.UI.Clients.Interfaces;
using Pulsar.Services.Identity.UI.Models;
using System.Net.Http.Json;

namespace Pulsar.Services.Identity.UI.Clients
{
    public class LoginClient : ILoginClient
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public LoginClient(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<UsuarioLogadoDTO?> TestCredentials(UsuarioSenhaDTO usuarioSenha, CancellationToken ct = default)
        {
            using var r = await _httpClient.PostAsJsonAsync("v2/login/test", usuarioSenha, cancellationToken: ct, options: SerializationOptions.Default);
            if (r.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            if (r.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exception = (await r.Content.ReadFromJsonAsync<ExceptionDTO>(cancellationToken: ct, options: SerializationOptions.Default))!;
                await _jsRuntime.InvokeVoidAsync("console.error", exception);
                throw new BackendException(exception);
            }
            return (await r.Content.ReadFromJsonAsync<UsuarioLogadoDTO>(cancellationToken: ct, options: SerializationOptions.Default))!;
        }

        public async Task<LoginResultDTO?> Login(LoginDTO login, CancellationToken ct = default)
        {
            using var r = await _httpClient.PostAsJsonAsync("v2/login", login, cancellationToken: ct, options: SerializationOptions.Default);
            if (r.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            if (r.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exception = (await r.Content.ReadFromJsonAsync<ExceptionDTO>(cancellationToken: ct, options: SerializationOptions.Default))!;
                await _jsRuntime.InvokeVoidAsync("console.error", exception);
                throw new BackendException(exception);
            }
            return await r.Content.ReadFromJsonAsync<LoginResultDTO>(cancellationToken: ct, options: SerializationOptions.Default);
        }
    }
}
