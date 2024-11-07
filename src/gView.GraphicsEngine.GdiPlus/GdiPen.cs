using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System.Drawing;

namespace gView.GraphicsEngine.GdiPlus
{
    class GdiPen : IPen
    {
        private Pen _pen;

        public GdiPen(ArgbColor color, float width)
        {
            _pen = new Pen(color.ToGdiColor(), width);
        }

        public object EngineElement => _pen;

        public ArgbColor Color
        {
            get
            {
                return _pen.Color.ToArgbColor();
            }
            set
            {
                _pen.Color = value.ToGdiColor();
            }
        }
        public float Width
        {
            get { return _pen.Width; }
            set { _pen.Width = value; }
        }
        public LineDashStyle DashStyle
        {
            get { return (LineDashStyle)_pen.DashStyle; }
            set { _pen.DashStyle = (System.Drawing.Drawing2D.DashStyle)value; }
        }
        public LineCap StartCap
        {
            get { return (LineCap)_pen.StartCap; }
            set { _pen.StartCap = (System.Drawing.Drawing2D.LineCap)value; }
        }
        public LineCap EndCap
        {
            get { return (LineCap)_pen.EndCap; }
            set { _pen.EndCap = (System.Drawing.Drawing2D.LineCap)value; }
        }
        public LineJoin LineJoin
        {
            get { return (LineJoin)_pen.LineJoin; }
            set { _pen.LineJoin = (System.Drawing.Drawing2D.LineJoin)value; }
        }

        public void Dispose()
        {
            if (_pen != null)
            {
                _pen.Dispose();
                _pen = null;
            }
        }
    }
}
