using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.Contracts.Commands.Usuarios;

public class MudarMeuAvatarCommand : IRequest<CommandResult>
{
    public string UsuarioLogadoId { get; set; }
    public Stream Stream { get; set; }
    public string FileName { get; set; }

    public MudarMeuAvatarCommand(string usuarioLogadoId, Stream stream, string fileName)
    {
        UsuarioLogadoId = usuarioLogadoId;
        Stream = stream;
        FileName = fileName;
    }
}
