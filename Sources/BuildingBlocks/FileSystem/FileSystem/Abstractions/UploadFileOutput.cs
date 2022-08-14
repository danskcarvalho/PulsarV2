namespace Pulsar.BuildingBlocks.FileSystem.Abstractions;

public class UploadFileOutput
{
    public string InternalUrl { get; }
    public string PubliclUrl { get; }

    public UploadFileOutput(string internalUrl, string publiclUrl)
    {
        InternalUrl = internalUrl;
        PubliclUrl = publiclUrl;
    }
}