using Microsoft.JSInterop;
using Pulsar.Services.Identity.Contracts.Commands.Convites;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.UI.Models;
using System.Net.Http.Json;

namespace Pulsar.Services.Identity.UI.Clients;

public class AceitarConviteClient
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    public AceitarConviteClient(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task Aceitar(AceitarConviteCommand cmd, CancellationToken ct = default)
    {
        using var r = await _httpClient.PostAsJsonAsync("v1/aceitar_convite", cmd, ct);
        if (r.StatusCode != System.Net.HttpStatusCode.OK)
        {
            var exception = (await r.Content.ReadFromJsonAsync<ExceptionDTO>(cancellationToken: ct))!;
            await _jsRuntime.InvokeVoidAsync("console.error", exception);
            throw new BackendException(exception);
        }
    }
}
