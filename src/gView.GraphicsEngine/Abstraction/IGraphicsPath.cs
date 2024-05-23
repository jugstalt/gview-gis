using System;
using System.Security.Cryptography;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IGraphicsPath : IDisposable
    {
        void StartFigure();
        void CloseFigure();

        void AddLine(float x1, float y1, float x2, float y2);

        void AddLine(CanvasPointF p1, CanvasPointF p2);

        void AddPoint(float x1, float y1);
        void AddPoint(CanvasPoint p1);

        void AddEllipse(CanvasRectangleF rect);

        CanvasRectangleF GetBounds();

        GraphicsPathBuildPerferences PathBuildPerferences { get; }

        object EngineElement { get; }
    }
}
