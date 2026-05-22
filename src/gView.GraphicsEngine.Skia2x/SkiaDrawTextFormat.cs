using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;

namespace gView.GraphicsEngine.Skia
{
    class SkiaDrawTextFormat : IDrawTextFormat
    {
        private SKPaint _skPaint;

        public SkiaDrawTextFormat()
        {
            _skPaint = new SKPaint()
            {
                TextAlign = SKTextAlign.Left
            };
            this.LineAlignment = StringAlignment.Near;
        }

        public StringAlignment Alignment
        {
            get
            {
                return _skPaint.TextAlign.ToStringAlignment();
            }
            set
            {
                _skPaint.TextAlign = value.ToSkTextAlign();
            }
        }
        public StringAlignment LineAlignment
        {
            get; set;
        }

        public object EngineElement => _skPaint;
    }
}
