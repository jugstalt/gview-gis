using gView.GraphicsEngine.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Filters
{
    public abstract class BaseFilter : IFilter
    {
        public IBitmap Apply(IBitmap bitmap)
        {
            var bmData = bitmap.LockBitmapPixelData(
                BitmapLockMode.ReadOnly, bitmap.PixelFormat);

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

        #region Helpers

        #endregion
    }
}
