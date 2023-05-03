using Amazon.Runtime.Internal.Util;

namespace Pulsar.BuildingBlocks.UnitTests.Mocking.FileSystem;

internal class MockedFileSystem : IFileSystem, IDisposable
{
    private readonly List<(string Url, TimeSpan? Duration)> _signedUrls = new List<(string Url, TimeSpan? Duration)>();
    private readonly ReadOnlyCollection<(string Url, TimeSpan? Duration)> _roSignedUrls;
    private readonly List<UploadFileInput> _uploads = new List<UploadFileInput>();
    private readonly ReadOnlyCollection<UploadFileInput> _roUploads;
    public MockedFileSystem()
    {
        _roSignedUrls = _signedUrls.AsReadOnly();
        _roUploads = _uploads.AsReadOnly();
    }
    public IReadOnlyList<(string Url, TimeSpan? Duration)> SignedUrls => _signedUrls;
    public IReadOnlyList<UploadFileInput> EmailsSent => _roUploads;

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public Task<string> SignGetUrl(string url, TimeSpan? duration = null, CancellationToken ct = default)
    {
        _signedUrls.Add((url, duration));
        return Task.FromResult(url);
    }

    public Task<UploadFileOutput> UploadFileAsync(UploadFileInput args, CancellationToken ct = default)
    {
        var cloned = new UploadFileInput(args.FileName, args.Content)
        {
            ContentLength = args.ContentLength,
            ContentType = args.ContentType,
            IsPublic = args.IsPublic
        };
        _uploads.Add(cloned);
        return Task.FromResult(new UploadFileOutput($"https://mockedserver.com/{Guid.NewGuid().ToString()}{Path.GetExtension(args.FileName)}", $"{Guid.NewGuid().ToString()}{Path.GetExtension(args.FileName)}"));
    }
}
