namespace Pulsar.BuildingBlocks.FileSystemAzure;

public static class DIExtensions
{
    public static void AddAzureBlobStorage(this IServiceCollection col)
    {
        col.AddScoped<IFileSystem>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILogger<AzureFileSystem>>();

            return new AzureFileSystem(
                logger,
                config.GetOrThrow("Azure:BlobStorage:ServiceUri"),
                config["Azure:BlobStorage:Containers:Public"],
                config["Azure:BlobStorage:Containers:Private"]);
        });
    }
}
