using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Skia
{
    class SkiaSolidBrush : IBrush
    {
        private SKPaint _skPaint;

        public SkiaSolidBrush(ArgbColor color)
        {
            _skPaint = new SKPaint()
            {
                ColorF = color.ToSKColor(),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        public ArgbColor Color
        {
            get
            {
                return _skPaint.Color.ToArgbColor();
            }
            set
            {
                _skPaint.Color = value.ToSKColor();
            }
        }

        public object EngineElement => _skPaint;

        public void Dispose()
        {
            _skPaint.Dispose();
        }
    }
}
