using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Shared.Commands;
using Pulsar.Web.Client.Models.Shared;

namespace Pulsar.Web.Client.Clients.Identity.Usuarios;

public interface IUsuarioClient
{
    Task<BasicUserInfoDTO> Logado();
    Task<CommandResult> EditarMeusDados(EditarMeusDadosCmd cmd);
    Task<CommandResult> MudarMinhaSenha(MudarMinhaSenhaCmd cmd);
    Task<CommandResult> MudarMeuAvatar(BrowserFile imgFile);
}
