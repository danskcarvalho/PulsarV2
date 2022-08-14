namespace Pulsar.BuildingBlocks.FileSystem.Abstractions;

public interface IFileSystem
{
    Task<UploadFileOutput> UploadFileAsync(UploadFileInput args, CancellationToken ct = default);

    string SignGetUrl(string url, TimeSpan? duration = null);

    PresignedUploadUrl SignPutUrl(string extension, string? contentType = null, TimeSpan? duration = null);
}
