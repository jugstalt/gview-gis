using gView.GraphicsEngine.Abstraction;
using OSGeo_v3.GDAL;
using System;
using System.IO;

namespace gView.GraphicsEngine.Gdal
{
    public class GdalBitmapEncoding : IBitmapEncoding
    {
        public GdalBitmapEncoding()
        {
            DataSources.OSGeo.Initializer.RegisterAll();
        }

        public string EngineName => "gdal";

        public bool CanEncode(IBitmap bitmap)
        {
            return
                DataSources.OSGeo.Initializer.InstalledVersion == DataSources.OSGeo.GdalVersion.V3 && 
                (bitmap.PixelFormat == PixelFormat.Rgb32 || bitmap.PixelFormat == PixelFormat.Rgba32);
        }

        public void Encode(IBitmap bitmap, string filename, ImageFormat format, int quality = 0)
        {
            BitmapPixelData pixelData = null;
            try
            {
                pixelData = bitmap.LockBitmapPixelData(BitmapLockMode.ReadWrite, bitmap.PixelFormat);

                using (var outImage = OSGeo_v3.GDAL.Gdal.GetDriverByName("GTiff")
                    .Create(filename,
                            pixelData.Width,
                            pixelData.Height,
                            4,
                            DataType.GDT_Byte,
                            null))
                {

                    Band outRedBand = outImage.GetRasterBand(1);
                    //Band outGreenBand = outImage.GetRasterBand(2);
                    //Band outBlueBand = outImage.GetRasterBand(3);
                    //Band outAlphaBand = outImage.GetRasterBand(4);

                    outRedBand.WriteRaster(
                        0, 0,
                        pixelData.Width, pixelData.Height,
                        pixelData.Scan0,
                        pixelData.Height, pixelData.Height,
                        DataType.GDT_Byte,
                        4, 0);

                    outImage.FlushCache();
                }

                //using (var bm = new System.Drawing.Bitmap(
                //        pixelData.Width,
                //        pixelData.Height,
                //        pixelData.Stride,
                //        pixelData.PixelFormat.ToGdiPixelFormat(),
                //        pixelData.Scan0))
                //{
                //    bm.Save(filename, format.ToImageFormat());
                //}
            }
            finally
            {
                if (pixelData != null)
                {
                    bitmap.UnlockBitmapPixelData(pixelData);
                }
            }
        }

        public void Encode(IBitmap bitmap, Stream stream, ImageFormat format, int quality = 0)
        {
            throw new NotImplementedException();
        }
    }
}
