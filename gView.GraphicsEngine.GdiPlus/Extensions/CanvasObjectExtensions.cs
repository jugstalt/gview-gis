using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class CanvasObjectExtensions
    {
        static public Point ToGdiPoint(this CanvasPoint canvasPoint)
        {
            return new Point(canvasPoint.X, canvasPoint.Y);
        }

        static public PointF ToGdiPointF(this CanvasPointF canvasPoint)
        {
            return new PointF(canvasPoint.X, canvasPoint.Y);
        }

        static public PointF[] ToGdiPointFArray(this IEnumerable<CanvasPointF> canvasPoints)
        {
            return canvasPoints?.Select(p => p.ToGdiPointF())
                                .ToArray();
        }

        static public Rectangle ToGdiRectangle(this CanvasRectangle canvasRectangle)
        {
            return new Rectangle(
                canvasRectangle.Left,
                canvasRectangle.Top,
                canvasRectangle.Width,
                canvasRectangle.Height);
        }

        static public RectangleF ToGdiRectangleF(this CanvasRectangleF canvasRectangleF)
        {
            return new RectangleF(
                canvasRectangleF.Left,
                canvasRectangleF.Top,
                canvasRectangleF.Width,
                canvasRectangleF.Height);
        }

        static public Size ToGdiSize(this CanvasSize canvasSize)
        {
            return new Size(canvasSize.Width, canvasSize.Height);
        }

        static public SizeF ToGdiSizeF(this CanvasSize canvasSizeF)
        {
            return new SizeF(canvasSizeF.Width, canvasSizeF.Height);
        }
    }
}
