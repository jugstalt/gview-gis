using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace gView.GraphicsEngine.Skia
{
    class SkiaBitmap : IBitmap
    {
        private SKBitmap _bitmap;

        private SkiaBitmap(SKBitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public SkiaBitmap(int width, int height)
        {
            _bitmap = new SKBitmap(width, height, true);
        }

        public SkiaBitmap(int width, int height, PixelFormat format)
        {
            _bitmap = new SKBitmap(width, height, colorType: format.ToSKColorType(), alphaType: SKAlphaType.Opaque);
        }

        public SkiaBitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
        {
            _bitmap = new SKBitmap(width, height, colorType: format.ToSKColorType(), alphaType: SKAlphaType.Opaque);
            _bitmap.SetPixels(scan0);
        }

        public SkiaBitmap(Stream stream)
        {
            _bitmap = SKBitmap.Decode(stream);
        }

        public SkiaBitmap(string filename)
        {
            _bitmap = SKBitmap.Decode(filename);
        }

        public int Width => _bitmap.Width;

        public int Height => _bitmap.Height;

        public float DpiX => 144f;

        public float DpiY => 144f;

        public PixelFormat PixelFormat
        {
            get
            {
                switch(_bitmap.BytesPerPixel)
                {
                    case 4:
                        return PixelFormat.Format32bppArgb;
                    case 3:
                        return PixelFormat.Format24bppRgb;
                    case 1:
                        return PixelFormat.Format8bppIndexed;
                }

                return PixelFormat.Format32bppArgb;
            }
        }

        public object EngineElement => _bitmap;

        public IBitmap Clone(PixelFormat format)
        {
            return new SkiaBitmap(_bitmap.Copy(format.ToSKColorType()));
        }

        public ICanvas CreateCanvas()
        {
            return new SkiaCanvas(new SKCanvas(_bitmap));
        }

        public void Dispose()
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }

        public ArgbColor GetPixel(int x, int y)
        {
            return _bitmap.GetPixel(x, y).ToArgbColor();
        }

        public BitmapPixelData LockBitmapPixelData(BitmapLockMode lockMode, PixelFormat pixelFormat)
        {
            var bytes = _bitmap.GetPixels();

            return new BitmapPixelData()
            {
                Width = _bitmap.Width,
                Height = _bitmap.Height,
                Stride = _bitmap.Info.RowBytes,
                PixelFormat = pixelFormat,
                Scan0 = _bitmap.GetPixels()
            };
        }

        public void MakeTransparent()
        {
            
        }

        public void MakeTransparent(ArgbColor color)
        {
            
        }

        public void Save(string filename, ImageFormat format, int quality = 0)
        {
            
        }

        public void Save(Stream stream, ImageFormat format, int quality = 0)
        {
           
        }

        public void SetResolution(float dpiX, float dpiY)
        {
            
        }

        public void UnlockBitmapPixelData(BitmapPixelData bitmapPixelData)
        {
            // DoTo ?
        }
    }
}
