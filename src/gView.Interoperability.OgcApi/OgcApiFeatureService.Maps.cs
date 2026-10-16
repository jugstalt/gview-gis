using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Interoperability.Extensions.Rendering;
using gView.Interoperability.OgcApi.Extensions;
using gView.Interoperability.OgcApi.Maps;

namespace gView.Interoperability.OgcApi;

/// <summary>
/// OGC API - Maps resources: /map (dataset) and /collections/{collectionId}/map
/// </summary>
public sealed partial class OgcApiFeatureService
{
    public string MapUrl => $"{ServiceUrl}/map";
    public string CollectionMapUrl(string collectionId) => $"{CollectionUrl(collectionId)}/map";

    /// <param name="collectionId">null => dataset map</param>
    public async Task<MapResult> GetMapAsync(string? collectionId, IReadOnlyDictionary<string, string> parameters, string? accept)
    {
        var collection = collectionId is null ? null : GetCollectionLayer(collectionId);
        var query = MapQuery.Parse(parameters, accept, Map.SupportedMapCrs(), isDatasetMap: collection is null);

        #region Visible layers

        List<int>? visibleLayerIds = collection is not null
            ? [collection.LayerId]
            : query.Collections?
                .Select(id => CollectionLayers.FirstOrDefault(c => c.Id == id)?.LayerId
                              ?? throw OgcApiException.BadRequest($"Unknown collection '{id}' in collections"))
                .ToList();

        #endregion

        #region CRS and extent

        var crs = query.Crs
            ?? (Map.Display?.SpatialReference?.EpsgCode > 0
                ? CrsReference.FromEpsg(Map.Display.SpatialReference.EpsgCode)
                : CrsReference.Crs84);
        var sref = crs.ToSpatialReference();

        IEnvelope bbox;
        if (query.BBox is not null)
        {
            var bboxSref = query.BBoxCrs.ToSpatialReference();
            var b = query.BBox;
            IEnvelope envelope = query.BBoxCrs.HasNorthingFirst(bboxSref)
                ? new Envelope(b[1], b[0], b[3], b[2])
                : new Envelope(b[0], b[1], b[2], b[3]);

            bbox = TransformExtent(envelope, bboxSref, sref)
                ?? throw OgcApiException.BadRequest("bbox can't be transformed into the map CRS");
        }
        else
        {
            bbox = (collection is not null
                        ? TransformExtent(collection.NativeExtent, collection.SpatialReference, sref)
                        : FullExtent(sref))
                ?? throw OgcApiException.BadRequest("The extent of the map is unknown, use the bbox parameter");
        }

        double extentWidth = bbox.MaxX - bbox.MinX, extentHeight = bbox.MaxY - bbox.MinY;
        if (extentWidth <= 0 || extentHeight <= 0)
        {
            throw OgcApiException.BadRequest("The bbox must have a width and a height");
        }

        #endregion

        var (width, height) = query.ImageSize(
            extentWidth,
            extentHeight,
            sref.SpatialParameters?.IsGeographic == true ? null : 1.0,
            Map.MapServiceProperties?.MaxImageWidth ?? 0,
            Map.MapServiceProperties?.MaxImageHeight ?? 0);

        var image = await Map.RenderImageAsync(new MapImageRequest()
        {
            SpatialReference = sref,
            BBox = bbox,
            Width = width,
            Height = height,
            Dpi = query.Dpi,
            Transparent = query.Transparent,
            BackgroundColor = query.BackgroundColor,
            Format = query.Format,
            BeforeRenderLayers = visibleLayerIds is null
                ? null
                : (sender, context, layers) =>
                {
                    foreach (var layer in layers)
                    {
                        if (layer is not null)
                        {
                            layer.Visible = false;
                        }
                    }

                    foreach (var layerId in visibleLayerIds)
                    {
                        foreach (ILayer layer in MapServerHelper.FindMapLayers(sender, true, layerId.ToString(), layers))
                        {
                            layer.Visible = true;
                        }
                    }
                }
        }) ?? throw new OgcApiException(500, "ServerError", "The map could not be rendered");

        double[] contentBbox = crs.HasNorthingFirst(sref)
            ? [bbox.MinY, bbox.MinX, bbox.MaxY, bbox.MaxX]
            : [bbox.MinX, bbox.MinY, bbox.MaxX, bbox.MaxY];

        return new MapResult(image, query.ContentType, crs, contentBbox);
    }

    private IEnvelope? FullExtent(ISpatialReference sref)
    {
        Envelope? fullExtent = null;

        foreach (var collection in CollectionLayers)
        {
            var extent = TransformExtent(collection.NativeExtent, collection.SpatialReference, sref);
            if (extent is null)
            {
                continue;
            }

            if (fullExtent is null)
            {
                fullExtent = new Envelope(extent);
            }
            else
            {
                fullExtent.Union(extent);
            }
        }

        return fullExtent;
    }
}
