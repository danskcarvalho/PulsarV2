using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.UI.Clients;
using Pulsar.Services.Identity.UI.Models;

namespace Pulsar.Services.Identity.UI.Pages;

public partial class EsqueciMinhaSenha
{
    EsqueciMinhaSenhaStage _stage = EsqueciMinhaSenhaStage.DigitarEmail;
    string? _errorMessage = null;
    EsqueciMinhaSenhaCmd _model = new EsqueciMinhaSenhaCmd();
    bool _loading = false;

    [Parameter, SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    async Task ContinueFromDigitarEmail()
    {
        _errorMessage = null;
        _loading = true;
        try
        {
            await EsqueciMinhaSenhaClient.EsqueciMinhaSenha(_model);
            _stage = EsqueciMinhaSenhaStage.EmailEnviado;
        }
        catch(BackendException e)
        {
            _errorMessage = e.Message;
            _stage = EsqueciMinhaSenhaStage.DigitarEmail;
        }
        finally
        {
            _loading = false;
        }
    }

    void Cancelar()
    {
        NavManager.NavigateTo(QueryHelpers.AddQueryString("/account/login", "ReturnUrl", ReturnUrl));
    }
}
