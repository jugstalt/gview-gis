using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using gView.GraphicsEngine.Threading;
using System.Drawing;

namespace gView.GraphicsEngine.GdiPlus
{
    class GdiFont : IFont
    {
        private Font _font;

        public GdiFont(string fontFamily, float size, FontStyle fontStyle)
        {
            var gdiStyle = fontStyle.ToGdiFontStyle();

            // Prefer a font registered from a configured font directory
            // (PrivateFontCollection), which "new Font(name, ...)" would not find.
            var privateFamily = GdiGraphicsEngine.TryGetPrivateFontFamily(fontFamily);
            if (privateFamily != null)
            {
                if (!privateFamily.IsStyleAvailable(gdiStyle))
                {
                    gdiStyle = FirstAvailableStyle(privateFamily, gdiStyle);
                }

                _font = new Font(privateFamily, size, gdiStyle);
            }
            else
            {
                _font = new Font(fontFamily, size, gdiStyle);
            }
        }

        private static System.Drawing.FontStyle FirstAvailableStyle(FontFamily family, System.Drawing.FontStyle requested)
        {
            // keep decorations the caller asked for, drop weight/slant the file lacks
            var decorations = requested & (System.Drawing.FontStyle.Underline | System.Drawing.FontStyle.Strikeout);

            foreach (var candidate in new[]
                     {
                         requested & ~decorations,
                         System.Drawing.FontStyle.Regular,
                         System.Drawing.FontStyle.Bold,
                         System.Drawing.FontStyle.Italic,
                         System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic
                     })
            {
                if (family.IsStyleAvailable(candidate | decorations))
                {
                    return candidate | decorations;
                }
            }

            return requested;
        }

        public object EngineElement => _font;

        public string Name => _font.Name;

        public float Size => _font.Size;

        public FontStyle Style => (FontStyle)_font.Style;

        public GraphicsUnit Unit => (GraphicsUnit)_font.Unit;

        public IThreadLocker LockObject => null;

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
