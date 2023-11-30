using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;
using System;

namespace gView.GraphicsEngine.Skia
{
    public class SkiaPen : IPen
    {
        private SKPaint _skPaint;
        private LineDashStyle _dashStyle;
        private LineCap _startCap, _endCap;

        public SkiaPen(ArgbColor color, float width)
        {
            _skPaint = new SKPaint()
            {
                Color = color.ToSKColor(),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = Math.Max(0.8f, width),
                StrokeJoin = SKStrokeJoin.Round
            };

            this.DashStyle = LineDashStyle.Solid;
            this.StartCap = this.EndCap = LineCap.Round;
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

                this.DashStyle = _dashStyle;   // restet DashPickerArray
            }
        }

        public LineDashStyle DashStyle
        {
            get
            {
                return _dashStyle;
            }
            set
            {
                _dashStyle = value;

                var pickerArray = _dashStyle.ToPickerArray(this.Width, this.StartCap);
                if (pickerArray != null)
                {
                    _skPaint.PathEffect = SKPathEffect.CreateDash(pickerArray, 0f);
                }
            }
        }

        public LineCap StartCap
        {
            get
            {
                return _startCap;
            }
            set
            {
                _startCap = _endCap = value;
                _skPaint.StrokeCap = value.ToSKStrokeCap();

                this.DashStyle = _dashStyle;   // restet DashPickerArray
            }
        }

        public LineCap EndCap
        {
            get
            {
                return _endCap;
            }
            set
            {
                _startCap = _endCap = value;
                _skPaint.StrokeCap = value.ToSKStrokeCap();

                this.DashStyle = _dashStyle;   // restet DashPickerArray
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
