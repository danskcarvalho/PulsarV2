using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.BuildingBlocks.FileSystem.Abstractions;

namespace Pulsar.Services.Identity.API.Application.Commands.Usuarios;

[NoTransaction]
public class MudarMeuAvatarCH : IdentityCommandHandler<MudarMeuAvatarCmd, CommandResult>
{
    public const int MAX_IMAGE_WIDTH = 256;
    private readonly IFileSystem _fileSystem;
    private readonly IImageManipulationProvider _imageManipulationProvider;
    public MudarMeuAvatarCH(IFileSystem fileSystem, IImageManipulationProvider imageManipulationProvider, IdentityCommandHandlerContext<MudarMeuAvatarCmd, CommandResult> ctx) : base(ctx)
    {
        _fileSystem = fileSystem;
        _imageManipulationProvider = imageManipulationProvider;
    }

    protected override async Task<CommandResult> HandleAsync(MudarMeuAvatarCmd cmd, CancellationToken ct)
    {
        var img = _imageManipulationProvider.Resize(cmd.Stream, MAX_IMAGE_WIDTH);
        using var ms = new MemoryStream(img.FileContents);
        var url = await _fileSystem.UploadFileAsync(new UploadFileInput(Guid.NewGuid().ToString() + ".jpg", ms)
        {
            IsPublic = true,
            ContentType = "image/jpeg"
        }, ct);

        if (ct.IsCancellationRequested)
            return new CommandResult();

        return await Session.TrackConsistencyToken(async _ =>
        {
            return await Session.OpenTransactionAsync(async _ =>
            {
                var usuario = await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioLogadoId.ToObjectId());
                if (usuario == null)
                    throw new IdentityDomainException(IdentityExceptionKey.UsuarioNaoEncontrado);
                usuario.AlterarAvatar(url.Url);
                await UsuarioRepository.ReplaceOneAsync(usuario);
                return new CommandResult(Session.ConsistencyToken);
            });
        });
    }
}
