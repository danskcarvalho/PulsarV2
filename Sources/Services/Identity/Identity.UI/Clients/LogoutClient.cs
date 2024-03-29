﻿using Microsoft.JSInterop;
using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.UI.Clients.Interfaces;
using Pulsar.Services.Identity.UI.Models;
using System.Net.Http.Json;

namespace Pulsar.Services.Identity.UI.Clients
{
    public class LogoutClient : ILogoutClient
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
            using var r = await _httpClient.PostAsJsonAsync("v2/logout/try", new LogoutDTO { LogoutId = logoutId }, cancellationToken: ct, options: SerializationOptions.Default);
            if (r.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exception = (await r.Content.ReadFromJsonAsync<ExceptionDTO>(cancellationToken: ct, options: SerializationOptions.Default))!;
                await _jsRuntime.InvokeVoidAsync("console.error", exception);
                throw new BackendException(exception);
            }
            return (await r.Content.ReadFromJsonAsync<LogoutResultDTO>(cancellationToken: ct, options: SerializationOptions.Default))!;
        }

        public async Task<LogoutResultDTO> Logout(string? logoutId, CancellationToken ct = default)
        {
            using var r = await _httpClient.PostAsJsonAsync("v2/logout", new LogoutDTO { LogoutId = logoutId }, cancellationToken: ct, options: SerializationOptions.Default);
            if (r.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exception = (await r.Content.ReadFromJsonAsync<ExceptionDTO>(cancellationToken: ct, options: SerializationOptions.Default))!;
                await _jsRuntime.InvokeVoidAsync("console.error", exception);
                throw new BackendException(exception);
            }
            return (await r.Content.ReadFromJsonAsync<LogoutResultDTO>(cancellationToken: ct, options: SerializationOptions.Default))!;
        }
    }
}
