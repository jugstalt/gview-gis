using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System;
using System.IO;

namespace gView.GraphicsEngine.GdiPlus
{
    public class GdiBitmapEncoding : IBitmapEncoding
    {
        public string EngineName => "GdiPlus";

        public bool CanEncode(IBitmap bitmap)
        {
            return bitmap != null && (bitmap.EngineElement is System.Drawing.Bitmap || bitmap.PixelFormat == PixelFormat.Rgb32 || bitmap.PixelFormat == PixelFormat.Rgba32);
        }

        public void Encode(IBitmap bitmap, string filename, ImageFormat format, int quality = 0)
        {
            if (bitmap == null)
            {
                throw new ArgumentException("Bitmap is NULL");
            }

            if (bitmap.EngineElement is System.Drawing.Bitmap)
            {
                ((System.Drawing.Bitmap)bitmap.EngineElement).Save(filename, format.ToImageFormat());
            }
            else
            {
                BitmapPixelData pixelData = null;
                try
                {
                    pixelData = bitmap.LockBitmapPixelData(BitmapLockMode.ReadWrite, bitmap.PixelFormat);
                    using (var bm = new System.Drawing.Bitmap(
                            pixelData.Width,
                            pixelData.Height,
                            pixelData.Stride,
                            pixelData.PixelFormat.ToGdiPixelFormat(),
                            pixelData.Scan0))
                    {
                        bm.Save(filename, format.ToImageFormat());
                    }
                }
                finally
                {
                    if (pixelData != null)
                    {
                        bitmap.UnlockBitmapPixelData(pixelData);
                    }
                }
            }
        }

        public void Encode(IBitmap bitmap, Stream stream, ImageFormat format, int quality = 0)
        {
            if (bitmap == null)
            {
                throw new ArgumentException("Bitmap is NULL");
            }

            if (bitmap.EngineElement is System.Drawing.Bitmap)
            {
                ((System.Drawing.Bitmap)bitmap.EngineElement).Save(stream, format.ToImageFormat());
            }
            else
            {
                BitmapPixelData pixelData = null;
                try
                {
                    pixelData = bitmap.LockBitmapPixelData(BitmapLockMode.ReadWrite, bitmap.PixelFormat);

                    //Console.WriteLine("Width: " + pixelData.Width);
                    //Console.WriteLine("Height: " + pixelData.Height);
                    //Console.WriteLine("Stride: " + pixelData.Stride);
                    //Console.WriteLine("Format: " + pixelData.PixelFormat);
                    //Console.WriteLine("Scan0: " + pixelData.Scan0);

                    using (var bm = new System.Drawing.Bitmap(
                            pixelData.Width,
                            pixelData.Height,
                            pixelData.Stride,
                            pixelData.PixelFormat.ToGdiPixelFormat(),
                            pixelData.Scan0))
                    {
                        bm.Save(stream, format.ToImageFormat());
                    }
                }
                finally
                {
                    if (pixelData != null)
                    {
                        bitmap.UnlockBitmapPixelData(pixelData);
                    }
                }
            }
        }
    }
}
