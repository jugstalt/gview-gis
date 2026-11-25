#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.MapServer;
using gView.GeoJsonService.DTOs;
using gView.Server.AppCode.Extensions;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService;

/// <summary>
/// Values of one service for the examples of its OpenAPI description, so a request can be sent right away
/// (like the request forms of Browse Services did): CRS and full extent of the map, the layers shown by default,
/// the queryable layers.
/// </summary>
internal record ServiceExample(string? Crs, BBox? Extent, string[] VisibleLayers, string[] FeatureLayers)
{
    public static ServiceExample From(IServiceMap map)
    {
        var layers = map.MapElements
            .OfType<ILayer>()
            .Select(layer => (layer, toc: map.TOC?.GetTOCElement(layer)))
            .Where(l => l.toc is not null && !l.toc.IsHidden() && l.layer is not IGroupLayer)
            .ToArray();

        return new ServiceExample(
            map.Display?.SpatialReference?.Name,
            map.FullExtent()?.ToBBox(),
            layers.Where(l => l.toc!.LayerVisible).Select(l => l.layer.ID.ToString()).ToArray(),
            layers.Where(l => l.layer is IFeatureLayer).Select(l => l.layer.ID.ToString()).ToArray());
    }

    public string? QueryLayerId => FeatureLayers.FirstOrDefault();

    public CoordinateReferenceSystem? CrsObject => string.IsNullOrEmpty(Crs) ? null : CoordinateReferenceSystem.CreateByName(Crs);

    /// <summary>The center of the extent as GeoJSON point coordinates</summary>
    public double[]? Center => Extent is null ? null : [(Extent.MinX + Extent.MaxX) / 2, (Extent.MinY + Extent.MaxY) / 2];
}
