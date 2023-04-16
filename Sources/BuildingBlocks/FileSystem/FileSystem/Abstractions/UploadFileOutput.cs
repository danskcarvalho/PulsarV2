namespace Pulsar.BuildingBlocks.FileSystem.Abstractions;

public class UploadFileOutput
{
    public string Url { get; }
    public string Key { get; }

    public UploadFileOutput(string url, string key)
    {
        Url = url;
        Key = key;
    }
}