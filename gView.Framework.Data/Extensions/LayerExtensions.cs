using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using System;

namespace gView.Framework.Data.Extensions
{
    public static class LayerExtensions
    {
        static public bool RenderInScale(this ILayer layer, IDisplay display)
        {
            if (layer.MinimumScale > 1 && layer.MinimumScale > display.MapScale)
            {
                return false;
            }

            if (layer.MaximumScale > 1 && layer.MaximumScale < display.MapScale)
            {
                return false;
            }

            return true;
        }

        static public bool LabelInScale(this ILayer layer, IDisplay display)
        {
            if (layer.MinimumLabelScale <= 1 && layer.MaximumLabelScale <= 1)
            {
                return layer.RenderInScale(display);
            }

            if (layer.MinimumLabelScale > 1 && layer.MinimumLabelScale > display.MapScale)
            {
                return false;
            }

            if (layer.MaximumLabelScale > 1 && layer.MaximumLabelScale < display.MapScale)
            {
                return false;
            }

            return true;
        }
    }
}
