using gView.GraphicsEngine.Abstraction;
using System;
using System.Drawing;

namespace gView.GraphicsEngine.GdiPlus
{
    class DisplayCharacterRanges : IDisplayCharacterRanges
    {
        private CanvasRectangleF[] _rectF;

        public DisplayCharacterRanges(Graphics graphics, System.Drawing.Font font, StringFormat format, string text)
        {
            _rectF = new CanvasRectangleF[text.Length];
            int c = 0;
            float xOffset = 0;

            for (int l = 0; l < text.Length; l += 32, c += 32)
            {
                string t = text.Substring(l, Math.Min(32, text.Length - l));

                CharacterRange[] ranges = new CharacterRange[t.Length];
                for (int i = 0; i < t.Length; i++)
                {
                    ranges[i] = new CharacterRange(i, 1);
                }

                format.SetMeasurableCharacterRanges(ranges);

                SizeF size = graphics.MeasureString(t, font);
                Region[] regions = graphics.MeasureCharacterRanges(t, font, new RectangleF(0, 0, size.Width, size.Height), format);

                for (int i = 0; i < regions.Length; i++)
                {
                    var bounds = regions[i].GetBounds(graphics);
                    _rectF[c + i] = new CanvasRectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    _rectF[c + i].Left += xOffset;
                    regions[i].Dispose();
                }

                xOffset = _rectF[c + t.Length - 1].Left + _rectF[c + t.Length - 1].Width;
            }
        }

        public CanvasRectangleF this[int i]
        {
            get { return _rectF[i]; }
        }

        public float Width
        {
            get
            {
                float w = 0f;
                for (int i = 0; i < _rectF.Length; i++)
                {
                    w += _rectF[i].Width;
                }

                return w;
            }
        }
    }
}
