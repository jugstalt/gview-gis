using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static public class PixelFormatExtensions
    {
        static public SKColorType ToSKColorType(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Alpha:
                    return SKColorType.Alpha8;
                case PixelFormat.Canonical:
                    return SKColorType.Unknown;
                case PixelFormat.DontCare:
                    return SKColorType.Unknown;
                case PixelFormat.Extended:
                    return SKColorType.Unknown;
                case PixelFormat.Format16bppArgb1555:
                    return SKColorType.RgbaF16;
                case PixelFormat.Format16bppGrayScale:
                    return SKColorType.RgF16;
                case PixelFormat.Format16bppRgb555:
                    return SKColorType.Rgba16161616; // ??
                case PixelFormat.Format16bppRgb565:
                    return SKColorType.Rgb565; // ??
                case PixelFormat.Format1bppIndexed:
                    return SKColorType.RgbaF16Clamped; // ??
                case PixelFormat.Format24bppRgb:
                    return SKColorType.Rgb888x;
                case PixelFormat.Format32bppArgb:
                    return SKColorType.Rgba8888;
                case PixelFormat.Format32bppPArgb:
                    return SKColorType.RgbaF32;  // ??
                case PixelFormat.Format32bppRgb:
                    return SKColorType.RgbaF32;  // ??
                case PixelFormat.Format48bppRgb:
                    return SKColorType.Unknown;
                case PixelFormat.Format4bppIndexed:
                    return SKColorType.Unknown;
                case PixelFormat.Format64bppArgb:
                    return SKColorType.Unknown;
                case PixelFormat.Format64bppPArgb:
                    return SKColorType.Unknown;
                case PixelFormat.Format8bppIndexed:
                    return SKColorType.Gray8;
                case PixelFormat.Gdi:
                    return SKColorType.Unknown;
                case PixelFormat.Indexed:
                    return SKColorType.Unknown;
                case PixelFormat.Max:
                    return SKColorType.Unknown;
                case PixelFormat.PAlpha:
                    return SKColorType.Unknown;
                default:
                    throw new Exception($"Not Supported PixelFormat: { format }");
            }
        }

        static public PixelFormat ToPixelFormat(this SKColorType colorType)
        {
            switch(colorType)
            {
                case SKColorType.Alpha8:
                    return PixelFormat.Alpha;
                case SKColorType.RgbaF16:
                    return PixelFormat.Format16bppArgb1555;
                case SKColorType.RgF16:
                    return PixelFormat.Format16bppGrayScale;
                case SKColorType.Rgba16161616:
                    return PixelFormat.Format16bppRgb555; // ??
                case SKColorType.Rgb565:
                    return PixelFormat.Format16bppRgb565; // ??
                case SKColorType.RgbaF16Clamped:
                    return PixelFormat.Format1bppIndexed; // ??
                case SKColorType.Rgb888x:
                    return PixelFormat.Format24bppRgb;
                case SKColorType.Rgba8888:
                    return PixelFormat.Format32bppArgb;
                case SKColorType.Rgba1010102: 
                    return PixelFormat.Format32bppPArgb;  // ??
                case SKColorType.RgbaF32:
                    return PixelFormat.Format32bppRgb;  // ??
                case SKColorType.Gray8:
                    return PixelFormat.Format8bppIndexed;
;
                default:
                    throw new Exception($"Not Supported ColorType: { colorType }");
            }
        }
    }
}
