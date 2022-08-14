namespace Pulsar.BuildingBlocks.FileSystemMinio;

public static class DIExtensions
{
    public static void AddMinio(this IServiceCollection col)
    {
        col.AddTransient<IFileSystem, MinioFileSystem>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var endpoint = config["MinIO:Endpoint"];
            var publicEndpoint = config["MinIO:PublicEndpoint"];
            var accessKey = config["MinIO:AccessKey"];
            var secretKey = config["MinIO:SecretKey"];
            var bucketName = config["MinIO:BucketName"];

            var options = new MinioConfigurationSection(endpoint, publicEndpoint, accessKey, secretKey, bucketName);
            var logger = sp.GetRequiredService<ILogger<MinioFileSystem>>();
            return new MinioFileSystem(logger, options);
        });
    }
}
