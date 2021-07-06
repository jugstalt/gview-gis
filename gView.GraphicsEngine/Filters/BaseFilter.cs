using gView.GraphicsEngine.Abstraction;
using System.Collections.Generic;
using System.IO;

namespace gView.GraphicsEngine.Filters
{
    public abstract class BaseFilter : IFilter
    {
        public IBitmap Apply(IBitmap bitmap)
        {
            var bmData = bitmap.LockBitmapPixelData(
                BitmapLockMode.ReadWrite, bitmap.PixelFormat);

            IBitmap dstImage = null;

            try
            {
                dstImage = Apply(bmData);
                if ((bitmap.DpiX > 0) && (bitmap.DpiY > 0))
                {
                    dstImage.SetResolution(bitmap.DpiX, bitmap.DpiY);
                }
            }
            finally
            {
                bitmap.UnlockBitmapPixelData(bmData);
            }

            return dstImage;
        }

        public IBitmap Apply(BitmapPixelData bmData)
        {
            int width = bmData.Width;
            int height = bmData.Height;

            PixelFormat dstPixelFormat = FormatTranslations[bmData.PixelFormat];

            // create new image of required format
            IBitmap dstImage = (dstPixelFormat == PixelFormat.Gray8) ?
                Helper.CreateGrayscaleImage(width, height) :
                GraphicsEngine.Current.Engine.CreateBitmap(width, height, dstPixelFormat);

            // lock destination bitmap data
            BitmapPixelData dstData = dstImage.LockBitmapPixelData(
                BitmapLockMode.ReadWrite, dstPixelFormat);

            try
            {
                ProcessFilter(bmData, dstData);
            }
            finally
            {
                // unlock destination images
                dstImage.UnlockBitmapPixelData(dstData);
            }

            return dstImage;
        }

        public abstract Dictionary<PixelFormat, PixelFormat> FormatTranslations { get; }

        protected abstract unsafe void ProcessFilter(BitmapPixelData sourceData, BitmapPixelData destinationData);

        #region Static Members

        public static IFilter GetFilter(FilterImplementations filterImplementation)
        {
            IFilter filter = null;

            switch (filterImplementation)
            {
                case FilterImplementations.Default:
                    return null;
                case FilterImplementations.GrayscaleBT709:
                    filter = Grayscale.BT709;
                    break;
                case FilterImplementations.GrayscaleRMY:
                    filter = Grayscale.RMY;
                    break;
                case FilterImplementations.GrayscaleY:
                    filter = Grayscale.Y;
                    break;
                case FilterImplementations.Channel_Red:
                    filter = ExtractChannel.R;
                    break;
                case FilterImplementations.Channel_Green:
                    filter = ExtractChannel.G;
                    break;
                case FilterImplementations.Channel_Blue:
                    filter = ExtractChannel.B;
                    break;
                case FilterImplementations.Channel_Alpha:
                    filter = ExtractChannel.A;
                    break;
            }

            return filter;
        }

        public static IBitmap ApplyFilter(IBitmap iBitmap, FilterImplementations filterImplementation)
        {
            IFilter filter = GetFilter(filterImplementation);

            if (filter != null && iBitmap != null)
            {
                return filter.Apply(iBitmap);
            }

            return iBitmap;
        }

        public static byte[] ApplyFilter(byte[] imageBytes, FilterImplementations filterImplementation, ImageFormat format)
        {
            IFilter filter = GetFilter(filterImplementation);
            if (filter == null)
            {
                return imageBytes;
            }

            using (var iBitmap = Current.Engine.CreateBitmap(new MemoryStream(imageBytes)))
            {
                filter.Apply(iBitmap);
                using(var ms = new MemoryStream())
                {
                    iBitmap.Save(ms, format);
                    return ms.ToArray();
                }
            }

        }

        #endregion
    }
}
