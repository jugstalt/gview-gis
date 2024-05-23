using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus;
using gView.GraphicsEngine.Skia;
using System;
using System.IO;

namespace gView.GraphicsEngine.Default
{
    public class BitmapEncoding : IBitmapEncoding
    {
        private readonly SkiaBitmapEncoding _skiaEncoding;
        private readonly GdiBitmapEncoding _gdiEncoding;

        public BitmapEncoding()
        {
            _skiaEncoding = new SkiaBitmapEncoding();
            _gdiEncoding = new GdiBitmapEncoding();
        }

        public string EngineName => "default";

        public bool CanEncode(IBitmap bitmap)
        {
            return true;
        }

        public void Encode(IBitmap bitmap, string filename, ImageFormat format, int quality = 0)
        {
            var encoder = BestEncoder(bitmap, format);
            if (encoder == null)
            {
                throw new Exception($"No image encoder available for {format} on current platform");
            }

            encoder.Encode(bitmap, filename, format, quality);
        }

        public void Encode(IBitmap bitmap, Stream stream, ImageFormat format, int quality = 0)
        {
            var encoder = BestEncoder(bitmap, format);
            if (encoder == null)
            {
                throw new Exception($"No image encoder available for {format} on current platform");
            }

            encoder.Encode(bitmap, stream, format, quality);
        }

        private IBitmapEncoding BestEncoder(IBitmap bitmap, ImageFormat format)
        {
            IBitmapEncoding encoder = null;

            switch (format)
            {
                case ImageFormat.Jpeg:
                case ImageFormat.Png:
                    if (GraphicsPlatform.IsWindows && _gdiEncoding.CanEncode(bitmap))
                    {
                        // Gdi+ encoding: faster & smaller images!
                        encoder = _gdiEncoding;
                    }
                    else
                    {
                        encoder = _skiaEncoding.CanEncode(bitmap) ? _skiaEncoding : null;
                    }
                    break;
                default:
                    encoder = _skiaEncoding.CanEncode(bitmap) ? _skiaEncoding : null;
                    break;
            }

            return encoder;
        }
    }
}
