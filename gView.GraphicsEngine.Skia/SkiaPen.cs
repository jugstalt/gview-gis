using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Skia
{
    public class SkiaPen : IPen
    {
        private SKPaint _skPaint;

        public SkiaPen(ArgbColor color, float width)
        {
            _skPaint = new SKPaint()
            {
                Color = color.ToSKColor(),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = width,
                StrokeCap = SKStrokeCap.Round,
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

        public float Width
        {
            get
            {
                return _skPaint.StrokeWidth;
            }
            set
            {
                _skPaint.StrokeWidth = value;
            }
        }

        public LineDashStyle DashStyle 
        {
            get
            {
                return LineDashStyle.Solid;
            }
            set
            {

            }
        }

        public LineCap StartCap 
        {
            get
            {
                return LineCap.Round;
            }
            set
            {

            }
        }

        public LineCap EndCap
        {
            get
            {
                return LineCap.Round;
            }
            set
            {

            }
        }

        public LineJoin LineJoin
        {
            get
            {
                return LineJoin.Round;
            }
            set
            {

            }
        }

        public object EngineElement => _skPaint;

        public void Dispose()
        {
            _skPaint.Dispose();
        }
    }
}
