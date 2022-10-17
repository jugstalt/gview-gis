using System;

namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class PixelFormatExtensions
    {
        static public System.Drawing.Imaging.PixelFormat ToGdiPixelFormat(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Rgb24:
                    return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                case PixelFormat.Rgba32:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                case PixelFormat.Rgb32:
                    return System.Drawing.Imaging.PixelFormat.Format32bppRgb;
                case PixelFormat.Gray8:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;

                //case PixelFormat.Alpha:
                //    return System.Drawing.Imaging.PixelFormat.Alpha;
                //case PixelFormat.Canonical:
                //    return System.Drawing.Imaging.PixelFormat.Canonical;
                //case PixelFormat.DontCare:
                //    return System.Drawing.Imaging.PixelFormat.DontCare;
                //case PixelFormat.Extended:
                //    return System.Drawing.Imaging.PixelFormat.Extended;
                //case PixelFormat.Format16bppArgb1555:
                //    return System.Drawing.Imaging.PixelFormat.Format16bppArgb1555;
                //case PixelFormat.Format16bppGrayScale:
                //    return System.Drawing.Imaging.PixelFormat.Format16bppGrayScale;
                //case PixelFormat.Format16bppRgb555:
                //    return System.Drawing.Imaging.PixelFormat.Format16bppRgb555;
                //case PixelFormat.Format16bppRgb565:
                //    return System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
                //case PixelFormat.Format1bppIndexed:
                //    return System.Drawing.Imaging.PixelFormat.Format1bppIndexed;

                //case PixelFormat.Format32bppPArgb:
                //    return System.Drawing.Imaging.PixelFormat.Format32bppPArgb;

                //case PixelFormat.Format48bppRgb:
                //    return System.Drawing.Imaging.PixelFormat.Format48bppRgb;
                //case PixelFormat.Format4bppIndexed:
                //    return System.Drawing.Imaging.PixelFormat.Format4bppIndexed;
                //case PixelFormat.Format64bppArgb:
                //    return System.Drawing.Imaging.PixelFormat.Format64bppArgb;
                //case PixelFormat.Format64bppPArgb:
                //    return System.Drawing.Imaging.PixelFormat.Format64bppPArgb;

                //case PixelFormat.Gdi:
                //    return System.Drawing.Imaging.PixelFormat.Gdi;
                //case PixelFormat.Indexed:
                //    return System.Drawing.Imaging.PixelFormat.Indexed;
                //case PixelFormat.Max:
                //    return System.Drawing.Imaging.PixelFormat.Max;
                //case PixelFormat.PAlpha:
                //    return System.Drawing.Imaging.PixelFormat.PAlpha;
                default:
                    throw new Exception($"Not Supported PixelFormat: {format}");
            }
        }

        static public PixelFormat ToPixelFormat(this System.Drawing.Imaging.PixelFormat format)
        {
            switch (format)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormat.Rgb24;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormat.Rgba32;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormat.Rgb32;
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return PixelFormat.Gray8;

                //case System.Drawing.Imaging.PixelFormat.Alpha:
                //    return PixelFormat.Alpha;
                //case System.Drawing.Imaging.PixelFormat.Canonical:
                //    return PixelFormat.Canonical;
                //case System.Drawing.Imaging.PixelFormat.DontCare:
                //    return PixelFormat.DontCare;
                //case System.Drawing.Imaging.PixelFormat.Extended:
                //    return PixelFormat.Extended;
                //case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                //    return PixelFormat.Format16bppArgb1555;
                //case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                //    return PixelFormat.Format16bppGrayScale;
                //case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                //    return PixelFormat.Format16bppRgb555;
                //case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                //    return PixelFormat.Format16bppRgb565;
                //case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                //    return PixelFormat.Format1bppIndexed;

                //case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                //    return PixelFormat.Format32bppPArgb;

                //case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                //    return PixelFormat.Format48bppRgb;
                //case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                //    return PixelFormat.Format4bppIndexed;
                //case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
                //    return PixelFormat.Format64bppArgb;
                //case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:
                //    return PixelFormat.Format64bppPArgb;

                //case System.Drawing.Imaging.PixelFormat.Gdi:
                //    return PixelFormat.Gdi;
                //case System.Drawing.Imaging.PixelFormat.Indexed:
                //    return PixelFormat.Indexed;
                //case System.Drawing.Imaging.PixelFormat.Max:
                //    return PixelFormat.Max;
                //case System.Drawing.Imaging.PixelFormat.PAlpha:
                //    return PixelFormat.PAlpha;
                default:
                    throw new Exception($"Not Supported PixelFormat: {format}");
            }
        }
    }
}
