using System.Drawing;

namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class ColorExtensions
    {
        static public Color ToGdiColor(this ArgbColor color)
        {
            return Color.FromArgb(color.ToArgb());
        }

        static public ArgbColor ToArgbColor(this Color color)
        {
            return ArgbColor.FromArgb(color.ToArgb());
        }
    }
}
