using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class CanvasObjectExtensions
    {
        static public Point ToGdiPoint(this CanvasPoint canvasPoint)
        {
            return new Point(
                canvasPoint?.X ?? 0,
                canvasPoint?.Y ?? 0);
        }

        static public PointF ToGdiPointF(this CanvasPointF canvasPoint)
        {
            return new PointF(
                canvasPoint?.X ?? 0f,
                canvasPoint?.Y ?? 0f);
        }

        static public PointF[] ToGdiPointFArray(this CanvasPointF[] canvasPoints)
        {
            return canvasPoints.Select(p => p.ToGdiPointF())
                               .ToArray();
        }

        static public Rectangle ToGdiRectangle(this CanvasRectangle canvasRectangle)
        {
            return new Rectangle(
                canvasRectangle?.Left ?? 0,
                canvasRectangle?.Top ?? 0,
                canvasRectangle?.Width ?? 0,
                canvasRectangle?.Height ?? 0);
        }

        static public RectangleF ToGdiRectangleF(this CanvasRectangleF canvasRectangleF)
        {
            return new RectangleF(
                canvasRectangleF?.Left ?? 0f,
                canvasRectangleF?.Top ?? 0f,
                canvasRectangleF?.Width ?? 0f,
                canvasRectangleF?.Height ?? 0f);
        }

        static public Size ToGdiSize(this CanvasSize canvasSize)
        {
            return new Size(
                canvasSize?.Width ?? 0,
                canvasSize?.Height ?? 0);
        }

        static public SizeF ToGdiSizeF(this CanvasSize canvasSizeF)
        {
            return new SizeF(
                canvasSizeF?.Width ?? 0f,
                canvasSizeF?.Height ?? 0f);
        }
    }
}
