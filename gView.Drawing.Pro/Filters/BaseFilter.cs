using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace gView.Drawing.Pro.Filters
{
    public abstract class BaseFilter : IFilter
    {
        public Bitmap Apply(Bitmap bitmap)
        {
            BitmapData bmData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);

            Bitmap dstImage = null;

            try
            {
                dstImage = Apply(bmData);
                if ((bitmap.HorizontalResolution > 0) && (bitmap.VerticalResolution > 0))
                {
                    dstImage.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
                }
            }
            finally
            {
                bitmap.UnlockBits(bmData);
            }

            return dstImage;
        }

        public Bitmap Apply(BitmapData bmData)
        {
            int width = bmData.Width;
            int height = bmData.Height;

            PixelFormat dstPixelFormat = FormatTranslations[bmData.PixelFormat];

            // create new image of required format
            Bitmap dstImage = (dstPixelFormat == PixelFormat.Format8bppIndexed) ?
                ImageGlobals.CreateGrayscaleImage(width, height) :
                new Bitmap(width, height, dstPixelFormat);

            // lock destination bitmap data
            BitmapData dstData = dstImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, dstPixelFormat);

            try
            {
                ProcessFilter(bmData, dstData);
            }
            finally
            {
                // unlock destination images
                dstImage.UnlockBits(dstData);
            }

            return dstImage;
        }

        public abstract Dictionary<PixelFormat, PixelFormat> FormatTranslations { get; }

        protected abstract unsafe void ProcessFilter(BitmapData sourceData, BitmapData destinationData);

        #region Helpers

        #endregion
    }
}
