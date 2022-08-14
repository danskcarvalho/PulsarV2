using Microsoft.AspNetCore.Components;
using Pulsar.Services.Identity.UI.Models;

namespace Pulsar.Services.Identity.UI.Pages;

public partial class EsqueciMinhaSenha
{
    [Parameter, SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    EsqueciMinhaSenhaStage _stage = EsqueciMinhaSenhaStage.DigitarEmail;
    string? _errorMessage = null;

    void ContinueFromDigitarEmail()
    {
        _stage = EsqueciMinhaSenhaStage.EmailEnviado;
    }

    void Cancelar()
    {
        NavManager.NavigateTo("/account/login");
    }
}
