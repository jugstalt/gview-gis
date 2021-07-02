using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class UnitExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float FontSizePointsToPixels(this float points) => points * (Current.Engine.ScreenDpi / 72f) * Current.Engine.ScreenDpi / 96f; //points * (96f / 72f) * Current.Engine.ScreenDpi / 96f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float PointsToPixels(this float points) => 96f / 72f;
    }
}
