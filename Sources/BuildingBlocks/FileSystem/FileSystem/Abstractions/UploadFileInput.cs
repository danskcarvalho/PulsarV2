namespace Pulsar.BuildingBlocks.FileSystem.Abstractions;

public class UploadFileInput
{
    public string FileName { get; }
    public Stream Content { get; }
    public int? ContentLength { get; set; }
    public string? ContentType { get; set; }

    public UploadFileInput(string fileName, Stream content)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }
}
