namespace Pulsar.BuildingBlocks.FileSystem.Abstractions;

public interface IFileSystem
{
    Task<UploadFileOutput> UploadFileAsync(UploadFileInput args, CancellationToken ct = default);

    Task<string> SignGetUrl(string url, TimeSpan? duration = null, CancellationToken ct = default);
}
