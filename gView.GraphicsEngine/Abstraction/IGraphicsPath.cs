using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IGraphicsPath : IDisposable
    {
        void StartFigure();
        void CloseFigure();

        void AddLine(float x1, float y1, float x2, float y2);

        void AddLine(CanvasPointF p1, CanvasPointF p2);

        CanvasRectangleF GetBounds();

        object EngineElement { get; }
    }
}
