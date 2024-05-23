namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static public class InterpolationModeExtensions
    {
        static public System.Drawing.Drawing2D.InterpolationMode ToGdiInterpolationMode(this InterpolationMode mode)
        {
            switch (mode)
            {
                case InterpolationMode.Low:
                    return System.Drawing.Drawing2D.InterpolationMode.Low;
                case InterpolationMode.High:
                    return System.Drawing.Drawing2D.InterpolationMode.High;
                case InterpolationMode.Bicubic:
                    return System.Drawing.Drawing2D.InterpolationMode.Bicubic;
                case InterpolationMode.Bilinear:
                    return System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                case InterpolationMode.NearestNeighbor:
                    return System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                case InterpolationMode.HighQualityBicubic:
                    return System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                case InterpolationMode.HighQualityBilinear:
                    return System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                default:
                    return System.Drawing.Drawing2D.InterpolationMode.Default;
            }
        }

        static public InterpolationMode ToInterpolationMode(this System.Drawing.Drawing2D.InterpolationMode mode)
        {
            switch (mode)
            {
                case System.Drawing.Drawing2D.InterpolationMode.Low:
                    return InterpolationMode.Low;
                case System.Drawing.Drawing2D.InterpolationMode.High:
                    return InterpolationMode.High;
                case System.Drawing.Drawing2D.InterpolationMode.Bicubic:
                    return InterpolationMode.Bicubic;
                case System.Drawing.Drawing2D.InterpolationMode.Bilinear:
                    return InterpolationMode.Bilinear;
                case System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor:
                    return InterpolationMode.NearestNeighbor;
                case System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic:
                    return InterpolationMode.HighQualityBicubic;
                case System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear:
                    return InterpolationMode.HighQualityBilinear;
                default:
                    return InterpolationMode.Default;
            }
        }
    }
}
