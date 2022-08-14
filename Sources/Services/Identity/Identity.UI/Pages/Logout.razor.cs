using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pulsar.Services.Identity.UI.Models;

namespace Pulsar.Services.Identity.UI.Pages;

public partial class Logout
{
    LogoutStage _stage = LogoutStage.Deslogando;
    string? _errorMessage = null; 
    ElementReference? _logoutIFrame;

    [Parameter, SupplyParameterFromQuery]
    public string? LogoutId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var r = await LogoutClient.TryLogout(LogoutId);
            if (r.ShowSignoutPrompt)
            {
                _stage = LogoutStage.Confirmacao;
            }
            else
            {
                if (r.SignOutIFrameUrl is not null)
                {
                    await JS.InvokeVoidAsync("AppendLogoutIFrame", _logoutIFrame, r.SignOutIFrameUrl);
                }

                if (r.PostLogoutRedirectUri is not null)
                {
                    NavManager.NavigateTo(r.PostLogoutRedirectUri, forceLoad: true);
                }
                else
                {
                    _stage = LogoutStage.Deslogado;
                }
            }
        }
        catch(BackendException e)
        {
            _stage = LogoutStage.Confirmacao;
            _errorMessage = e.Message;
        }
    }

    async Task Confirm()
    {
        try
        {
            _stage = LogoutStage.Deslogando;
            var r = await LogoutClient.Logout(LogoutId);

            if (r.SignOutIFrameUrl is not null)
            {
                await JS.InvokeVoidAsync("AppendLogoutIFrame", _logoutIFrame, r.SignOutIFrameUrl);
            }

            if (r.PostLogoutRedirectUri is not null)
            {
                NavManager.NavigateTo(r.PostLogoutRedirectUri, forceLoad: true);
            }
            else
            {
                _stage = LogoutStage.Deslogado;
            }
        }
        catch (BackendException e)
        {
            _stage = LogoutStage.Confirmacao;
            _errorMessage = e.Message;
        }
    }
}
