using System.Drawing;

namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class StringFormatExtensions
    {
        public static StringFormat ToStringFormat(this DrawTextFormat format)
        {
            return new StringFormat()
            {
                Alignment = (System.Drawing.StringAlignment)format.Alignment,
                LineAlignment = (System.Drawing.StringAlignment)format.LineAlignment,
                FormatFlags = System.Drawing.StringFormatFlags.DirectionRightToLeft
            };
        }
    }
}
