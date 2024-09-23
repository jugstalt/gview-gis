using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Data.Filters;
using gView.GraphicsEngine;
using System.Collections.Generic;
using System.Linq;

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

        static public string TocNameOrLayerTitle(this ILayer layer, IMap map)
            => map?.TOC?.GetTocElementByLayerId(layer.ID)?.Name
            ?? layer.Title;

        static public void ClonePropertiesFrom(this ILayer targetLayer, ILayer sourceLayer)
        {
            #region copy general ILayer propertiers

            if (targetLayer is not null)
            {
                targetLayer.MinimumScale = sourceLayer.MinimumScale;
                targetLayer.MaximumScale = sourceLayer.MaximumScale;

                targetLayer.MinimumLabelScale = sourceLayer.MinimumLabelScale;
                targetLayer.MaximumLabelScale = sourceLayer.MaximumLabelScale;

                targetLayer.MaximumZoomToFeatureScale = sourceLayer.MaximumZoomToFeatureScale;
            }

            #endregion

            #region copy general IFeatureLayer properties

            if (targetLayer is IFeatureLayer newFeatureLayer
                && sourceLayer is IFeatureLayer originalFeatureLayer)
            {
                if (originalFeatureLayer.FilterQuery != null)
                {
                    QueryFilter filter = new QueryFilter();
                    filter.WhereClause = originalFeatureLayer.FilterQuery.WhereClause;
                    newFeatureLayer.FilterQuery = filter;
                }

                newFeatureLayer.FeatureRenderer = originalFeatureLayer.FeatureRenderer?.Clone() as IFeatureRenderer /*?? newFeatureLayer.FeatureRenderer*/;
                newFeatureLayer.LabelRenderer = originalFeatureLayer.LabelRenderer?.Clone() as ILabelRenderer /*?? newFeatureLayer.LabelRenderer*/;

                newFeatureLayer.MaxRefScaleFactor = originalFeatureLayer.MaxRefScaleFactor;
                newFeatureLayer.MaxLabelRefScaleFactor = originalFeatureLayer.MaxLabelRefScaleFactor;

                newFeatureLayer.Joins = originalFeatureLayer.Joins?.Clone() as FeatureLayerJoins;
            }

            #endregion

            #region copy general IRasterLayer properties

            if (targetLayer is IRasterLayer newRasterLayer
               && sourceLayer is IRasterLayer originalRasterLayer)
            {
                newRasterLayer.InterpolationMethod = originalRasterLayer.InterpolationMethod;
                newRasterLayer.Opacity = originalRasterLayer.Opacity;
                newRasterLayer.TransparentColor = ArgbColor.FromColor(originalRasterLayer.TransparentColor);
                newRasterLayer.FilterImplementation = originalRasterLayer.FilterImplementation;
            }

            #endregion
        }

        static public bool TrySetGroupLayer(this ILayer layer, IGroupLayer groupLayer)
        {
            if (layer is Layer l)
            {
                l.GroupLayer = groupLayer;
                return true;
            }

            return false;
        }

        static public bool TryRemoveLayer(this IGroupLayer groupLayer, ILayer layer)
        {
            if (groupLayer is GroupLayer g
                && layer is Layer l
                && g.ChildLayers.Contains(l))
            {
                g.Remove(l);
                return true;
            }

            return false;
        }

        static public IEnumerable<ILayer> CreateCopy(
                    this ILayer layer,
                    IMap map,
                    bool recursive = true,
                    IGroupLayer targetGroupLayer = null,
                    bool newName = true)
        {
            var result = new List<ILayer>();

            var copy = layer.Class switch
            {
                null when layer is IGroupLayer => new GroupLayer(),
                null => null,
                _ => LayerFactory.Create(layer.Class)
            };

            if (copy is null) return result;

            copy.ClonePropertiesFrom(layer);
            copy.TrySetGroupLayer(targetGroupLayer ?? layer.GroupLayer);
            copy.Title = newName switch
            {
                true => layer.NextUnusedName(map),
                false => layer.TocNameOrLayerTitle(map)
            };

            result.Add(copy);

            if (recursive
                && copy is IGroupLayer copyGroup
                && layer is IGroupLayer groupLayer)
            {
                foreach (var childLayer in groupLayer.ChildLayers ?? [])
                {
                    result.AddRange(childLayer.CreateCopy(map, targetGroupLayer: copyGroup, newName: false));
                }
            }

            return result;
        }

        static public string NextUnusedName(this ILayer layer, IMap map)
        {
            int index = 0;
            while (true)
            {
                var newName = $"{layer.TocNameOrLayerTitle(map)} ({++index})";

                if (map.TOC.Elements.Any(e => e.Name == newName) == false)
                {
                    return newName;
                }
            }
        }
    }
}
