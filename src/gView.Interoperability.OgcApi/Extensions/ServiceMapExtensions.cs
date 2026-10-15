using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.UI;
using gView.Interoperability.OGC;
using gView.Interoperability.OgcApi.Features;
using System.Globalization;

namespace gView.Interoperability.OgcApi.Extensions;

internal static class ServiceMapExtensions
{
    private static readonly Guid WfsExportMetadataGuid = new Guid("A2342AC5-9FD6-47a4-A25A-F684C375C895");

    /// <summary>
    /// Every visible TOC layer element with exactly one feature class becomes a collection.
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

            if (tocElement.Layers[0] is IFeatureLayer featureLayer
                && featureLayer.Class is IFeatureClass featureClass)
            {
                yield return new OgcCollectionLayer(
                    featureLayer.ID,
                    String.IsNullOrWhiteSpace(tocElement.Name) ? featureLayer.Title : tocElement.Name,
                    map.GetLayerDescription(featureLayer.ID),
                    featureLayer,
                    featureClass,
                    featureClass.SpatialReference ?? map.LayerDefaultSpatialReference);
            }
        }
    }

    public static OgcCollectionLayer GetOgcCollectionLayer(this IServiceMap map, string collectionId)
        => map.OgcCollectionLayers().FirstOrDefault(c => c.Id == collectionId)
           ?? throw OgcApiException.NotFound($"Unknown collection '{collectionId}'");

    /// <summary>
    /// CRS84 + the storage CRS + the EPSG codes configured for the WFS export of the service
    /// </summary>
    public static IReadOnlyList<CrsReference> SupportedCrs(this IServiceMap map, OgcCollectionLayer? collection = null)
    {
        var result = new List<CrsReference> { CrsReference.Crs84 };

        void Add(CrsReference crs)
        {
            if (!result.Contains(crs))
            {
                result.Add(crs);
            }
        }

        if (collection?.SpatialReference?.EpsgCode > 0)
        {
            Add(CrsReference.FromEpsg(collection.SpatialReference.EpsgCode));
        }

        foreach (var epsgCode in map.WfsExportEpsgCodes())
        {
            Add(CrsReference.FromEpsg(epsgCode));
        }

        return result;
    }

    public static IEnumerable<int> WfsExportEpsgCodes(this IServiceMap map)
    {
        var codes = (map.MetadataProvider(WfsExportMetadataGuid) as IEpsgMetadata)?.EpsgCodes;

        if (codes is null || codes.Length == 0)
        {
            if (map.Display?.SpatialReference?.EpsgCode > 0)
            {
                yield return map.Display.SpatialReference.EpsgCode;
            }
            yield break;
        }

        foreach (var code in codes)
        {
            if (code is not null
                && code.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(code.Substring(5), NumberStyles.None, CultureInfo.InvariantCulture, out int epsgCode)
                && epsgCode > 0)
            {
                yield return epsgCode;
            }
        }
    }

    public static string ServiceTitle(this IServiceMap map)
        => !String.IsNullOrWhiteSpace(map.Title)
            ? map.Title
            : map.Name.Contains('/') ? map.Name.Substring(map.Name.LastIndexOf('/') + 1) : map.Name;
}
