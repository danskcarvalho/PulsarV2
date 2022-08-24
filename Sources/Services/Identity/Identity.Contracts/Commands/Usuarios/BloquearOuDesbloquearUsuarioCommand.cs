using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Shared.Attributes;

namespace Pulsar.Services.Identity.Contracts.Commands.Usuarios;

public class BloquearOuDesbloquearUsuarioCommand : IRequest<CommandResult>
{
    [SwaggerExclude]
    public string UsuarioLogadoId { get; set; }
    public string UsuarioId { get; set; }
    public bool Bloquear { get; set; }

    public BloquearOuDesbloquearUsuarioCommand(string usuarioLogadoId, string usuarioId, bool bloquear)
    {
        UsuarioLogadoId = usuarioLogadoId;
        UsuarioId = usuarioId;
        Bloquear = bloquear;
    }
}
