using Microsoft.JSInterop;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Web.Client.Clients.Identity.Base;
using Pulsar.Web.Client.Models.Shared;

namespace Pulsar.Web.Client.Clients.Identity.Usuarios
{
    public class UsuarioClient(ClientContext clientContext) : ClientBase(clientContext), IUsuarioClient
    {
        protected override string Section => "usuarios";

        protected override string Service => "identity";

        public Task<CommandResult> EditarMeusDados(EditarMeusDadosCmd cmd) => Post<CommandResult>("meus_dados", cmd);

        public Task<BasicUserInfoDTO> Logado() => Get<BasicUserInfoDTO>("logado");

        public async Task<CommandResult> MudarMeuAvatar(BrowserFile imgFile)
        {
            return await SendFiles<CommandResult>(HttpMethod.Post, "avatar", files: ("Imagem", imgFile));
        }

        public Task<CommandResult> MudarMinhaSenha(MudarMinhaSenhaCmd cmd) => Post<CommandResult>("minha_senha", cmd);
    }
}
