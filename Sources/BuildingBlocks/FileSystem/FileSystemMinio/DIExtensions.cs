using Pulsar.BuildingBlocks.Utils;

namespace Pulsar.BuildingBlocks.FileSystemMinio;

public static class DIExtensions
{
    public static void AddMinio(this IServiceCollection col)
    {
        col.AddTransient<IFileSystem, MinioFileSystem>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var endpoint = config.GetOrThrow("MinIO:Endpoint");
            var publicEndpoint = config.GetOrThrow("MinIO:PublicEndpoint");
            var accessKey = config.GetOrThrow("MinIO:AccessKey");
            var secretKey = config.GetOrThrow("MinIO:SecretKey");
            var bucketName = config.GetOrThrow("MinIO:BucketName");

            var options = new MinioConfigurationSection(endpoint!, publicEndpoint!, accessKey!, secretKey!, bucketName!);
            var logger = sp.GetRequiredService<ILogger<MinioFileSystem>>();
            return new MinioFileSystem(logger, options);
        });
    }
}
