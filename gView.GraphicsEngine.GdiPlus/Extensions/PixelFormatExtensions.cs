using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class PixelFormatExtensions
    {
        static public System.Drawing.Imaging.PixelFormat ToPixelFormat(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Alpha:
                    return System.Drawing.Imaging.PixelFormat.Alpha;
                case PixelFormat.Canonical:
                    return System.Drawing.Imaging.PixelFormat.Canonical;
                case PixelFormat.DontCare:
                    return System.Drawing.Imaging.PixelFormat.DontCare;
                case PixelFormat.Extended:
                    return System.Drawing.Imaging.PixelFormat.Extended;
                case PixelFormat.Format16bppArgb1555:
                    return System.Drawing.Imaging.PixelFormat.Format16bppArgb1555;
                case PixelFormat.Format16bppGrayScale:
                    return System.Drawing.Imaging.PixelFormat.Format16bppGrayScale;
                case PixelFormat.Format16bppRgb555:
                    return System.Drawing.Imaging.PixelFormat.Format16bppRgb555;
                case PixelFormat.Format16bppRgb565:
                    return System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
                case PixelFormat.Format1bppIndexed:
                    return System.Drawing.Imaging.PixelFormat.Format1bppIndexed;
                case PixelFormat.Format24bppRgb:
                    return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                case PixelFormat.Format32bppArgb:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                case PixelFormat.Format32bppPArgb:
                    return System.Drawing.Imaging.PixelFormat.Format32bppPArgb;
                case PixelFormat.Format32bppRgb:
                    return System.Drawing.Imaging.PixelFormat.Format32bppRgb;
                case PixelFormat.Format48bppRgb:
                    return System.Drawing.Imaging.PixelFormat.Format48bppRgb;
                case PixelFormat.Format4bppIndexed:
                    return System.Drawing.Imaging.PixelFormat.Format4bppIndexed;
                case PixelFormat.Format64bppArgb:
                    return System.Drawing.Imaging.PixelFormat.Format64bppArgb;
                case PixelFormat.Format64bppPArgb:
                    return System.Drawing.Imaging.PixelFormat.Format64bppPArgb;
                case PixelFormat.Format8bppIndexed:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                case PixelFormat.Gdi:
                    return System.Drawing.Imaging.PixelFormat.Gdi;
                case PixelFormat.Indexed:
                    return System.Drawing.Imaging.PixelFormat.Indexed;
                case PixelFormat.Max:
                    return System.Drawing.Imaging.PixelFormat.Max;
                case PixelFormat.PAlpha:
                    return System.Drawing.Imaging.PixelFormat.PAlpha;
                default:
                    throw new Exception($"Save Bitmap: Format not supported: { format }");
            }
        }
    }
}
