using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.UI;
using gView.Interoperability.Extensions.Extensions;
using gView.Interoperability.Extensions.Features;
using gView.Interoperability.Extensions.Rendering;
using System;
using System.Collections.Generic;

namespace gView.Interoperability.Extensions;

static public class ServiceMapExtensions
{
    /// <summary>
    /// Fits Display.ImageWidth/ImageHeight/Dpi into the limits of the map service
    /// (see <see cref="ImageSizeLimits.Fit"/>)
    /// </summary>
    /// <returns>true, if the image was scaled down</returns>
    /// <exception cref="gView.Framework.Core.Exceptions.MapServerException">width or height are not positive</exception>
    static public bool ResizeImageSizeToMapServiceLimits(this IServiceMap map)
    {
        var properties = map.MapServiceProperties;

        var (width, height, dpi, resized) = ImageSizeLimits.Fit(
            map.Display.ImageWidth,
            map.Display.ImageHeight,
            map.Display.Dpi,
            properties?.MaxImageWidth ?? 0,
            properties?.MaxImageHeight ?? 0,
            properties?.MaxDpi ?? 0);

        map.Display.ImageWidth = width;
        map.Display.ImageHeight = height;
        map.Display.Dpi = dpi;

        return resized;
    }

    /// <summary>
    /// Every visible TOC layer element with exactly one feature or raster layer becomes a collection.
    /// Layers inside checkbox-style group layers are hidden (same rule as the GeoJsonService).
    /// </summary>
    public static IEnumerable<OgcCollectionLayer> OgcCollectionLayers(this IServiceMap map)
    {
        if (map?.TOC?.Elements is null)
        {
            yield break;
        }

        foreach (var tocElement in map.TOC.Elements)
        {
            if (tocElement is null
                || tocElement.ElementType != TocElementType.Layer
                || tocElement.Layers is null
                || tocElement.Layers.Count != 1
                || tocElement.IsHiddenForServices())
            {
                continue;
            }

            var layer = tocElement.Layers[0];
            string title = String.IsNullOrWhiteSpace(tocElement.Name) ? layer.Title : tocElement.Name;

            if (layer is IFeatureLayer featureLayer
                && featureLayer.Class is IFeatureClass featureClass)
            {
                yield return new OgcCollectionLayer(
                    featureLayer,
                    title,
                    map.GetLayerDescription(featureLayer.ID),
                    featureClass.Envelope,
                    featureClass.SpatialReference ?? map.LayerDefaultSpatialReference);
            }
            else if (layer is IRasterLayer rasterLayer
                     && rasterLayer.Class is IRasterClass rasterClass)
            {
                yield return new OgcCollectionLayer(
                    rasterLayer,
                    title,
                    map.GetLayerDescription(rasterLayer.ID),
                    rasterClass.Polygon?.Envelope,
                    rasterClass.SpatialReference ?? map.LayerDefaultSpatialReference);
            }
        }
    }
}
