using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Shared.Attributes;

namespace Pulsar.Services.Identity.Contracts.Commands.Usuarios;

public class BloquearOuDesbloquearUsuarioCmd : IRequest<CommandResult>
{
    [SwaggerExclude]
    public string UsuarioLogadoId { get; set; }
    public string UsuarioId { get; set; }
    public bool Bloquear { get; set; }

    public BloquearOuDesbloquearUsuarioCmd(string usuarioLogadoId, string usuarioId, bool bloquear)
    {
        UsuarioLogadoId = usuarioLogadoId;
        UsuarioId = usuarioId;
        Bloquear = bloquear;
    }
}
