using System;

namespace gView.GraphicsEngine.Abstraction
{
    public interface ICanvas : IDisposable
    {
        float DpiX { get; }

        float DpiY { get; }

        CompositingMode CompositingMode { set; }

        void DrawRectangle(IPen pen, CanvasRectangle rectangle);
        void DrawRectangle(IPen pen, CanvasRectangleF rectangleF);

        void FillRectangle(IBrush brush, CanvasRectangle rectangle);
        void FillRectangle(IBrush brush, CanvasRectangleF rectangleF);

        void DrawBitmap(IBitmap bitmap, CanvasPoint point);
        void DrawBitmap(IBitmap bitmap, CanvasPointF pointF);
        void DrawBitmap(IBitmap bitmap, CanvasRectangle dest, CanvasRectangle source, float opacity=1.0f);
        void DrawBitmap(IBitmap bitmap, CanvasRectangleF dest, CanvasRectangleF source);
        void DrawBitmap(IBitmap bitmap, CanvasPointF[] points, CanvasRectangleF source, float opacity = 1.0f);

        void DrawText(string text, IFont font, IBrush brush, CanvasPoint point);
        void DrawText(string text, IFont font, IBrush brush, int x, int y);
        void DrawText(string text, IFont font, IBrush brush, CanvasPointF pointF);
        void DrawText(string text, IFont font, IBrush brush, float x, float y);

        void DrawLine(IPen pen, CanvasPoint p1, CanvasPoint p2);
        void DrawLine(IPen pen, CanvasPointF p1, CanvasPointF p2);
        void DrawLine(IPen pen, int x1, int y1, int x2, int y2);
        void DrawLine(IPen pen, float x1, float y1, float x2, float y2);

        CanvasSizeF MeasureText(string text, IFont font);
    }
}
