using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System;
using System.Drawing;
using System.IO;

namespace gView.GraphicsEngine.GdiPlus
{
    internal class GdiBitmap : IBitmap
    {
        private Bitmap _bitmap;

        #region Constructors

        private GdiBitmap(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public GdiBitmap(int width, int height)
        {
            _bitmap = new Bitmap(width, height);
        }

        public GdiBitmap(int width, int height, PixelFormat format)
        {
            _bitmap = new Bitmap(width, height, format.ToGdiPixelFormat());
        }

        public GdiBitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
        {
            _bitmap = new Bitmap(width, height, stride, format.ToGdiPixelFormat(), scan0);
        }

        public GdiBitmap(Stream stream)
        {
            _bitmap = (Bitmap)Bitmap.FromStream(stream);
        }

        public GdiBitmap(string filename)
        {
            _bitmap = (Bitmap)Bitmap.FromFile(filename);
        }

        #endregion Constructors

        #region IBitmap

        public int Width => _bitmap != null ? _bitmap.Width : 0;
        public int Height => _bitmap != null ? _bitmap.Height : 0;

        public PixelFormat PixelFormat => _bitmap != null ? _bitmap.PixelFormat.ToPixelFormat() : PixelFormat.DontCare;

        public object EngineElement => _bitmap;

        public ICanvas CreateCanvas()
        {
            return new Canvas(_bitmap);
        }

        public void MakeTransparent()
        {
            if (_bitmap != null)
            {
                _bitmap.MakeTransparent();
            }
        }

        public void MakeTransparent(ArgbColor color)
        {
            if (_bitmap != null)
            {
                _bitmap.MakeTransparent(color.ToGdiColor());
            }
        }

        public IBitmap Clone(PixelFormat format) =>
            _bitmap == null ? 
                null :
                new GdiBitmap(_bitmap.Clone(new System.Drawing.Rectangle(0, 0, _bitmap.Width, _bitmap.Height), format.ToGdiPixelFormat()));

        public void Save(string filename, ImageFormat format, int quality = 0)
        {
            _bitmap.Save(filename, format.ToImageFormat());
        }

        public void Save(Stream stream, ImageFormat format, int quality = 0)
        {
            _bitmap.Save(stream, format.ToImageFormat());
        }
        public BitmapPixelData LockBitmapPixelData(BitmapLockMode lockMode, PixelFormat pixelFormat)
        {
            var bitmapData =_bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
                                             lockMode.ToGidImageLockMode(),
                                             pixelFormat.ToGdiPixelFormat());

            return new GdiBitmapPixelData(bitmapData);
        }

        public void UnlockBitmapPixelData(BitmapPixelData bitmapPixelData)
        {
            if(bitmapPixelData is GdiBitmapPixelData)
            {
                _bitmap.UnlockBits(((GdiBitmapPixelData)bitmapPixelData).BitmapData);
            }
        }


        #endregion IBitmap

        #region IDisposable

        public void Dispose()
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }

        #endregion IDisposable

        #region Classes

        public class GdiBitmapPixelData : BitmapPixelData
        {
            private readonly System.Drawing.Imaging.BitmapData _bitmapData; 
            public GdiBitmapPixelData(System.Drawing.Imaging.BitmapData bitmapData)
            {
                _bitmapData = bitmapData;

                this.PixelFormat = bitmapData.PixelFormat.ToPixelFormat();
                this.Reserved = bitmapData.Reserved;
                this.Stride = bitmapData.Stride;
                this.Scan0 = bitmapData.Scan0;
                this.Width = bitmapData.Width;
                this.Height = bitmapData.Height;
            }

            internal System.Drawing.Imaging.BitmapData BitmapData;
        }
 
        #endregion
    }
}