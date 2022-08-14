using Pulsar.Services.Identity.UI.Models;

namespace Pulsar.Services.Identity.UI.Pages;

public partial class AceitarConvite
{
    AceitarConviteStage _stage;
    string? _errorMessage = null;

    void ContinueFromInformarDados()
    {
        _stage = AceitarConviteStage.ConviteAceito;
    }
}
