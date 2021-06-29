using gView.Drawing.Pro.Filters;
using System;
using System.Collections.Generic;
using System.IO;

namespace gView.Drawing.Pro
{
    public enum ImageProcessingFilters
    {
        Default,
        GrayscaleBT709,
        GrayscaleRMY,
        GrayscaleY,
        Channel_Red,
        Channel_Green,
        Channel_Blue,
        Channel_Alpha
    }

    public enum ImageProcessingFilterCategory
    {
        All = 0,
        Standard = 1,
        Art = 2,
        Color = 4,
        Correction = 8
    }

    public class ImageProcessing
    {
        #region Filters (System.Drawing)

        private static byte[] ApplyFilter(byte[] imageBytes, IFilter filter, System.Drawing.Imaging.ImageFormat format = null)
        {
            if (imageBytes == null)
                return null;

            try
            {
                using (var from = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(new MemoryStream(imageBytes)))
                {
                    using (System.Drawing.Bitmap bm = filter.Apply(from))
                    {
                        return ImageOperations.Image2Bytes(bm, format ?? from.RawFormat);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return null;
        }

        public static byte[] ApplyFilter(System.Drawing.Image image, ImageProcessingFilters filter, System.Drawing.Imaging.ImageFormat format = null)
        {
            try
            {
                return ApplyFilter(ImageOperations.Image2Bytes(image), filter, format);
            }
            catch { return null; }
        }

        public static byte[] ApplyFilter(byte[] imageBytes, ImageProcessingFilters filter, System.Drawing.Imaging.ImageFormat format = null)
        {
            IFilter baseFilter = null;

            switch (filter)
            {
                case ImageProcessingFilters.Default:
                    return imageBytes;
                case ImageProcessingFilters.GrayscaleBT709:
                    baseFilter = Grayscale.BT709;
                    break;
                case ImageProcessingFilters.GrayscaleRMY:
                    baseFilter = Grayscale.RMY;
                    break;
                case ImageProcessingFilters.GrayscaleY:
                    baseFilter = Grayscale.Y;
                    break;
                case ImageProcessingFilters.Channel_Red:
                    baseFilter = ExtractChannel.R;
                    break;
                case ImageProcessingFilters.Channel_Green:
                    baseFilter = ExtractChannel.G;
                    break;
                case ImageProcessingFilters.Channel_Blue:
                    baseFilter = ExtractChannel.B;
                    break;
                case ImageProcessingFilters.Channel_Alpha:
                    baseFilter = ExtractChannel.A;
                    break;
            }

            if (baseFilter == null)
                return null;

            return ApplyFilter(imageBytes, baseFilter, format);
        }

        #endregion

        #region Filters (GraphicsEngine) 

        public static byte[] ApplyFilter(byte[] imageBytes, ImageProcessingFilters filter, GraphicsEngine.ImageFormat format)
        {
            IFilter baseFilter = null;

            switch (filter)
            {
                case ImageProcessingFilters.Default:
                    return imageBytes;
                case ImageProcessingFilters.GrayscaleBT709:
                    baseFilter = Grayscale.BT709;
                    break;
                case ImageProcessingFilters.GrayscaleRMY:
                    baseFilter = Grayscale.RMY;
                    break;
                case ImageProcessingFilters.GrayscaleY:
                    baseFilter = Grayscale.Y;
                    break;
                case ImageProcessingFilters.Channel_Red:
                    baseFilter = ExtractChannel.R;
                    break;
                case ImageProcessingFilters.Channel_Green:
                    baseFilter = ExtractChannel.G;
                    break;
                case ImageProcessingFilters.Channel_Blue:
                    baseFilter = ExtractChannel.B;
                    break;
                case ImageProcessingFilters.Channel_Alpha:
                    baseFilter = ExtractChannel.A;
                    break;
            }

            // DoTo

            return imageBytes;
        }

        #endregion
    }
}
