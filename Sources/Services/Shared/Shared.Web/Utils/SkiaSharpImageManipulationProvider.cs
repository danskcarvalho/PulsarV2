using SkiaSharp;
using System.Drawing;
using System.Xml.Linq;

namespace Pulsar.Services.Shared.API.Utils;

public class SkiaSharpImageManipulationProvider : IImageManipulationProvider
{
    public (byte[] FileContents, int Height, int Width) Resize(Stream fileContents, int maxWidthHeight)
    {
        using SKBitmap sourceBitmap = SKBitmap.Decode(fileContents);
        SKBitmap? croppedBitmap = null;
        try
        {
            if (sourceBitmap.Width > sourceBitmap.Height)
            {
                var l = (double)sourceBitmap.Width;
                l = l / 2;
                l -= (double)sourceBitmap.Height / 2;
                l = Math.Round(l, MidpointRounding.ToZero);
                var r = SKRectI.Create((int)l, 0, sourceBitmap.Height, sourceBitmap.Height);
                croppedBitmap = new SKBitmap(r.Width, r.Height, sourceBitmap.ColorType, sourceBitmap.AlphaType);
                sourceBitmap.ExtractSubset(croppedBitmap, r);
            }
            else if (sourceBitmap.Height > sourceBitmap.Width)
            {
                var l = (double)sourceBitmap.Height;
                l = l / 2;
                l -= (double)sourceBitmap.Width / 2;
                l = Math.Round(l, MidpointRounding.ToZero);
                var r = SKRectI.Create(0, (int)l, sourceBitmap.Width, sourceBitmap.Width);
                croppedBitmap = new SKBitmap(r.Width, r.Height, sourceBitmap.ColorType, sourceBitmap.AlphaType);
                sourceBitmap.ExtractSubset(croppedBitmap, r);
            }
            else
            {
                croppedBitmap = sourceBitmap;
            }

            if (croppedBitmap.Width > maxWidthHeight)
            {
                using SKBitmap scaledBitmap = croppedBitmap.Resize(new SKImageInfo(maxWidthHeight, maxWidthHeight), SKFilterQuality.High);
                using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
                using SKData data = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 75);
                return (data.ToArray(), maxWidthHeight, maxWidthHeight);
            }
            else
            {
                using SKImage img = SKImage.FromBitmap(croppedBitmap);
                using SKData data = img.Encode(SKEncodedImageFormat.Jpeg, 75);
                return (data.ToArray(), maxWidthHeight, maxWidthHeight);
            }
        }
        finally
        {
            if (croppedBitmap != sourceBitmap && croppedBitmap != null)
                croppedBitmap.Dispose();
        }
    }
}
