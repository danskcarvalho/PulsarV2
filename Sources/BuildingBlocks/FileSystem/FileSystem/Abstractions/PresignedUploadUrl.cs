namespace Pulsar.BuildingBlocks.FileSystem.Abstractions;

public class PresignedUploadUrl
{
    public string InternalUrl { get; }
    public string PubliclUrl { get; }
    public string UploadUrl { get; }

    public PresignedUploadUrl(string internalUrl, string publiclUrl, string uploadUrl)
    {
        InternalUrl = internalUrl;
        PubliclUrl = publiclUrl;
        UploadUrl = uploadUrl;
    }
}
