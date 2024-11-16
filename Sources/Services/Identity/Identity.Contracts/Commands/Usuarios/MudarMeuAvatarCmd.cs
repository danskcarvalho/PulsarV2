namespace Pulsar.Services.Identity.Contracts.Commands.Usuarios;

public class MudarMeuAvatarCmd : IRequest<CommandResult>
{
    public string UsuarioLogadoId { get; set; }
    public Stream Stream { get; set; }
    public string FileName { get; set; }

    public MudarMeuAvatarCmd(string usuarioLogadoId, Stream stream, string fileName)
    {
        UsuarioLogadoId = usuarioLogadoId;
        Stream = stream;
        FileName = fileName;
    }
}
