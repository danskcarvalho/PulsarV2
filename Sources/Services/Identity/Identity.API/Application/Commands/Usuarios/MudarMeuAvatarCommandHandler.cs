using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.BuildingBlocks.FileSystem.Abstractions;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Usuarios;

[NoTransaction]
public class MudarMeuAvatarCommandHandler : IdentityCommandHandler<MudarMeuAvatarCommand, CommandResult>
{
    public const int MaxImageWidth = 256;
    private readonly IFileSystem _fileSystem;
    private readonly IImageManipulationProvider _imageManipulationProvider;
    public MudarMeuAvatarCommandHandler(IFileSystem fileSystem, IImageManipulationProvider imageManipulationProvider, IdentityCommandHandlerContext<MudarMeuAvatarCommand, CommandResult> ctx) : base(ctx)
    {
        _fileSystem = fileSystem;
        _imageManipulationProvider = imageManipulationProvider;
    }

    protected override async Task<CommandResult> HandleAsync(MudarMeuAvatarCommand cmd, CancellationToken ct)
    {
        var img = _imageManipulationProvider.Resize(cmd.Stream, MaxImageWidth);
        using var ms = new MemoryStream(img.FileContents);
        var url = await _fileSystem.UploadFileAsync(new UploadFileInput("avatars/" + Guid.NewGuid().ToString() + ".jpg", ms), ct);

        return await Session.OpenTransactionAsync(async ct2 =>
        {
            var usuario = await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioLogadoId.ToObjectId(), ct2);
            if (usuario == null)
                throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
            usuario.AlterarAvatar(url.PubliclUrl, url.InternalUrl);
            await UsuarioRepository.ReplaceOneAsync(usuario, ct: ct2);
            return new CommandResult(Session.ConsistencyToken);
        }, ct: ct);
    }
}
