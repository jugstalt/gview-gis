using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System.Drawing;

namespace gView.GraphicsEngine.GdiPlus
{
    class GdiFont : IFont
    {
        private Font _font;

        public GdiFont(string fontFamily, float size, FontStyle fontStyle)
        {
            _font = new Font(fontFamily, size, fontStyle.ToGdiFontStyle());
        }

        public object EngineElement => _font;

        public string Name => _font.Name;

        public float Size => _font.Size;

        public FontStyle Style => (FontStyle)_font.Style;

        public GraphicsUnit Unit => (GraphicsUnit)_font.Unit;

        public object LockObject => null;

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
