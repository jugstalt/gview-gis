using gView.GraphicsEngine.Abstraction;
using System;
using System.IO;

namespace gView.GraphicsEngine.Gdal
{
    public class GdalBitmapEncoding : IBitmapEncoding
    {
        public string EngineName => throw new NotImplementedException();

        public bool CanEncode(IBitmap bitmap)
        {
            return bitmap.PixelFormat == PixelFormat.Rgb32 || bitmap.PixelFormat == PixelFormat.Rgba32;
        }

        public void Encode(IBitmap bitmap, string filename, ImageFormat format, int quality = 0)
        {
            throw new NotImplementedException();
        }

        public void Encode(IBitmap bitmap, Stream stream, ImageFormat format, int quality = 0)
        {
            throw new NotImplementedException();
        }
    }
}
