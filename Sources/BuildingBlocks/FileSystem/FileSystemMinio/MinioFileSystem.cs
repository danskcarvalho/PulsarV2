namespace Pulsar.BuildingBlocks.FileSystemMinio;

public class MinioFileSystem : IFileSystem
{
    private ILogger<MinioFileSystem> _logger;
    public MinioConfigurationSection Configuration { get; }

    public MinioFileSystem(ILogger<MinioFileSystem> logger, MinioConfigurationSection configuration)
    {
        _logger = logger;
        Configuration = configuration;
    }

    private MinioClient BuildMinioClient()
    {
        return new MinioClient()
                    .WithEndpoint(Configuration.Endpoint)
                    .WithCredentials(Configuration.AccessKey, Configuration.SecretKey)
                    .WithSSL()
                    .Build();
    }

    public async Task<UploadFileOutput> UploadFileAsync(UploadFileInput args, CancellationToken ct = default)
    {
        var retryPolicy = Policy
                   .Handle<Exception>(e => true)
                   .WaitAndRetryAsync(3,
                       retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 0.5 + Math.Pow(2, retryAttempt) * Random.Shared.NextDouble()),
                       (e, ts) =>
                       {
                           _logger.LogError(e, $"error while uploading file {args.FileName}");
                           _logger.LogInformation($"will retry in {(int)ts.TotalSeconds} seconds");
                       });

        try
        {
            return await retryPolicy.ExecuteAsync(async (CancellationToken ct2) =>
            {
                var minioClient = BuildMinioClient();
                var objectName = Guid.NewGuid().ToString() + Path.GetExtension(args.FileName);
                var minioArgs = new PutObjectArgs()
                    .WithBucket(Configuration.BucketName)
                    .WithObject(objectName)
                    .WithStreamData(args.Content)
                    .WithObjectSize(args.ContentLength ?? args.Content.Length)
                    .WithContentType(args.ContentType);
                await minioClient.PutObjectAsync(minioArgs, ct2);
                var internalUrl = $"https://{Configuration.Endpoint}/{Configuration.BucketName}/{objectName}";
                var publicUrl = $"https://{Configuration.PublicEndpoint}/{Configuration.BucketName}/{objectName}";
                _logger.LogInformation($"uploaded to {publicUrl}");

                return new UploadFileOutput(internalUrl, publicUrl);
            }, ct);
        }
        catch(Exception e)
        {
            _logger.LogError(e, $"error while uploading file {args.FileName}");
            throw;
        }
    }

    public string SignGetUrl(string url, TimeSpan? duration = null)
    {
        if (duration == null)
            duration = TimeSpan.FromHours(1);
        if (duration.Value.TotalDays >= 7)
            duration = TimeSpan.FromDays(7);
        var uri = new Uri(url);
        var path = uri.AbsolutePath;
        path = path.Substring(1); // remove first slash (/)
        var objectName = path.Substring(path.IndexOf('/') + 1);

        AmazonS3Config config = new AmazonS3Config()
        {
            ServiceURL = string.Format("https://{0}", Configuration.PublicEndpoint),
            UseHttp = true,
            ForcePathStyle = true
        };

        AWSCredentials creds = new BasicAWSCredentials(Configuration.AccessKey, Configuration.SecretKey);
        using var s3Client = new AmazonS3Client(creds, config);
        return s3Client.GetPreSignedURL(new Amazon.S3.Model.GetPreSignedUrlRequest()
        {
            BucketName = Configuration.BucketName,
            Key = objectName,
            Protocol = Protocol.HTTPS,
            Expires = DateTime.Now.Add(duration.Value)
        });
    }

    public PresignedUploadUrl SignPutUrl(string extension, string? contentType = null, TimeSpan? duration = null)
    {
        extension = extension.Trim();
        if (!extension.StartsWith("."))
            throw new ArgumentException("extension must start with dot ('.')");
        if (duration == null)
            duration = TimeSpan.FromHours(1);
        if (duration.Value.TotalDays >= 7)
            duration = TimeSpan.FromDays(7);

        AmazonS3Config config = new AmazonS3Config()
        {
            ServiceURL = string.Format("https://{0}", Configuration.PublicEndpoint),
            UseHttp = true,
            ForcePathStyle = true
        };

        AWSCredentials creds = new BasicAWSCredentials(Configuration.AccessKey, Configuration.SecretKey);
        using var s3Client = new AmazonS3Client(creds, config);
        var objectName = Guid.NewGuid().ToString() + extension;
        var options = new Amazon.S3.Model.GetPreSignedUrlRequest()
        {
            BucketName = Configuration.BucketName,
            Key = objectName,
            Protocol = Protocol.HTTPS,
            Verb = HttpVerb.PUT,
            Expires = DateTime.Now.Add(duration.Value)
        };
        if (contentType != null)
            options.ContentType = contentType;
        var uploadUrl = s3Client.GetPreSignedURL(options);
        var internalUrl = $"https://{Configuration.Endpoint}/{Configuration.BucketName}/{objectName}";
        var publicUrl = $"https://{Configuration.PublicEndpoint}/{Configuration.BucketName}/{objectName}";
        return new PresignedUploadUrl(internalUrl, publicUrl, uploadUrl);
    }
}

public class MinioConfigurationSection
{
    public string Endpoint { get; private set; }
    public string PublicEndpoint { get; private set; }
    public string AccessKey { get; private set; }
    public string SecretKey { get; private set; }
    public string BucketName { get; private set; }

    public MinioConfigurationSection(string endpoint, string publicEndpoint, string accessKey, string secretKey, string bucketName)
    {
        Endpoint = endpoint?.Trim() ?? throw new ArgumentNullException(nameof(endpoint));
        PublicEndpoint = publicEndpoint?.Trim() ?? throw new ArgumentNullException(nameof(publicEndpoint));
        AccessKey = accessKey ?? throw new ArgumentNullException(nameof(accessKey));
        SecretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
        BucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
    }
}

public class MinioConfigurationSectionBuilder
{
    public string? Endpoint { get; set; }
    public string? PublicEndpoint { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? BucketName { get; set; }

    public MinioConfigurationSection Build() => new MinioConfigurationSection(
        Endpoint ?? throw new InvalidOperationException("Endpoint is null"),
        PublicEndpoint ?? throw new InvalidOperationException("PublicEndpoint is null"),
        AccessKey ?? throw new InvalidOperationException("AccessKey is null"),
        SecretKey ?? throw new InvalidOperationException("SecretKey is null"),
        BucketName ?? throw new InvalidOperationException("BucketName is null"));
}