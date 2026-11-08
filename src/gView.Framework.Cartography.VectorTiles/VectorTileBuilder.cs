using gView.Framework.Cartography.VectorTiles.Extensions;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Tiling;
using gView.Framework.Metadata;
using SpatialNET;
using SpatialNET.IO;
using System.Globalization;

namespace gView.Framework.Cartography.VectorTiles;

/// <summary>
/// Builds one Mapbox vector tile (MVT 2.1) of a service map for the tile grid of its
/// <see cref="TileServiceMetadata"/>: any EPSG, the metadata scales, upper-left or lower-left origin.
/// </summary>
/// <remarks>
/// One MVT layer per visible feature layer (see <see cref="MvtLayerNames"/>); raster layers are ignored.
/// Geometries are projected to the tile CRS, then clipped, simplified and quantized by SpatialNET.
/// Attributes: the fields the renderers need plus the configured <see cref="TileServiceMetadata.MvtLayerAttributes"/>.
/// The OID becomes the MVT feature id.
/// </remarks>
public sealed class VectorTileBuilder
{
    public const string ContentType = "application/vnd.mapbox-vector-tile";

    private readonly IServiceMap _map;
    private readonly TileServiceMetadata _metadata;

    public VectorTileBuilder(IServiceMap map, TileServiceMetadata metadata)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    /// <summary>
    /// Builds the tile. Returns the gzipped tile bytes, or <c>null</c> if the tile holds no features.
    /// </summary>
    public async Task<byte[]?> BuildAsync(int epsg,
                                          double scale,
                                          int row,
                                          int col,
                                          GridOrientation orientation,
                                          ICancelTracker? cancelTracker = null)
    {
        if (!_metadata.EPSGCodes.Contains(epsg))
        {
            throw new ArgumentException($"EPSG:{epsg} is not part of the tile cache");
        }

        int level = _metadata.Scales.InnerList.IndexOf(_metadata.Scales.GetScale(scale));
        if (level < 0)
        {
            throw new ArgumentException("Wrong scale argument");
        }

        scale = _metadata.Scales.InnerList[level];

        var sRef = SpatialReference.FromID($"epsg:{epsg}")
            ?? throw new Exception($"Can't load spatial reference EPSG:{epsg}");

        var grid = _metadata.CreateTileGrid(epsg, orientation, sRef);
        var builder = new MvtTileBuilder(grid, level, row, col, MvtTileOptions.Default);

        #region Prepare display (scale dependent visibility, renderers)

        var display = _map.Display;
        display.SpatialReference = sRef;
        display.Dpi = _metadata.Dpi;
        display.ImageWidth = _metadata.TileWidth;
        display.ImageHeight = _metadata.TileHeight;

        var tileEnvelope = grid.TileEnvelope(level, row, col);
        display.ZoomTo(new Envelope(tileEnvelope.MinX, tileEnvelope.MinY, tileEnvelope.MaxX, tileEnvelope.MaxY));

        #endregion

        var clip = builder.ClipEnvelope;
        var clipEnvelope = new Envelope(clip.MinX, clip.MinY, clip.MaxX, clip.MaxY);
        var layerNames = MvtLayerNames.FromMap(_map);

        foreach (var layer in VisibleLayers())
        {
            if (cancelTracker?.Continue == false)
            {
                return null;
            }

            if (layer is not IFeatureLayer featureLayer
                || featureLayer.FeatureClass is null
                || !layer.RenderInScale(display))
            {
                continue;
            }

            string? mvtLayerName = layerNames[layer];
            if (mvtLayerName == null)
            {
                continue;
            }

            await AddLayerAsync(builder, featureLayer, mvtLayerName, sRef, clipEnvelope, cancelTracker);
        }

        return builder.IsEmpty ? null : builder.ToBytes(gzip: true);
    }

    #region Layers

    private IEnumerable<ILayer> VisibleLayers()
    {
        if (_map.TOC != null)
        {
            // TOC.VisibleLayers is top to bottom: MVT layers are drawn in order, so bottom first
            var layers = new List<ILayer>(_map.TOC.VisibleLayers);
            layers.Reverse();
            return layers;
        }

        return _map.MapElements.OfType<ILayer>().Where(l => l.Visible);
    }

