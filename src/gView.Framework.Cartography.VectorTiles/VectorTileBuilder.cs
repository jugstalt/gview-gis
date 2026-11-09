using gView.Framework.Cartography.VectorTiles.Extensions;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Geometry;
using gView.Framework.Geometry.GeoProcessing;
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

        #region Prepare display (scale dependent visibility, renderers)

        var display = _map.Display;
        display.SpatialReference = sRef;
        display.Dpi = _metadata.Dpi;
        display.ImageWidth = _metadata.TileWidth;
        display.ImageHeight = _metadata.TileHeight;

        var tileEnvelope = grid.TileEnvelope(level, row, col);  // SpatialNET Envelope2D
        display.ZoomTo(new Envelope(tileEnvelope.MinX, tileEnvelope.MinY, tileEnvelope.MaxX, tileEnvelope.MaxY));

        // the scale computed from the envelope differs in the last digits from tile to tile (249999.9999 / 250000.0001):
        // a layer visible up to exactly this scale would flip from tile to tile. All tiles of a level use the level's scale,
        // as the style does.
        display.MapScale = scale;

        #endregion

        var layers = VisibleLayers()
            .OfType<IFeatureLayer>()
            .Where(l => l.FeatureClass is not null && l.RenderInScale(display))
            .ToList();

        // Clipping adds artificial polygon edges on the buffer border. Clients cut each tile at its border,
        // so these edges stay invisible only if the buffer is wider than half of the widest line (or the
        // largest symbol) at this scale. Otherwise thick outlines show up as stripes along the tile borders.
        var options = MvtTileOptions.Default with { Buffer = BufferFor(layers, level) };
        var builder = new MvtTileBuilder(grid, level, row, col, options);

        var clip = builder.ClipEnvelope;
        var clipEnvelope = new Envelope(clip.MinX, clip.MinY, clip.MaxX, clip.MaxY);
        var layerNames = MvtLayerNames.FromMap(_map);

        foreach (var featureLayer in layers)
        {
            if (cancelTracker?.Continue == false)
            {
                return null;
            }

            var layer = (ILayer)featureLayer;

            string? mvtLayerName = layerNames[layer];
            if (mvtLayerName == null)
            {
                continue;
            }

            var labels = layer.LabelInScale(display)
                ? MvtLabels.Entries(featureLayer.LabelRenderer).Where(e => e.InScale(scale)).ToList()
                : [];

            await AddLayerAsync(builder, featureLayer, mvtLayerName, sRef, clipEnvelope, tileEnvelope, labels, cancelTracker);
        }

        return builder.IsEmpty ? null : builder.ToBytes(gzip: true);
    }

    /// <summary>
    /// The clip buffer in tile units: the default (64 of 4096), or more when half of the widest symbol
    /// (plus 2 pixels for antialiasing) reaches further into the neighbor tile.
    /// </summary>
    /// <remarks>
    /// Clients show the tiles of a level not only at its own scale but between the neighbor levels (OpenLayers
    /// picks the nearest level), scaled, while the style interpolates line widths over the zoom:
    /// <list type="bullet">
    /// <item>towards the next finer level the lines get wider (up to that level's width) while the tile is magnified;</item>
    /// <item>towards the coarser level the tile is shrunk, so the same pixel width covers more tile units.</item>
    /// </list>
    /// The buffer covers both ends, taken half way (geometric mean of the resolutions) to the neighbor levels.
    /// </remarks>
    private double BufferFor(IEnumerable<IFeatureLayer> layers, int level)
    {
        var scales = _metadata.Scales.InnerList;
        double scale = scales[level];

        double MaxPixels(double atScale) => layers
            .Select(l => l.MaxSymbolPixels(atScale, _map.Display.ReferenceScale, _metadata.Dpi))
            .DefaultIfEmpty(0)
            .Max();

        double here = MaxPixels(scale);

        // half way to the coarser level: same widths at most, the tile shown smaller
        double towardsCoarser = level > 0
            ? here * Math.Sqrt(scales[level - 1] / scale)
            : here;

        // half way to the finer level: widths up to the finer level's, the tile shown larger
        double towardsFiner = level < scales.Count - 1
            ? MaxPixels(scales[level + 1]) * Math.Sqrt(scales[level + 1] / scale)
            : here;

        double maxPixels = Math.Max(here, Math.Max(towardsCoarser, towardsFiner));

        double tilePixels = Math.Max(1, Math.Min(_metadata.TileWidth, _metadata.TileHeight));
        double unitsPerPixel = MvtTileOptions.Default.Extent / tilePixels;
        double needed = (maxPixels / 2.0 + 2.0) * unitsPerPixel;

        // never more than half a tile: features from further away would multiply the tile size
        return Math.Min(MvtTileOptions.Default.Extent / 2.0, Math.Max(MvtTileOptions.Default.Buffer, Math.Ceiling(needed)));
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
                                     Envelope2D tileEnvelope,
                                     IReadOnlyList<MvtLabels.LabelEntry> labels,
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
        double labelTileCoverage = _metadata.LabelTileCoverage(mvtLayerName, layer.Title);
        foreach (var label in labels)
        {
            label.Renderer.PrepareQueryFilter(_map.Display, layer, filter);
        }

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

            if (labels.Count > 0)
            {
                AddLabels(builder, mvtLayerName, feature, shape, tileEnvelope, labels, labelTileCoverage, id);
            }
        }
    }

    /// <summary>
    /// The label feature(s) of <paramref name="feature"/> in the <c>{layer}__labels</c> MVT layer: the label texts
    /// computed by gView, on label points inside this tile (points, polygons) or on the line itself.
    /// </summary>
    private void AddLabels(MvtTileBuilder builder,
                           string mvtLayerName,
                           IFeature feature,
                           IGeometry shape,
                           Envelope2D tileEnvelope,
                           IReadOnlyList<MvtLabels.LabelEntry> labels,
                           double labelTileCoverage,
                           ulong? id)
    {
        var texts = new List<KeyValuePair<string, object?>>();
        foreach (var label in labels)
        {
            if (!label.Renderer.LabelsFeature(_map.Display, feature))
            {
                continue;
            }

            string? text = label.Renderer.EvaluateLabelText(_map.Display, feature)?.Trim();
            if (!String.IsNullOrEmpty(text))
            {
                texts.Add(new KeyValuePair<string, object?>(label.Attribute, text));
            }
        }

        if (texts.Count == 0)
        {
            return;
        }

        string labelLayer = MvtLabels.LayerName(mvtLayerName);
        bool perPart = labels.Any(l => l.Renderer.HowManyLabels == SimpleLabelRenderer.RenderHowManyLabels.OnPerPart);

        foreach (var part in Flatten(shape))
        {
            if (part is IPolyline2D)
            {
                builder.AddFeature(labelLayer, part, texts, id);
                continue;
            }

            if (part is IPolygon largePolygon && labelTileCoverage > 0 &&
                TileLabelPoint(largePolygon, tileEnvelope, labelTileCoverage) is IPoint tilePoint)
            {
                // the part covers a large share of this tile: label it here, wherever its label point is
                builder.AddFeature(labelLayer, new gView.Framework.Geometry.Point(tilePoint.X, tilePoint.Y), texts, id);
                if (!perPart)
                {
                    break;
                }

                continue;
            }

            IEnumerable<IPoint> points = part switch
            {
                IPolygon polygon => Algorithm.PolygonLabelPoints(polygon),
                IMultiPoint multiPoint => multiPoint,
                IPoint point => [point],
                _ => []
            };

            if (!perPart)
            {
                points = points.Take(1);
            }

            foreach (var point in points)
            {
                // only in the tile that contains the point: a label must not repeat in the neighbor tiles
                if (point.X >= tileEnvelope.MinX && point.X < tileEnvelope.MaxX &&
                    point.Y > tileEnvelope.MinY && point.Y <= tileEnvelope.MaxY)
                {
                    builder.AddFeature(labelLayer, new gView.Framework.Geometry.Point(point.X, point.Y), texts, id);
                }
            }

            if (!perPart)
            {
                break;
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

    /// <summary>
    /// The label point of the part of <paramref name="polygon"/> inside the tile, if that part covers at least
    /// <paramref name="coverage"/> (0..1) of the tile; otherwise <c>null</c> (the polygon's own label point applies).
    /// </summary>
    private static IPoint? TileLabelPoint(IPolygon polygon, Envelope2D tileEnvelope, double coverage)
    {
        double tileArea = tileEnvelope.Width * tileEnvelope.Height;
        if (tileArea <= 0 || polygon.Area < coverage * tileArea)
        {
            return null;  // the whole polygon is smaller than the threshold: no clip needed
        }

        var envelope = new Envelope(tileEnvelope.MinX, tileEnvelope.MinY, tileEnvelope.MaxX, tileEnvelope.MaxY);
        if (Clip.PerformClip(envelope, polygon) is not IPolygon clipped || clipped.Area < coverage * tileArea)
        {
            return null;
        }

        foreach (var point in Algorithm.PolygonLabelPoints(clipped))
        {
            if (point.X >= tileEnvelope.MinX && point.X < tileEnvelope.MaxX &&
                point.Y > tileEnvelope.MinY && point.Y <= tileEnvelope.MaxY)
            {
                return point;
            }
        }

        return null;
    }

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
