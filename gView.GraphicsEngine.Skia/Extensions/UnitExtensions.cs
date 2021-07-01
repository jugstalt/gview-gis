using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class UnitExtensions
    {
        static public float PointsToPixels(this float points) => points * (96f / 72f);
        static public float PixelsToPoints(this float pixels) => pixels * (72f / 96f);
    }
}
