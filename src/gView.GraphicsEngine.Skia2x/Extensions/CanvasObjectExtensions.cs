using SkiaSharp;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class CanvasObjectExtensions
    {
        static public SKPoint ToSKPoint(this CanvasPoint point)
        {
            return new SKPoint(point.X, point.Y);
        }

        static public SKPoint ToSKPoint(this CanvasPointF point)
        {
            return new SKPoint(point.X, point.Y);
        }

        static public CanvasPoint ToCanvasPoint(this SKPoint point)
        {
            return new CanvasPoint((int)point.X, (int)point.Y);
        }

        static public CanvasPointF ToCanvasPointF(this SKPoint point)
        {
            return new CanvasPointF(point.X, point.Y);
        }

        static public SKRect ToSKRect(this CanvasRectangle rect)
        {
            return new SKRect(rect.Left, rect.Top, rect.Left + rect.Width, rect.Top + rect.Height);
        }

        static public SKRect ToSKRect(this CanvasRectangleF rect)
        {
            return new SKRect(rect.Left, rect.Top, rect.Left + rect.Width, rect.Top + rect.Height);
        }

        static public CanvasRectangle ToCanvasRectangle(this SKRect rect)
        {
            return new CanvasRectangle((int)rect.Left, (int)rect.Top, (int)(rect.Right - rect.Left), (int)(rect.Bottom - rect.Top));
        }

        static public CanvasRectangleF ToCanvasRectangleF(this SKRect rect)
        {
            return new CanvasRectangleF(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }
    }
}
