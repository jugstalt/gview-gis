using System.Collections.Generic;

namespace gView.GraphicsEngine.Filters
{
    class EraseColorFilter : BaseFilter
    {
        private readonly ArgbColor _eraseColor;
        private readonly ArgbColor _transparentColor;

        public EraseColorFilter(ArgbColor eraseColor, ArgbColor? transparentColor = null)
        {
            _eraseColor = eraseColor;
            _transparentColor = transparentColor ?? ArgbColor.Transparent;

            _formatTranslations[PixelFormat.Rgb24] = PixelFormat.Rgba32;
            _formatTranslations[PixelFormat.Rgb32] = PixelFormat.Rgba32;
            _formatTranslations[PixelFormat.Rgba32] = PixelFormat.Rgba32;
        }

        private Dictionary<PixelFormat, PixelFormat> _formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

        public override bool ShouldApplied => !_eraseColor.Equals(_transparentColor);

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
            PixelFormat dstPixelFormat = destinationData.PixelFormat;

            if (
                    ((srcPixelFormat == PixelFormat.Rgb24) ||
                     (srcPixelFormat == PixelFormat.Rgb32) ||
                     (srcPixelFormat == PixelFormat.Rgba32)) 
                    && 
                    ((dstPixelFormat == PixelFormat.Rgb24) ||
                     (dstPixelFormat == PixelFormat.Rgb32) ||
                     (dstPixelFormat == PixelFormat.Rgba32))
                )
            {
                int srcPixelSize = (srcPixelFormat == PixelFormat.Rgb24) ? 3 : 4;
                int dstPixelSize = (dstPixelFormat == PixelFormat.Rgb24) ? 3 : 4;
                int srcOffset = sourceData.Stride - width * srcPixelSize;
                int dstOffset = destinationData.Stride - width * dstPixelSize;

                // do the job
                byte* src = (byte*)sourceData.Scan0.ToPointer();
                byte* dst = (byte*)destinationData.Scan0.ToPointer();

                // for each line
                for (int y = 0; y < height; y++)
                {
                    // for each pixel
                    for (int x = 0; x < width; x++, src += srcPixelSize, dst += dstPixelSize)
                    {
                        var srcCol = ArgbColor.FromArgb(
                                srcPixelSize == 4 ? src[RGB.A] : 255,
                                src[RGB.R], src[RGB.G], src[RGB.B]
                            );

                        if (srcCol.Equals(_eraseColor))
                        {
                            if(dstPixelSize == 4) dst[RGB.A] = _transparentColor.A;
                            dst[RGB.R] = _transparentColor.R;
                            dst[RGB.G] = _transparentColor.G;
                            dst[RGB.B] = _transparentColor.B;
                        }
                        else
                        {
                            if (dstPixelSize == 4) dst[RGB.A] = srcCol.A;
                            dst[RGB.R] = srcCol.R;
                            dst[RGB.G] = srcCol.G;
                            dst[RGB.B] = srcCol.B;
                        }
                    }

                    src += srcOffset;
                    dst += dstOffset;
                }
            }
        }
    }
}
