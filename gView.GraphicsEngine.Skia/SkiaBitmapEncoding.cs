using gView.GraphicsEngine.Abstraction;
using SkiaSharp;
using System;
using System.IO;

namespace gView.GraphicsEngine.Skia
{
    public class SkiaBitmapEncoding : IBitmapEncoding
    {
        public string EngineName => "SkiaSharp";

        public bool CanEncode(IBitmap bitmap)
        {
            return bitmap?.EngineElement is SKBitmap;
        }

        public void Encode(IBitmap bitmap, string filename, ImageFormat format, int quality = 0)
        {
            if (!(bitmap?.EngineElement is SKBitmap))
            {
                throw new ArgumentException($"SkiaBitmapEncoding can't encode {bitmap?.EngineElement?.GetType()}");
            }

            var image = SKImage.FromBitmap((SKBitmap)bitmap.EngineElement);
            using (var data = EncodedData(image, format, quality))
            {
                using (var stream = File.OpenWrite(filename))
                {
                    data.SaveTo(stream);
                }
            }
        }

        public void Encode(IBitmap bitmap, Stream stream, ImageFormat format, int quality = 0)
        {
            if (!(bitmap?.EngineElement is SKBitmap))
            {
                throw new ArgumentException($"SkiaBitmapEncoding can't encode {bitmap?.EngineElement?.GetType()}");
            }

            var image = SKImage.FromBitmap((SKBitmap)bitmap.EngineElement);
            using (var data = EncodedData(image, format, quality))
            {
                if (data == null)
                {
                    throw new ArgumentException($"Image not encodable to {format}");
                }

                stream.Write(data.ToArray(), 0, (int)data.Size);
            }
        }

        #region Helper

        private SKData EncodedData(SKImage image, ImageFormat format, int quality = 0)
        {
            switch (format)
            {
                case ImageFormat.Png:
                    return image.Encode(SKEncodedImageFormat.Png, quality > 0 ? quality : 75);
                case ImageFormat.Jpeg:
                    return image.Encode(SKEncodedImageFormat.Jpeg, quality > 0 ? quality : 75);
                case ImageFormat.Webp:
                    return image.Encode(SKEncodedImageFormat.Webp, quality > 0 ? quality : 75);
                case ImageFormat.Gif:
                    return image.Encode(SKEncodedImageFormat.Gif, quality > 0 ? quality : 75);
                case ImageFormat.Bmp:
                    return image.Encode(SKEncodedImageFormat.Bmp, quality > 0 ? quality : 75);
                case ImageFormat.Heif:
                    return image.Encode(SKEncodedImageFormat.Heif, quality > 0 ? quality : 75);
                case ImageFormat.Astc:
                    return image.Encode(SKEncodedImageFormat.Astc, quality > 0 ? quality : 75);
                case ImageFormat.Dng:
                    return image.Encode(SKEncodedImageFormat.Dng, quality > 0 ? quality : 75);
                case ImageFormat.Ico:
                    return image.Encode(SKEncodedImageFormat.Ico, quality > 0 ? quality : 75);
                case ImageFormat.Ktx:
                    return image.Encode(SKEncodedImageFormat.Ktx, quality > 0 ? quality : 75);
                case ImageFormat.Pkm:
                    return image.Encode(SKEncodedImageFormat.Pkm, quality > 0 ? quality : 75);
                case ImageFormat.Wbmp:
                    return image.Encode(SKEncodedImageFormat.Wbmp, quality > 0 ? quality : 75);
                default:
                    throw new Exception($"Unsported image format: {format}");
            }
        }

        #endregion
    }
}
