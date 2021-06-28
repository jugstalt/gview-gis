using gView.GraphicsEngine.Abstraction;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace gView.GraphicsEngine.GdiPlus
{
    class GdiFont : IFont
    {
        private Font _font;

        public GdiFont(string fontFamily, float size, FontStyle fontStyle)
        {
            _font = new Font(fontFamily, size);
        }

        public object EngineElement => _font;

        public string Name => _font.Name;

        public float Size => _font.Size;

        public FontStyle Style => (FontStyle)_font.Style;

        public GraphicsUnit Unit => (GraphicsUnit)_font.Unit;

        public void Dispose()
        {
            if (_font != null)
            {
                _font.Dispose();
                _font = null;
            }
        }
    }
}
