using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System;
using System.Drawing.Drawing2D;

namespace gView.GraphicsEngine.GdiPlus
{
    internal class GdiLinearGradientBrush : IBrush
    {
        private LinearGradientBrush _brush;

        public GdiLinearGradientBrush(CanvasRectangleF rect, ArgbColor col1, ArgbColor col2, float angle)
        {
            _brush = new LinearGradientBrush(rect.ToGdiRectangleF(), col1.ToGdiColor(), col2.ToGdiColor(), angle);
        }

        public object EngineElement => _brush;

        public ArgbColor Color { get; set; }

        public void Dispose()
        {
            if (_brush != null)
            {
                _brush.Dispose();
                _brush = null;
            }
        }
    }
}