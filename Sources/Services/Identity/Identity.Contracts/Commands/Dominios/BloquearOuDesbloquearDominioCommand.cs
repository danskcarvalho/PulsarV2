using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.Contracts.Commands.Dominios;

public class BloquearOuDesbloquearDominioCommand : IRequest<CommandResult>
{
    public string? UsuarioLogadoId { get; set; }
    public string? DominioId { get; set; }
    public bool Bloquear { get; set; }

    public BloquearOuDesbloquearDominioCommand(string? usuarioLogadoId, string? dominioId, bool bloquear)
    {
        UsuarioLogadoId = usuarioLogadoId;
        DominioId = dominioId;
        Bloquear = bloquear;
    }
}
