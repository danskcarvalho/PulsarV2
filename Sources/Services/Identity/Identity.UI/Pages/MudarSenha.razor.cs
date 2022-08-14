using Pulsar.Services.Identity.UI.Models;

namespace Pulsar.Services.Identity.UI.Pages;

public partial class MudarSenha
{
    MudarSenhaStage _stage;
    string? _errorMessage = null;

    void ContinueFromInformarSenha()
    {
        _stage = MudarSenhaStage.SenhaAlterada;
    }
}
