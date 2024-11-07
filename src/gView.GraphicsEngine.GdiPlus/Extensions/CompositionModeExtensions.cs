namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class CompositionModeExtensions
    {
        static public System.Drawing.Drawing2D.CompositingMode ToGdiCompositionMode(this CompositingMode mode)
        {
            switch (mode)
            {
                case CompositingMode.SourceCopy:
                    return System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                default:
                    return System.Drawing.Drawing2D.CompositingMode.SourceOver;
            }
        }
    }
}
