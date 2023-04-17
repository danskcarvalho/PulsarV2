using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Polly;
using System.IO;

namespace Pulsar.BuildingBlocks.FileSystemAzure;

public class AzureFileSystem : IFileSystem
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string? _publicContainer;
    private readonly string? _privateContainer;
    private readonly string _serviceUri;
    UserDelegationKey? _userDelegationKey;
    private ILogger<AzureFileSystem> _logger;

    public AzureFileSystem(ILogger<AzureFileSystem> logger, string serviceUri, string? publicContainer, string? privateContainer)
    {
        _logger = logger;
        _userDelegationKey = null;
        _publicContainer = publicContainer;
        _privateContainer = privateContainer;
        _serviceUri = serviceUri;
        while(_serviceUri.EndsWith("/"))
            _serviceUri = _serviceUri.Substring(0, _serviceUri.Length - 1);
        _blobServiceClient = new BlobServiceClient(new Uri(_serviceUri), new DefaultAzureCredential());
    }
    public async Task<string> SignGetUrl(string url, TimeSpan? duration = null, CancellationToken ct = default)
    {
        var retryPolicy = Policy
                   .Handle<Exception>(e => true)
                   .WaitAndRetryAsync(3,
                       retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 0.5 + Math.Pow(2, retryAttempt) * Random.Shared.NextDouble()),
                       (e, ts) =>
                       {
                           _logger.LogError(e, $"error while trying to sign blob {url}");
                           _logger.LogInformation($"will retry in {(int)ts.TotalSeconds} seconds");
                       });

        try
        {
            return await retryPolicy.ExecuteAsync(async (CancellationToken ct2) =>
            {
                GetBlobAndContainer(url, out string container, out string blob);
                duration ??= TimeSpan.FromMinutes(5);

                if (UserDelegationKeyHasExpired())
                    _userDelegationKey = (await _blobServiceClient.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(2), ct2)).Value;

                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = container,
                    BlobName = blob,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.Add(duration.Value),
                    Protocol = SasProtocol.Https
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                BlobUriBuilder blobUriBuilder = new BlobUriBuilder(new Uri(url))
                {
                    Sas = sasBuilder.ToSasQueryParameters(_userDelegationKey, _blobServiceClient.AccountName)
                };

                return blobUriBuilder.ToUri().ToString();
            }, ct);
        }
        catch(Exception e)
        {
            _logger.LogError(e, $"error signing blob {url}");
            throw;
        }
    }

    private void GetBlobAndContainer(string url, out string container, out string blob)
    {
        while (url.EndsWith("/"))
            url = url.Substring(0, url.Length - 1);

        var i = url.LastIndexOf('/');
        blob = url.Substring(i + 1);
        url = url.Substring(0, i); 
        i = url.LastIndexOf('/');
        container = url.Substring(i + 1);
    }

    private bool UserDelegationKeyHasExpired()
    {
        if (_userDelegationKey == null)
            return true;

        return DateTimeOffset.UtcNow >= _userDelegationKey.SignedExpiresOn.AddDays(-1);
    }

    public async Task<UploadFileOutput> UploadFileAsync(UploadFileInput args, CancellationToken ct = default)
    {
        var retryPolicy = Policy
                   .Handle<Exception>(e => true)
                   .WaitAndRetryAsync(3,
                       retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 0.5 + Math.Pow(2, retryAttempt) * Random.Shared.NextDouble()),
                       (e, ts) =>
                       {
                           _logger.LogError(e, $"error while trying to upload {args.FileName}");
                           _logger.LogInformation($"will retry in {(int)ts.TotalSeconds} seconds");
                           args.Content.Position = 0; //reset the position in case of an exception
                       });

        try
        {
            return await retryPolicy.ExecuteAsync(async (CancellationToken ct2) =>
            {
                if (args.IsPublic && _publicContainer == null)
                    throw new InvalidOperationException("no public container specified");
                if (!args.IsPublic && _privateContainer == null)
                    throw new InvalidOperationException("no private container specified");

                var container = args.IsPublic ? _blobServiceClient.GetBlobContainerClient(_publicContainer)
                    : _blobServiceClient.GetBlobContainerClient(_privateContainer);

                var filename = Guid.NewGuid().ToString() + Path.GetExtension(args.FileName);
                var blob = container.GetBlobClient(filename);

                var result = await blob.UploadAsync(args.Content, new BlobHttpHeaders { ContentType = args.ContentType });
                var url = $"{_serviceUri}/{container.Name}/{filename}";
                return new UploadFileOutput(url, filename);
            }, ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"error while uploading file {args.FileName}");
            throw;
        }
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
