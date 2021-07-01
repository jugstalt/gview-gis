using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Skia
{
    class SkiaGraphicsPath : IGraphicsPath
    {
        private SKPath _path;

        public SkiaGraphicsPath()
        {
            _path = new SKPath();
        }

        public object EngineElement => _path;

        public void AddLine(float x1, float y1, float x2, float y2)
        {
            if (_path.PointCount == 0)
            {
                _path.MoveTo(x1, y1);
                _path.LineTo(x2, y2);
            }
            else
            {
                _path.LineTo(x1, y1);
                _path.LineTo(x2, y2);
            }
        }

        public void AddLine(CanvasPointF p1, CanvasPointF p2)
        {
            if (_path.PointCount == 0)
            {
                _path.MoveTo(p1.ToSKPoint());
                _path.LineTo(p2.ToSKPoint());
            }
            else
            {
                _path.LineTo(p1.ToSKPoint());
                _path.LineTo(p2.ToSKPoint());
            }
        }

        public void CloseFigure()
        {
            _path.Close();
        }

        public void Dispose()
        {
            _path.Dispose();
        }

        public CanvasRectangleF GetBounds()
        {
            return _path.GetRoundRect().Rect.ToCanvasRectangleF();
        }

        public void StartFigure()
        {
            
        }
    }
}
