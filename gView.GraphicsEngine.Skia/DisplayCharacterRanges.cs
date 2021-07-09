using gView.GraphicsEngine.Abstraction;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.GraphicsEngine.Skia
{
    class DisplayCharacterRanges : IDisplayCharacterRanges
    {
        private CanvasRectangleF[] _rectF;

        public DisplayCharacterRanges(SKCanvas canvas, SKPaint font, IDrawTextFormat format, string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                _rectF = new CanvasRectangleF[0];
                return;
            }

            _rectF = new CanvasRectangleF[text.Length];

            var rect = new SKRect();
            font.MeasureText("_", ref rect);
            float spaceWidth = rect.Width;

            float xOffset = 0, height = 0; ;
            for (int i = 0, to = text.Length; i < to; i++)
            {
                font.MeasureText($"{ text[i] }_", ref rect);

                _rectF[i].Left = rect.Left + xOffset;
                _rectF[i].Top = rect.Top;
                if (text[i] == ' ')
                {
                    _rectF[i].Width = rect.Width;
                }
                else
                {
                    _rectF[i].Width = rect.Width - spaceWidth * 0.9f;
                }
                height = Math.Max(rect.Height, height);

                xOffset += rect.Width;
            }

            //var far = font.FontMetrics != null ? -font.FontMetrics.Bottom : 0f;
            //var near = font.FontMetrics != null ? -font.FontMetrics.Top : 0f;
            //var center = (far + near) * .5f;

            var empiricValue = 1f;
            switch (format.LineAlignment)
            {
                case StringAlignment.Near:
                case StringAlignment.Far:
                    empiricValue = 1.5f;  // to generate more vartical line offset   
                    break;
            }

            for (int i = 0; i < _rectF.Length; i++)
            {
                //switch (format.LineAlignment)
                //{
                //    case StringAlignment.Near:
                //        _rectF[i].Top += near;
                //        break;
                //    case StringAlignment.Far:
                //        _rectF[i].Top += far;
                //        break;
                //    default:
                //        _rectF[i].Top += center;
                //        break;
                //}
                _rectF[i].Height = height * empiricValue;
            }
        }

        public CanvasRectangleF this[int i] => _rectF[i];

        public float Width => _rectF == null ? 0 : _rectF.Select(r => r.Width).Sum();
    }
}
