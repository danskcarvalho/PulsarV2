using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.UI.Models;
using System.Net.Http;

namespace Pulsar.Services.Identity.UI.Clients.Interfaces;

public interface ILogoutClient
{
    Task<LogoutResultDTO> TryLogout(string? logoutId, CancellationToken ct = default);

    Task<LogoutResultDTO> Logout(string? logoutId, CancellationToken ct = default);
}
