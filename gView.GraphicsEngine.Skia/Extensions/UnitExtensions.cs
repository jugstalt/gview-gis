using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class UnitExtensions
    {
        static public float PointsToPixels(this float points) => points * (Current.Engine.ScreenDpi / 72f) * Current.Engine.ScreenDpi / 96f; //points * (96f / 72f) * Current.Engine.ScreenDpi / 96f;
        static public float PixelsToPoints(this float pixels) => pixels * (72f / Current.Engine.ScreenDpi) * 96f / Current.Engine.ScreenDpi;
    }
}
