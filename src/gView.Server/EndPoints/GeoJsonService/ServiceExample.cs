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
/// the queryable layers, the first layer that allows inserts (for applyEdits).
/// </summary>
internal record ServiceExample(string? Crs, BBox? Extent, string[] VisibleLayers, string[] FeatureLayers, ServiceExample.EditLayer? InsertLayer = null)
{
    /// <summary>A layer of the editor module that allows inserts, with the geometry type of its features</summary>
    public record EditLayer(string Id, gView.Framework.Core.Geometry.GeometryType GeometryType);

    public static ServiceExample From(IServiceMap map)
    {
        var layers = map.MapElements
            .OfType<ILayer>()
            .Select(layer => (layer, toc: map.TOC?.GetTOCElement(layer)))
            .Where(l => l.toc is not null && !l.toc.IsHidden() && l.layer is not IGroupLayer)
            .ToArray();

        var editModule = map.GetModule<Plugins.Modules.EditorModule>();
        var insertLayer = layers
            .Select(l => l.layer)
            .OfType<IFeatureLayer>()
            .FirstOrDefault(l => editModule?.GetEditLayer(l.ID)?.Statements.HasFlag(Framework.Editor.Core.EditStatements.INSERT) == true);

        return new ServiceExample(
            map.Display?.SpatialReference?.Name,
            map.FullExtent()?.ToBBox(),
            layers.Where(l => l.toc!.LayerVisible).Select(l => l.layer.ID.ToString()).ToArray(),
            layers.Where(l => l.layer is IFeatureLayer).Select(l => l.layer.ID.ToString()).ToArray(),
            insertLayer is null ? null : new EditLayer(insertLayer.ID.ToString(), insertLayer.LayerGeometryType));
    }

    /// <summary>
    /// A small geometry of the type of the layer at the center of the map (1% of the extent): point, line or square
    /// </summary>
    public Geometry? GeometryAtCenter(gView.Framework.Core.Geometry.GeometryType geometryType)
    {
        if (Extent is null || Center is null)
        {
            return null;
        }
        double x = Center[0], y = Center[1], d = (Extent.MaxX - Extent.MinX) / 100;
        return geometryType switch
        {
            gView.Framework.Core.Geometry.GeometryType.Point
                => new Geometry { Type = GeometryType.Point, Coordinates = new[] { x, y } },
            gView.Framework.Core.Geometry.GeometryType.Multipoint
                => new Geometry { Type = GeometryType.MultiPoint, Coordinates = new[] { new[] { x, y } } },
            gView.Framework.Core.Geometry.GeometryType.Polyline
                => new Geometry { Type = GeometryType.LineString, Coordinates = new[] { new[] { x - d, y }, new[] { x + d, y } } },
            gView.Framework.Core.Geometry.GeometryType.Polygon
                => new Geometry
                {
                    Type = GeometryType.Polygon,
                    Coordinates = new[] { new[] { new[] { x - d, y - d }, new[] { x + d, y - d }, new[] { x + d, y + d }, new[] { x - d, y + d }, new[] { x - d, y - d } } }
                },
            _ => null
        };
    }

    public string? QueryLayerId => FeatureLayers.FirstOrDefault();

    public CoordinateReferenceSystem? CrsObject => string.IsNullOrEmpty(Crs) ? null : CoordinateReferenceSystem.CreateByName(Crs);

    /// <summary>The center of the extent as GeoJSON point coordinates</summary>
    public double[]? Center => Extent is null ? null : [(Extent.MinX + Extent.MaxX) / 2, (Extent.MinY + Extent.MaxY) / 2];
}
