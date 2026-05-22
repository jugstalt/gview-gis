using SkiaSharp;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class InterpolationModeMethods
    {
        //static public SKFilterQuality ToSKFilterQuality(this InterpolationMode interpolation)
        //{
        //    switch (interpolation)
        //    {
        //        case InterpolationMode.Low:
        //            return SKFilterQuality.Low;
        //        case InterpolationMode.NearestNeighbor:
        //            return SKFilterQuality.None;
        //        case InterpolationMode.Bilinear:
        //        case InterpolationMode.HighQualityBilinear:
        //            return SKFilterQuality.Medium;
        //        case InterpolationMode.Bicubic:
        //        case InterpolationMode.HighQualityBicubic:
        //            return SKFilterQuality.High;
        //    }

        //    return SKFilterQuality.None;
        //}

        static public SKSamplingOptions ToSKSamplingOpitons(this InterpolationMode interpolation)
            => interpolation switch
            {
                // none
                InterpolationMode.NearestNeighbor => new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None),
                // low
                InterpolationMode.Low => new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None),
                // medium
                InterpolationMode.Bilinear => new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear),
                InterpolationMode.HighQualityBilinear => new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear),
                // high
                InterpolationMode.Bicubic => new SKSamplingOptions(SKCubicResampler.Mitchell),
                InterpolationMode.HighQualityBicubic => new SKSamplingOptions(SKCubicResampler.Mitchell),

                _ => new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None)
            };
    }
}
