using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System;
using System.IO;

namespace gView.GraphicsEngine.GdiPlus;

public class GdiBitmapEncoding : IBitmapEncoding
{
    private const int DefaultQuality = 75;
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

        quality = Math.Max(0, quality <= 0 ? DefaultQuality : Math.Min(quality, 100));

        if (bitmap.EngineElement is System.Drawing.Bitmap b)
        {
            if (quality > 0 && format == ImageFormat.Jpeg)
            {
                SaveJpegWithQuality(b, filename, quality);
            }
            else
            {
                b.Save(filename, format.ToImageFormat());
            }
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
                    if (quality > 0 && format == ImageFormat.Jpeg)
                    {
                        SaveJpegWithQuality(bm, filename, quality);
                    }
                    else
                    {
                        bm.Save(filename, format.ToImageFormat());
                    }
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

        quality = Math.Max(0, quality <= 0 ? DefaultQuality : Math.Min(quality, 100));

        if (bitmap.EngineElement is System.Drawing.Bitmap b)
        {
            if (quality > 0 && format == ImageFormat.Jpeg)
            {
                SaveJpegWithQuality(b, stream, quality);
            }
            else
            {
                b.Save(stream, format.ToImageFormat());
            }
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
                    if (quality > 0 && format == ImageFormat.Jpeg)
                    {
                        SaveJpegWithQuality(bm, stream, quality);
                    }
                    else
                    {
                        bm.Save(stream, format.ToImageFormat());
                    }
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

    #region Helper

    private void SaveJpegWithQuality(System.Drawing.Bitmap bitmap, string outputPath, long quality)
    {
        var jpegEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);

        if (jpegEncoder == null)
        {
            throw new Exception("JPEG Encoder nicht gefunden.");
        }

        var qualityParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
        var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
        encoderParams.Param[0] = qualityParam;

        bitmap.Save(outputPath, jpegEncoder, encoderParams);
    }

    private void SaveJpegWithQuality(System.Drawing.Bitmap bitmap, Stream outputStream, long quality)
    {
        var jpegEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);

        if (jpegEncoder == null)
        {
            throw new Exception("JPEG Encoder not found.");
        }

        var qualityParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
        var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
        encoderParams.Param[0] = qualityParam;

        bitmap.Save(outputStream, jpegEncoder, encoderParams);
    }

    private System.Drawing.Imaging.ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
    {
        System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders();

        foreach (System.Drawing.Imaging.ImageCodecInfo codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }
        return null;
    }

    #endregion
}
