using SkiaSharp;
using System;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static public class PixelFormatExtensions
    {
        static public SKColorType ToSKColorType(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Rgb24:
                case PixelFormat.Rgb32:
                case PixelFormat.Rgba32:
                    return /*OsPlatform.IsWindows ?*/ SKColorType.Bgra8888 /*: SKColorType.Rgba8888*/;
                case PixelFormat.Gray8:
                    return SKColorType.Gray8;
                default:
                    throw new Exception($"Not Supported PixelFormat: {format}");
            }
        }

        static public PixelFormat ToPixelFormat(this SKColorType colorType)
        {
            switch (colorType)
            {
                case SKColorType.Bgra8888:
                case SKColorType.Rgba8888:
                    return PixelFormat.Rgba32;
                case SKColorType.Gray8:
                    return PixelFormat.Gray8;
                default:
                    throw new Exception($"Not Supported ColorType: {colorType}");
            }
        }
    }
}
