using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace gView.Framework.Symbology
{
    class DisplayCharacterRanges : IDisplayCharacterRanges
    {
        private RectangleF[] _rectF;

        public DisplayCharacterRanges(System.Drawing.Graphics gr, System.Drawing.Font font, StringFormat format, string text)
        {
            _rectF = new RectangleF[text.Length];
            int c = 0;
            float xOffset = 0;
            for (int l = 0; l < text.Length; l += 32, c += 32)
            {
                string t = text.Substring(l, Math.Min(32, text.Length - l));

                CharacterRange[] ranges = new CharacterRange[t.Length];
                for (int i = 0; i < t.Length; i++)
                    ranges[i] = new CharacterRange(i, 1);

                format.SetMeasurableCharacterRanges(ranges);

                SizeF size = gr.MeasureString(t, font);
                Region[] regions = gr.MeasureCharacterRanges(t, font, new RectangleF(0, 0, size.Width, size.Height), format);

                for (int i = 0; i < regions.Length; i++)
                {
                    _rectF[c + i] = regions[i].GetBounds(gr);
                    _rectF[c + i].X += xOffset;
                    regions[i].Dispose();
                }

                xOffset = _rectF[c + t.Length - 1].X + _rectF[c + t.Length - 1].Width;
            }
        }

        public RectangleF this[int i]
        {
            get { return _rectF[i]; }
        }

        public float Width
        {
            get
            {
                float w = 0f;
                for (int i = 0; i < _rectF.Length; i++)
                    w += _rectF[i].Width;
                return w;
            }
        }
    }
}
