using gView.Framework.Core.Carto;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.Common;
using System;
using gView.GraphicsEngine;

namespace gView.Framework.Symbology
{
    public static class ReferenceScaleHelper
    {
        private const float MinimumPenWidth = 1.0f;  // The min width in skia should not be <1f, otherwise lines can be so thin, they disapear

        static public float PenWidth(float penWidth, IPenWidth symbol, IDisplay display)
        {
            float dpiFactor = display == null || display.Dpi == 96.0f ?
                1f :
                (float)(display.Dpi / 96.0);

            var result = penWidth;

            if (symbol.MinPenWidth > 0f && penWidth < symbol.MinPenWidth * dpiFactor)
            {
                result = symbol.MinPenWidth * dpiFactor;
            }
            else if (symbol.MaxPenWidth > 0 && penWidth > symbol.MaxPenWidth * dpiFactor)
            {
                result = symbol.MaxPenWidth * dpiFactor;
            }

            return Math.Min(MinimumPenWidth, result);
        }

        static public float CalcPixelUnitFactor(CloneOptions options)
        {
            float fac = 1f;

            var display = options.Display;
            if (display != null)
            {
                if (options.ApplyRefScale)
                {
                    fac = (float)(display.ReferenceScale / display.MapScale);
                    fac = options.RefScaleFactor(fac);
                }
                fac *= options.DpiFactor;
            }

            return fac;
        }

        static public float RefscaleFactor(float factor, float sizeValue, float minSizeValue, float maxSizeValue)
        {
            if (sizeValue == 0)
            {
                return factor;
            }

            if (minSizeValue > 0 && sizeValue * factor < minSizeValue)
            {
                return minSizeValue / sizeValue;
            }
            if (maxSizeValue > 0 && sizeValue * factor > maxSizeValue)
            {
                return maxSizeValue / sizeValue;
            }

            return factor;
        }
    }
}
