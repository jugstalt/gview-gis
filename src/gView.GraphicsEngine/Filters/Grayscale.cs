using System.Collections.Generic;

namespace gView.GraphicsEngine.Filters
{
    class Grayscale : BaseFilter
    {
        public static readonly Grayscale BT709 = new Grayscale(0.2125, 0.7154, 0.0721);
        public static readonly Grayscale RMY = new Grayscale(0.5000, 0.4190, 0.0810);
        public static readonly Grayscale Y = new Grayscale(0.2990, 0.5870, 0.1140);

        public readonly double RedCoefficient;
        public readonly double GreenCoefficient;
        public readonly double BlueCoefficient;

        private Dictionary<PixelFormat, PixelFormat> _formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

        public Grayscale(double cr, double cg, double cb)
        {
            RedCoefficient = cr;
            GreenCoefficient = cg;
            BlueCoefficient = cb;

            _formatTranslations[PixelFormat.Rgb24] = PixelFormat.Gray8;
            _formatTranslations[PixelFormat.Rgb32] = PixelFormat.Gray8;
            _formatTranslations[PixelFormat.Rgba32] = PixelFormat.Gray8;
            //_formatTranslations[PixelFormat.Format48bppRgb] = PixelFormat.Format16bppGrayScale;
            //_formatTranslations[PixelFormat.Format64bppArgb] = PixelFormat.Format16bppGrayScale;
        }

        public override bool ShouldApplied => true;

        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
        {
            get { return _formatTranslations; }
        }

        protected override unsafe void ProcessFilter(BitmapPixelData sourceData, BitmapPixelData destinationData)
        {
            // get width and height
            int width = sourceData.Width;
            int height = sourceData.Height;
            PixelFormat srcPixelFormat = sourceData.PixelFormat;

            if (
                (srcPixelFormat == PixelFormat.Rgb24) ||
                (srcPixelFormat == PixelFormat.Rgb32) ||
                (srcPixelFormat == PixelFormat.Rgba32))
            {
                int pixelSize = (srcPixelFormat == PixelFormat.Rgb24) ? 3 : 4;
                int srcOffset = sourceData.Stride - width * pixelSize;
                int dstOffset = destinationData.Stride - width;

                int rc = (int)(0x10000 * RedCoefficient);
                int gc = (int)(0x10000 * GreenCoefficient);
                int bc = (int)(0x10000 * BlueCoefficient);

                // make sure sum of coefficients equals to 0x10000
                while (rc + gc + bc < 0x10000)
                {
                    bc++;
                }

                // do the job
                byte* src = (byte*)sourceData.Scan0.ToPointer();
                byte* dst = (byte*)destinationData.Scan0.ToPointer();

                // for each line
                for (int y = 0; y < height; y++)
                {
                    // for each pixel
                    for (int x = 0; x < width; x++, src += pixelSize, dst++)
                    {
                        *dst = (byte)((rc * src[RGB.R] + gc * src[RGB.G] + bc * src[RGB.B]) >> 16);
                    }
                    src += srcOffset;
                    dst += dstOffset;
                }
            }
        }
    }
}
