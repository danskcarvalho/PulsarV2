using Microsoft.AspNetCore.Components.Forms;

namespace Pulsar.Web.Client.Models.Shared
{
    public class BrowserFile(string contentType, FileInfo fileInfo) : IBrowserFile
    {
        public string Name => ChangeExtension(fileInfo.Name, contentType);

        private string ChangeExtension(string name, string contentType)
        {
            switch (contentType)
            {
                case "image/jpeg":
                    return Path.GetFileNameWithoutExtension(name) + ".jpeg";
                case "image/jpg":
                    return Path.GetFileNameWithoutExtension(name) + ".jpg";
                case "image/png":
                    return Path.GetFileNameWithoutExtension(name) + ".png";
                default:
                    return name;
            }
        }

        public DateTimeOffset LastModified => fileInfo.LastWriteTimeUtc;

        public long Size => fileInfo.Length;

        public string ContentType => contentType;

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            return fileInfo.OpenRead();
        }
    }
}
