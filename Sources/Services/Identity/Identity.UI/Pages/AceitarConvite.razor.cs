using Microsoft.AspNetCore.Components;
using Pulsar.Services.Identity.Contracts.Commands.Convites;
using Pulsar.Services.Identity.UI.Clients;
using Pulsar.Services.Identity.UI.Models;

namespace Pulsar.Services.Identity.UI.Pages;

public partial class AceitarConvite
{
    AceitarConviteCommand _model = new AceitarConviteCommand();
    AceitarConviteStage _stage = AceitarConviteStage.InformarDados;
    string? _errorMessage = null;
    bool _loading = false;

    [Parameter, SupplyParameterFromQuery]
    public string? ConviteId { get; set; }
    [Parameter, SupplyParameterFromQuery]
    public string? Token { get; set; }

    protected override void OnInitialized()
    {
        _model.ConviteId = ConviteId;
        _model.Token = Token;
    }


    async Task ContinueFromInformarDados()
    {
        _errorMessage = null;
        _loading = true;
        try
        {
            await AceitarConviteClient.Aceitar(_model);
            _stage = AceitarConviteStage.ConviteAceito;
        }
        catch (BackendException e)
        {
            _errorMessage = e.Message;
            _stage = AceitarConviteStage.InformarDados;
        }
        finally
        {
            _loading = false;
        }
    }
}