    private async Task AddLayerAsync(MvtTileBuilder builder,
                                     IFeatureLayer layer,
                                     string mvtLayerName,
                                     ISpatialReference tileSRef,
                                     IEnvelope clipEnvelope,
                                     ICancelTracker? cancelTracker)
    {
        var fClass = layer.FeatureClass;
        var layerSRef = fClass.SpatialReference ?? _map.LayerDefaultSpatialReference;

        using var transformer = NeedsTransformation(layerSRef, tileSRef)
            ? GeometricTransformerFactory.Create(_map.Display.DatumTransformations)
            : null;
        transformer?.SetSpatialReferences(layerSRef, tileSRef);

        IGeometry queryGeometry = clipEnvelope;
        if (transformer != null)
        {
            var projected = transformer.InvTransform2D(Densify(clipEnvelope)) as IGeometry;
            if (projected?.Envelope == null)
            {
                return;
            }

            queryGeometry = projected.Envelope;
        }

        var filter = new SpatialFilter
        {
            Geometry = queryGeometry,
            SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects,
            MapScale = _map.Display.MapScale,
            CancelTracker = cancelTracker
        };
        filter.AddField(fClass.ShapeFieldName);
        if (!String.IsNullOrEmpty(fClass.IDFieldName))
        {
            filter.AddField(fClass.IDFieldName);
        }

        if (layer.FilterQuery != null)
        {
            filter.WhereClause = layer.FilterQuery.WhereClause;
        }

        PrepareAttributeFields(layer, mvtLayerName, filter);

        using var cursor = await fClass.GetFeatures(filter);
        if (cursor == null)
        {
            return;
        }

        IFeature feature;
        while ((feature = await cursor.NextFeature()) != null)
        {
            if (cancelTracker?.Continue == false)
            {
                return;
            }

            var shape = feature.Shape;
            if (shape == null)
            {
                continue;
            }

            if (transformer != null)
            {
                shape = transformer.Transform2D(shape) as IGeometry;
                if (shape == null)
                {
                    continue;
                }
            }

            var attributes = Attributes(feature, fClass).ToList();
            ulong? id = feature.OID > 0 ? (ulong)feature.OID : null;

            foreach (var geometry in Flatten(shape))
            {
                builder.AddFeature(mvtLayerName, geometry, attributes, id);
            }
        }
    }

    private void PrepareAttributeFields(IFeatureLayer layer, string mvtLayerName, IQueryFilter filter)
    {
        var renderer = layer.FeatureRenderer;
        if (renderer != null)
        {
            // PrepareQueryFilter only adds the fields the renderer needs (ValueMap field, Quantity field, ...)
            // and does not change the renderer itself, so no clone is required here.
            renderer.PrepareQueryFilter(layer, filter);
        }

        string? fields = null;
        if (!_metadata.MvtLayerAttributes.TryGetValue(mvtLayerName, out fields))
        {
            _metadata.MvtLayerAttributes.TryGetValue(layer.Title ?? String.Empty, out fields);
        }

        if (!String.IsNullOrWhiteSpace(fields))
        {
            foreach (var field in fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (field == "*")
                {
                    filter.SubFields = "*";
                    break;
                }

                filter.AddField(field);
            }
        }
    }

    #endregion

    #region Helper

    private static bool NeedsTransformation(ISpatialReference? from, ISpatialReference to)
        => from != null && !from.Equals(to);

    private static IPolygon Densify(IEnvelope envelope, int steps = 16)
    {
        var ring = new Ring();
        double dx = envelope.Width / steps, dy = envelope.Height / steps;
        for (int i = 0; i < steps; i++) ring.AddPoint(new Point(envelope.MinX + i * dx, envelope.MinY));
        for (int i = 0; i < steps; i++) ring.AddPoint(new Point(envelope.MaxX, envelope.MinY + i * dy));
        for (int i = 0; i < steps; i++) ring.AddPoint(new Point(envelope.MaxX - i * dx, envelope.MaxY));
        for (int i = 0; i < steps; i++) ring.AddPoint(new Point(envelope.MinX, envelope.MaxY - i * dy));
        ring.Close();

        var polygon = new Polygon();
        polygon.AddRing(ring);
        return polygon;
    }

    private static IEnumerable<IGeometry2D> Flatten(IGeometry shape)
    {
        if (shape is IAggregateGeometry aggregate)
        {
            for (int i = 0; i < aggregate.GeometryCount; i++)
            {
                foreach (var g in Flatten(aggregate[i]))
                {
                    yield return g;
                }
            }
        }
        else if (shape is IGeometry2D geometry2D)
        {
            yield return geometry2D;
        }
    }

    private static IEnumerable<KeyValuePair<string, object?>> Attributes(IFeature feature, IFeatureClass fClass)
    {
        foreach (var field in feature.Fields)
        {
            if (field?.Name == null
                || field.Name.Equals(fClass.ShapeFieldName, StringComparison.OrdinalIgnoreCase)
                || field.Name.Equals(fClass.IDFieldName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            object? value = field.Value switch
            {
                null or DBNull => null,
                string s => s,
                bool b => b,
                byte or sbyte or short or ushort or int or uint or long => Convert.ToInt64(field.Value, CultureInfo.InvariantCulture),
                ulong ul => ul,
                float f => f,
                double d => d,
                decimal m => (double)m,
                DateTime dt => dt.ToString("o", CultureInfo.InvariantCulture),
                Guid g => g.ToString(),
                byte[] => null,
                IGeometry => null,
                var other => Convert.ToString(other, CultureInfo.InvariantCulture)
            };

            if (value != null)
            {
                yield return new KeyValuePair<string, object?>(field.Name, value);
            }
        }
    }

    #endregion
}
