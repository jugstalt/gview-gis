using SkiaSharp;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class ColorExtensions
    {
        static public SKColor ToSKColor(this ArgbColor color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        static public ArgbColor ToArgbColor(this SKColor color)
        {
            return ArgbColor.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        }
    }
}
