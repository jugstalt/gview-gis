using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Carto
{
    static public class Extensions
    {
        static public bool RequiresFeatureRendererClone(this IFeatureLayer layer, IDisplay display)
        {
            if (layer?.FeatureRenderer != null)
            {
                if (layer.UseWithRefScale(display))
                {
                    return true;
                }

                if (layer.FeatureRenderer.RequireClone())
                {
                    return true;
                }

                if (display.dpi != 96.0)
                {
                    return true;
                }
            }

            return false;
        }

        static public bool RequiresLabelRendererClone(this IFeatureLayer layer, IDisplay display)
        {
            if (layer?.LabelRenderer != null)
            {
                if (layer.UseLabelsWithRefScale(display))
                {
                    return true;
                }

                if (display.dpi != 96.0)
                {
                    return true;
                }
            }

            return false;
        }

        static public bool UseWithRefScale(this IFeatureLayer layer, IDisplay display)
        {
            return display.refScale > 1 && layer.FeatureRenderer.UseReferenceScale && layer.ApplyRefScale;
        }

        static public bool UseLabelsWithRefScale(this IFeatureLayer layer, IDisplay display)
        {
            return display.refScale > 1 && layer.ApplyLabelRefScale;
        }
    }
}
