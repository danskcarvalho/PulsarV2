using Microsoft.JSInterop;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.UI.Models;
using System.Net.Http.Json;

namespace Pulsar.Services.Identity.UI.Clients;

public class EsqueciMinhaSenhaClient
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    public EsqueciMinhaSenhaClient(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task EsqueciMinhaSenha(EsqueciMinhaSenhaCommand cmd, CancellationToken ct = default)
    {
        using var r = await _httpClient.PostAsJsonAsync("v2/esqueci_minha_senha", cmd, cancellationToken: ct, options: SerializationOptions.Default);
        if (r.StatusCode != System.Net.HttpStatusCode.OK)
        {
            var exception = (await r.Content.ReadFromJsonAsync<ExceptionDTO>(cancellationToken: ct, options: SerializationOptions.Default))!;
            await _jsRuntime.InvokeVoidAsync("console.error", exception);
            throw new BackendException(exception);
        }
    }

    public async Task RecuperarSenha(RecuperarSenhaCommand cmd, CancellationToken ct = default)
    {
        using var r = await _httpClient.PostAsJsonAsync("v2/esqueci_minha_senha/recuperar", cmd, cancellationToken: ct, options: SerializationOptions.Default);
        if (r.StatusCode != System.Net.HttpStatusCode.OK)
        {
            var exception = (await r.Content.ReadFromJsonAsync<ExceptionDTO>(cancellationToken: ct, options: SerializationOptions.Default))!;
            await _jsRuntime.InvokeVoidAsync("console.error", exception);
            throw new BackendException(exception);
        }
    }
}
