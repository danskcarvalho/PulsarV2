using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.UI.Models;
using System.Net.Http;

namespace Pulsar.Services.Identity.UI.Clients.Interfaces;

public interface IEsqueciMinhaSenhaClient
{
    Task EsqueciMinhaSenha(EsqueciMinhaSenhaCmd cmd, CancellationToken ct = default);

    Task RecuperarSenha(RecuperarSenhaCmd cmd, CancellationToken ct = default);
}
