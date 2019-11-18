using gView.Framework.Carto;
using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Data
{
    public static class Extensions
    {
        static public bool RenderInScale(this ILayer layer, IDisplay display)
        {
            if (layer.MinimumScale > 1 && layer.MinimumScale > display.mapScale)
            {
                return false;
            }

            if (layer.MaximumScale > 1 && layer.MaximumScale < display.mapScale)
            {
                return false;
            }

            return true;
        }

        static public bool LabelInScale(this ILayer layer, IDisplay display)
        {
            if (layer.MinimumLabelScale > 1 && layer.MinimumLabelScale > display.mapScale)
            {
                return false;
            }

            if (layer.MaximumLabelScale > 1 && layer.MaximumLabelScale < display.mapScale)
            {
                return false;
            }

            return true;
        }
    }
}
