using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;

namespace gView.GraphicsEngine.Skia
{
    class SkiaGraphicsPath : IGraphicsPath
    {
        private SKPath _path;
        private bool _startFigure = true;

        public SkiaGraphicsPath()
        {
            _path = new SKPath()
            {
                FillType = SKPathFillType.EvenOdd
            };
        }

        public object EngineElement => _path;

        public GraphicsPathBuildPerferences PathBuildPerferences => GraphicsPathBuildPerferences.AddPointsPreferred;

        public void AddLine(float x1, float y1, float x2, float y2)
        {
            if (_startFigure == true)
            {
                _startFigure = false;
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
                _startFigure = false;
                _path.MoveTo(p1.ToSKPoint());
                _path.LineTo(p2.ToSKPoint());
            }
            else
            {
                _path.LineTo(p1.ToSKPoint());
                _path.LineTo(p2.ToSKPoint());
            }
        }

        public void AddPoint(float x, float y)
        {
            if (_startFigure == true)
            {
                _startFigure = false;
                _path.MoveTo(x, y);
            }
            else
            {
                _path.LineTo(x, y);
            }
        }

        public void AddPoint(CanvasPoint p)
        {
            if (_path.PointCount == 0)
            {
                _startFigure = false;
                _path.MoveTo(p.ToSKPoint());
            }
            else
            {
                _path.LineTo(p.ToSKPoint());
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
            SKRect rect;
            _path.GetBounds(out rect);

            return rect.ToCanvasRectangleF();
        }

        public void StartFigure()
        {
            _startFigure = true;
        }
    }
}
