using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Carto
{
    static public class Extensions
    {
        static public bool RequiresFeatureRendererClone(this IFeatureLayer layer)
        {
            if (layer?.FeatureRenderer != null)
            {
                if (layer.FeatureRenderer.UseReferenceScale && layer.ApplyRefScale)
                {
                    return true;
                }

                if (layer.FeatureRenderer.RequireClone())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
