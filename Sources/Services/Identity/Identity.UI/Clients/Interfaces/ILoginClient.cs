using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.UI.Models;
using System.Net.Http;

namespace Pulsar.Services.Identity.UI.Clients.Interfaces;

public interface ILoginClient
{
    Task<UsuarioLogadoDTO?> TestCredentials(UsuarioSenhaDTO usuarioSenha, CancellationToken ct = default);

    Task<LoginResultDTO?> Login(LoginDTO login, CancellationToken ct = default);
}
