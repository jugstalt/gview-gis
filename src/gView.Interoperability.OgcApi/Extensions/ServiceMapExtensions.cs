using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.UI;
using gView.Interoperability.OGC;
using gView.Interoperability.Extensions;

namespace gView.Interoperability.OgcApi.Extensions;

internal static class ServiceMapExtensions
{
    private static readonly Guid WfsExportMetadataGuid = new Guid("A2342AC5-9FD6-47a4-A25A-F684C375C895");

    private static readonly Guid WmsExportMetadataGuid = new Guid("0F6317BC-38FD-41d3-8E1A-82AB1873C526");

    public static OgcCollectionLayer GetOgcCollectionLayer(this IServiceMap map, string collectionId)
        => map.OgcCollectionLayers().FirstOrDefault(c => c.Id == collectionId)
           ?? throw OgcApiException.NotFound($"Unknown collection '{collectionId}'");

    /// <summary>
    /// Items, queryables, ... only exist for feature collections
    /// </summary>
    public static OgcCollectionLayer GetOgcFeatureCollectionLayer(this IServiceMap map, string collectionId)
    {
        var collection = map.GetOgcCollectionLayer(collectionId);
        return collection.HasItems
            ? collection
            : throw OgcApiException.NotFound($"Collection '{collectionId}' has no features");
    }

    /// <summary>
    /// The map CRS + the EPSG codes configured for the WMS export of the service
    /// </summary>
    public static IReadOnlyList<CrsReference> SupportedMapCrs(this IServiceMap map)
    {
        var result = new List<CrsReference>();

        void Add(CrsReference crs)
        {
            if (!result.Contains(crs))
            {
                result.Add(crs);
            }
        }

        if (map.Display?.SpatialReference?.EpsgCode > 0)
        {
            Add(CrsReference.FromEpsg(map.Display.SpatialReference.EpsgCode));
        }

        foreach (var epsgCode in map.ExportEpsgCodes(WmsExportMetadataGuid))
        {
            Add(CrsReference.FromEpsg(epsgCode));
        }

        Add(CrsReference.Crs84);

        return result;
    }

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

        foreach (var epsgCode in map.ExportEpsgCodes(WfsExportMetadataGuid))
        {
            Add(CrsReference.FromEpsg(epsgCode));
        }

        return result;
    }

    public static IEnumerable<int> ExportEpsgCodes(this IServiceMap map, Guid exportMetadataGuid)
    {
        var codes = (map.MetadataProvider(exportMetadataGuid) as IEpsgMetadata)?.EpsgCodes;

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
            if (CrsReference.TryParse(code, out var crs) && !crs.IsCrs84)
            {
                yield return crs.EpsgCode;
            }
        }
    }

    public static string ServiceTitle(this IServiceMap map)
        => !String.IsNullOrWhiteSpace(map.Title)
            ? map.Title
            : map.Name.Contains('/') ? map.Name.Substring(map.Name.LastIndexOf('/') + 1) : map.Name;
}
