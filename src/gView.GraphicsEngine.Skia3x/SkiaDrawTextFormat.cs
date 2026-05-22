using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia3x.Extensions;
using SkiaSharp;

namespace gView.GraphicsEngine.Skia3x
{
    class SkiaDrawTextFormat : IDrawTextFormat
    {
        private SKTextAlign _skTextAlign;

        public SkiaDrawTextFormat()
        {
            _skTextAlign = SKTextAlign.Left;
            
            this.LineAlignment = StringAlignment.Near;
        }

        public StringAlignment Alignment
        {
            get
            {
                return _skTextAlign.ToStringAlignment();
            }
            set
            {
                _skTextAlign = value.ToSkTextAlign();
            }
        }
        public StringAlignment LineAlignment
        {
            get; set;
        }

        public object EngineElement => _skTextAlign;
    }
}
