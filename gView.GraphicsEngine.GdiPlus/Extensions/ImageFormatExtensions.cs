using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class ImageFormatExtensions
    {
        static public System.Drawing.Imaging.ImageFormat ToImageFormat(this ImageFormat format)
        {
            switch (format)
            {
                case ImageFormat.Png:
                    return System.Drawing.Imaging.ImageFormat.Png;
                case ImageFormat.Jpeg:
                    return System.Drawing.Imaging.ImageFormat.Jpeg;
                case ImageFormat.Gif:
                    return System.Drawing.Imaging.ImageFormat.Gif;
                case ImageFormat.Bmp:
                    return System.Drawing.Imaging.ImageFormat.Bmp;
                default:
                    throw new Exception($"Save Bitmap: Format not supported: { format }");
            }
        }
    }
}
