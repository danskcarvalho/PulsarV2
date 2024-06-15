using System.Drawing;
using System.Xml.Linq;

namespace Pulsar.Services.Shared.API.Utils;

public interface IImageManipulationProvider
{
    public (byte[] FileContents, int Height, int Width) Resize(Stream fileContents, int maxWidthHeight);
}
