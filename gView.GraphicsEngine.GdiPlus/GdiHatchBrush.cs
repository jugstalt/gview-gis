using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System.Drawing.Drawing2D;

namespace gView.GraphicsEngine.GdiPlus
{
    internal class GdiHatchBrush : IBrush
    {
        private HatchBrush _brush;

        public GdiHatchBrush(HatchStyle hatchStyle, ArgbColor foreColor, ArgbColor backColor)
        {
            _brush = new HatchBrush((System.Drawing.Drawing2D.HatchStyle)hatchStyle,
                                    foreColor.ToGdiColor(),
                                    backColor.ToGdiColor());
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