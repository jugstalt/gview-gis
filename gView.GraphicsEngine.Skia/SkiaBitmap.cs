using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace gView.GraphicsEngine.Skia
{
    internal class SkiaBitmap : IBitmap
    {
        private SKBitmap _bitmap;
        private float _dpiX = 96f, _dpyY = 96f;
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
            _bitmap = new SKBitmap(width, height, colorType: format.ToSKColorType(), alphaType: SKAlphaType.Premul);
        }

        public SkiaBitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
        {
            _bitmap = new SKBitmap(width, height, colorType: format.ToSKColorType(), alphaType: SKAlphaType.Premul);
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

        public int Width => _bitmap != null ? _bitmap.Width : 0;

        public int Height => _bitmap != null ? _bitmap.Height : 0;

        public float DpiX => _dpiX;

        public float DpiY => _dpyY;

        public PixelFormat PixelFormat
        {
            get
            {
                return _bitmap.ColorType.ToPixelFormat();
            }
        }

        public object EngineElement => _bitmap;

        public IBitmap Clone(PixelFormat format)
        {
            if (_bitmap != null)
            {
                return new SkiaBitmap(_bitmap.Copy(format.ToSKColorType()));
            }

            return null;
        }

        public ICanvas CreateCanvas()
        {
            if (_bitmap != null)
            {
                return new SkiaCanvas(new SKCanvas(_bitmap));
            }
            return null;
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
            return _bitmap?.GetPixel(x, y).ToArgbColor() ?? ArgbColor.Empty;
        }

        public BitmapPixelData LockBitmapPixelData(BitmapLockMode lockMode, PixelFormat pixelFormat)
        {
            if (_bitmap != null)
            {
                IntPtr scan0 ;
                if (lockMode == BitmapLockMode.Copy)
                {
                    scan0 = Marshal.AllocHGlobal(_bitmap.ByteCount /*+ _bitmap.RowBytes * 100*/);
                    // Copy the array to unmanaged memory.
                    Marshal.Copy(_bitmap.Bytes, 0, scan0, _bitmap.ByteCount);
                }
                else
                {
                    scan0 = _bitmap.GetPixels();
                }

                return new BitmapPixelData(lockMode)
                {
                    Width = _bitmap.Width,
                    Height = _bitmap.Height,
                    Stride = _bitmap.RowBytes,
                    PixelFormat = _bitmap.ColorType.ToPixelFormat(),
                    Scan0 = scan0
                    //Scan0 = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(_bitmap.Bytes, 0)
                    //Scan0 = _bitmap.GetPixels()
                };
            }

            return null;
        }

        public void UnlockBitmapPixelData(BitmapPixelData bitmapPixelData)
        {
            if (bitmapPixelData.LockMode == BitmapLockMode.Copy && bitmapPixelData.Scan0 != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(bitmapPixelData.Scan0);
            }
        }

        public void MakeTransparent()
        {
            using(var canvas = new SKCanvas(_bitmap))
            {
                canvas.Clear(SKColors.Transparent);
            }
        }

        public void MakeTransparent(ArgbColor color)
        {
        }

        public void Save(string filename, ImageFormat format, int quality = 0)
        {
            //var image = SKImage.FromBitmap(_bitmap);
            //using (var data = image.Encode(SKEncodedImageFormat.Png, 50))
            //{
            //    using (var stream = File.OpenWrite(filename))
            //    {
            //        data.SaveTo(stream);
            //    }
            //}

            using (var bm = new System.Drawing.Bitmap(
                        _bitmap.Width,
                        _bitmap.Height,
                        _bitmap.RowBytes,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                        Marshal.UnsafeAddrOfPinnedArrayElement(_bitmap.Bytes, 0)))
            {
                bm.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        public void Save(Stream stream, ImageFormat format, int quality = 0)
        {
            //var image = SKImage.FromBitmap(_bitmap);
            //using (var data = image.Encode(SKEncodedImageFormat.Png, 75))
            //{
            //    stream.Write(data.ToArray(), 0, (int)data.Size);
            //}

            using (var bm = new System.Drawing.Bitmap(
                        _bitmap.Width,
                        _bitmap.Height,
                        _bitmap.RowBytes,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                        Marshal.UnsafeAddrOfPinnedArrayElement(_bitmap.Bytes, 0)))
            {
                bm.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        public void SetResolution(float dpiX, float dpiY)
        {
            _dpiX = dpiX;
            _dpyY = dpiY;
        }
    }
}