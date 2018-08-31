using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Framework.Symbology
{
    static class ReferenceScaleHelper
    {
        static public float PenWidth(float penWidth, IPenWidth symbol, Carto.IDisplay display)
        {
            float dpiFactor = display == null || display.dpi == 96.0f ? 1f : (float)(display.dpi / 96.0);

            if (symbol.MinPenWidth > 0f && penWidth < symbol.MinPenWidth * dpiFactor)
                return symbol.MinPenWidth * dpiFactor;

            if (symbol.MaxPenWidth > 0 && penWidth > symbol.MaxPenWidth * dpiFactor)
                return symbol.MaxPenWidth * dpiFactor;

            return penWidth;
        }

        static public float RefscaleFactor(float factor, float sizeValue, float minSizeValue, float maxSizeValue)
        {
            if (sizeValue == 0)
                return factor;

            if (minSizeValue > 0 && sizeValue * factor < minSizeValue)
            {
                return minSizeValue / sizeValue;
            }
            if(maxSizeValue>0 && sizeValue*factor>maxSizeValue)
            {
                return maxSizeValue / sizeValue;
            }

            return factor;
        }
    }
}
