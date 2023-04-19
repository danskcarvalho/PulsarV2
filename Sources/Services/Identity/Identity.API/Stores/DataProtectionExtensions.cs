using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;

namespace Pulsar.Services.Identity.API.Stores
{
    public static class DataProtectionExtensions
    {
        public static void ConfigureDataProtection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDataProtection()
                .PersistKeysToAzureBlobStorage(sp =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();
                    var serviceUri = config.GetOrThrow("Azure:BlobStorage:ServiceUri");
                    var containerName = config.GetOrThrow("Azure:BlobStorage:DataProtection:Container");
                    var blobName = config.GetOrThrow("Azure:BlobStorage:DataProtection:Blob");
                    var blobServiceClient = new BlobServiceClient(new Uri(serviceUri), new DefaultAzureCredential());
                    var container = blobServiceClient.GetBlobContainerClient(containerName);
                    return container.GetBlobClient(blobName);
                })
                .ProtectKeysWithAzureKeyVault(new Uri(configuration.GetOrThrow("Azure:KeyVault:DataProtection:KeyId")), new DefaultAzureCredential());
        }
    }
}
