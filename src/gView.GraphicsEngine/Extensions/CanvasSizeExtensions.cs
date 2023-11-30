using gView.GraphicsEngine.Abstraction;

namespace gView.GraphicsEngine.Extensions
{
    public static class CanvasSizeExtensions
    {
        public static CanvasSizeF AddPadding(this CanvasSizeF sizeF, IFont font)
        {
            if (font == null)
            {
                return sizeF;
            }

            if (Current.Engine.MeasuresTextWithPadding == false)
            {
                var fontPixelSize = font.Size;

                switch (font.Unit)
                {
                    case GraphicsUnit.Point:
                        fontPixelSize *= Current.Engine.ScreenDpi / 72f;
                        break;
                }

                sizeF.Width += fontPixelSize * 0.4f;
                sizeF.Height += fontPixelSize * 0.2f;
            }

            return sizeF;
        }

        public static CanvasRectangleF AddOffsetPadding(this CanvasRectangleF rectF, IFont font, IDrawTextFormat format)
        {
            if (font == null)
            {
                return rectF;
            }

            if (Current.Engine.MeasuresTextWithPadding == false)
            {
                var fontPixelSize = font.Size;

                switch (font.Unit)
                {
                    case GraphicsUnit.Point:
                        fontPixelSize *= Current.Engine.ScreenDpi / 72f;
                        break;
                }

                switch (format.Alignment)
                {
                    case StringAlignment.Near:
                        rectF.Offset(fontPixelSize * 0.02f, 0f);
                        break;

                    case StringAlignment.Far:
                        rectF.Offset(fontPixelSize * 0.2f, 0f);
                        break;
                }
                switch (format.LineAlignment)
                {
                    case StringAlignment.Near:
                        rectF.Offset(0f, fontPixelSize * 0.19f);
                        break;

                    case StringAlignment.Far:
                        rectF.Offset(0f, -fontPixelSize * 0.19f);
                        break;
                }
            }

            return rectF;
        }
    }
}
