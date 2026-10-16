using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Options;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Util;

/// <summary>
///     Provides functionality to process images
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    ///     Resizes and encodes the image
    /// </summary>
    /// <param name="imageData">User provided image data</param>
    /// <returns>A JPG image with max. size as configured</returns>
    public byte[] ProcessImage(byte[] imageData);
}

internal sealed class ImageProcessor(IOptions<Settings> options) : IImageProcessor
{
    private const int JpegQuality = 75;

    private readonly int _maxImageSize = options.Value.MaxImageSizePixel;

    public byte[] ProcessImage(byte[] imageData)
    {
        using var input = new MemoryStream(imageData);
        using var decoded = new Bitmap(input);
        var resizedSize = GetResizedSize(decoded.PixelSize);
        using var resized = resizedSize == decoded.PixelSize
            ? null
            : decoded.CreateScaledBitmap(resizedSize, BitmapInterpolationMode.HighQuality);

        using var output = new MemoryStream();
        (resized ?? decoded).Save(output, new JpegBitmapEncoderOptions { Quality = JpegQuality });

        return output.ToArray();
    }

    private PixelSize GetResizedSize(PixelSize size)
    {
        // TODO - ensure largest side is not larger than max size as configured in the application settings
        throw new NotImplementedException();
    }
}
