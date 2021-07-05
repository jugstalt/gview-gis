using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Filters
{
    public class ExtractChannel : BaseFilter
    {
        public static readonly ExtractChannel R = new ExtractChannel(RGB.R);
        public static readonly ExtractChannel G = new ExtractChannel(RGB.G);
        public static readonly ExtractChannel B = new ExtractChannel(RGB.B);
        public static readonly ExtractChannel A = new ExtractChannel(RGB.A);

        private short channel = RGB.R;

        private Dictionary<PixelFormat, PixelFormat> _formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
        {
            get { return _formatTranslations; }
        }

        public short Channel
        {
            get { return channel; }
            set
            {
                if (
                    (value != RGB.R) && (value != RGB.G) &&
                    (value != RGB.B) && (value != RGB.A)
                    )
                {
                    throw new ArgumentException("Invalid channel is specified.");
                }
                channel = value;
            }
        }

        public ExtractChannel()
        {
            // initialize format translation dictionary
            _formatTranslations[PixelFormat.Rgb24] = PixelFormat.Gray8;
            _formatTranslations[PixelFormat.Rgb32] = PixelFormat.Gray8;
            _formatTranslations[PixelFormat.Rgba32] = PixelFormat.Gray8;
        }

        public ExtractChannel(short channel) : this()
        {
            this.Channel = channel;
        }

        protected override unsafe void ProcessFilter(BitmapPixelData sourceData, BitmapPixelData destinationData)
        {
            // get width and height
            int width = sourceData.Width;
            int height = sourceData.Height;

            int pixelSize = Helper.GetPixelFormatSize(sourceData.PixelFormat) / 8;

            if ((channel == RGB.A) && (pixelSize != 4) && (pixelSize != 8))
            {
                throw new Exception("Can not extract alpha channel from none ARGB image.");
            }

            if (pixelSize <= 4)
            {
                int srcOffset = sourceData.Stride - width * pixelSize;
                int dstOffset = destinationData.Stride - width;

                // do the job
                byte* src = (byte*)sourceData.Scan0.ToPointer();
                byte* dst = (byte*)destinationData.Scan0.ToPointer();

                // allign source pointer to the required channel
                src += channel;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, src += pixelSize, dst++)
                    {
                        *dst = *src;
                    }
                    src += srcOffset;
                    dst += dstOffset;
                }
            }
            else
            {
                pixelSize /= 2;

                byte* srcBase = (byte*)sourceData.Scan0.ToPointer();
                byte* dstBase = (byte*)destinationData.Scan0.ToPointer();
                int srcStride = sourceData.Stride;
                int dstStride = destinationData.Stride;

                // for each line
                for (int y = 0; y < height; y++)
                {
                    ushort* src = (ushort*)(srcBase + y * srcStride);
                    ushort* dst = (ushort*)(dstBase + y * dstStride);

                    // allign source pointer to the required channel
                    src += channel;

                    // for each pixel
                    for (int x = 0; x < width; x++, src += pixelSize, dst++)
                    {
                        *dst = *src;
                    }
                }

            }
        }
    }
}
