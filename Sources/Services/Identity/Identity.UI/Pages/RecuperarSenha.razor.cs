using Microsoft.AspNetCore.Components;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.UI.Clients;
using Pulsar.Services.Identity.UI.Models;

namespace Pulsar.Services.Identity.UI.Pages;

public partial class RecuperarSenha
{
    MudarSenhaStage _stage = MudarSenhaStage.InformarSenha;
    string? _errorMessage = null;
    RecuperarSenhaCmd _model = new RecuperarSenhaCmd();
    bool _loading = false;

    [Parameter, SupplyParameterFromQuery]
    public string? Token { get; set; }
    [Parameter, SupplyParameterFromQuery]
    public string? UserId { get; set; }

    protected override void OnInitialized()
    {
        _model.UsuarioId = UserId;
        _model.Token = Token;
    }

    async Task ContinueFromInformarSenha()
    {
        _errorMessage = null;
        _loading = true;
        try
        {
            await EsqueciMinhaSenhaClient.RecuperarSenha(_model);
            _stage = MudarSenhaStage.SenhaAlterada;
        }
        catch (BackendException e)
        {
            _errorMessage = e.Message;
            _stage = MudarSenhaStage.InformarSenha;
        }
        finally
        {
            _loading = false;
        }
    }
}
