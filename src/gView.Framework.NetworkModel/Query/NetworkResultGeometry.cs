using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.NetworkModel.Build;

namespace gView.Framework.NetworkModel.Query;

/// <summary>The real geometries of a result: the parts of the line features its edges stand for, read from the map.</summary>
public static class NetworkResultGeometry
{
    private const int OidsPerQuery = 500;

    /// <summary>
    /// The coordinates of every edge of the result in the spatial reference of the network: the part of its line feature
    /// between its measures. Edges whose layer or feature is not in the map are left out (written as straight lines).
    /// </summary>
    public static async Task<Dictionary<long, IReadOnlyList<double[]>>> EdgeCoordinatesAsync(
        IMap map, NetworkQueryResult result, CancellationToken cancellationToken = default)
    {
        var coordinates = new Dictionary<long, IReadOnlyList<double[]>>();
        if (result.Edges.Count == 0)
        {
            return coordinates;
        }

        var layers = MapLayers.FeatureLayers(map);
        var srs = string.IsNullOrEmpty(result.Srs) ? map.Display?.SpatialReference : SpatialReference.FromID(result.Srs);
        foreach (var group in result.Edges.GroupBy(e => e.Layer, StringComparer.OrdinalIgnoreCase))
        {
            MapLayer layer;
            try
            {
                layer = MapLayers.Find(layers, group.Key);
            }
            catch (InvalidDataException)
            {
                continue;
            }

            var shapes = await ShapesAsync(layer.Layer, group.Select(e => e.Oid).Distinct(), srs, map.Display?.DatumTransformations, cancellationToken);
            foreach (var edge in group)
            {
                if (shapes.TryGetValue(edge.Oid, out var shape) && shape is IPolyline polyline)
                {
                    var section = Section(polyline, edge.FromMeasure, edge.ToMeasure);
                    if (section.Count >= 2)
                    {
                        coordinates[edge.Id] = section;
                    }
                }
            }
        }
        return coordinates;
    }

    /// <summary>
    /// Projects the coordinates of the result (nodes, violations, the straight edges) and of the edge coordinates into
    /// another spatial reference; sets <see cref="NetworkQueryResult.Srs"/>.
    /// </summary>
    public static void Project(
        NetworkQueryResult result, Dictionary<long, IReadOnlyList<double[]>> edgeCoordinates,
        ISpatialReference from, ISpatialReference to, IDatumTransformations? datumTransformations = null)
    {
        using var transformer = GeometricTransformerFactory.Create(datumTransformations);
        transformer.SetSpatialReferences(from, to);
        (double X, double Y) P(double x, double y)
        {
            if (double.IsNaN(x) || double.IsNaN(y))
            {
                return (x, y);
            }
            return transformer.Transform2D(new Point(x, y)) is IPoint p ? (p.X, p.Y) : (double.NaN, double.NaN);
        }

        for (int i = 0; i < result.Nodes.Count; i++)
        {
            var n = result.Nodes[i];
            var (x, y) = P(n.X, n.Y);
            result.Nodes[i] = n with { X = x, Y = y };
        }
        for (int i = 0; i < result.Edges.Count; i++)
        {
            var e = result.Edges[i];
            var (fx, fy) = P(e.FromX, e.FromY);
            var (tx, ty) = P(e.ToX, e.ToY);
            result.Edges[i] = e with { FromX = fx, FromY = fy, ToX = tx, ToY = ty };
        }
        for (int i = 0; i < result.Violations.Count; i++)
        {
            var v = result.Violations[i];
            var (x, y) = P(v.X, v.Y);
            result.Violations[i] = v with { X = x, Y = y };
        }
        foreach (var id in edgeCoordinates.Keys.ToList())
        {
            edgeCoordinates[id] = edgeCoordinates[id].Select(c => { var (x, y) = P(c[0], c[1]); return new[] { x, y }; }).ToList();
        }
        result.Srs = to.Name;
    }

    /// <summary>The object ids per layer path of every node (with merged points), edge and violation of the result.</summary>
    public static Dictionary<string, HashSet<long>> ObjectIdsByLayer(NetworkQueryResult result)
    {
        var ids = new Dictionary<string, HashSet<long>>(StringComparer.OrdinalIgnoreCase);
        void Add(string? layer, long? oid)
        {
            if (layer is null || oid is null)
            {
                return;
            }
            if (!ids.TryGetValue(layer, out var set))
            {
                ids[layer] = set = [];
            }
            set.Add(oid.Value);
        }

        foreach (var node in result.Nodes)
        {
            Add(node.Layer, node.Oid);
        }
        foreach (var edge in result.Edges)
        {
            Add(edge.Layer, edge.Oid);
        }
        foreach (var violation in result.Violations)
        {
            Add(violation.Layer, violation.Oid);
            Add(violation.EdgeLayer, violation.EdgeOid);
        }
        return ids;
    }

    /// <summary>The part of a line between two measures (length from its start along all paths); reversed when from &gt; to.</summary>
    public static IReadOnlyList<double[]> Section(IPolyline polyline, double fromMeasure, double toMeasure)
    {
        bool reverse = fromMeasure > toMeasure;
        double start = Math.Min(fromMeasure, toMeasure), end = Math.Max(fromMeasure, toMeasure);
        var result = new List<double[]>();
        double measure = 0;

        for (int p = 0; p < polyline.PathCount; p++)
        {
            var path = polyline[p];
            for (int i = 1; i < path.PointCount; i++)
            {
                var a = path[i - 1];
                var b = path[i];
                double length = Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
                double segmentStart = measure, segmentEnd = measure + length;
                measure = segmentEnd;

                if (segmentEnd < start || segmentStart > end || length == 0)
                {
                    continue;
                }
                if (result.Count == 0)
                {
                    result.Add(At(a, b, (start - segmentStart) / length));
                }
                result.Add(segmentEnd <= end ? [b.X, b.Y] : At(a, b, (end - segmentStart) / length));
                if (segmentEnd >= end)
                {
                    if (reverse)
                    {
                        result.Reverse();
                    }
                    return result;
                }
            }
        }

        if (reverse)
        {
            result.Reverse();
        }
        return result;

        static double[] At(IPoint a, IPoint b, double t)
        {
            t = Math.Clamp(t, 0, 1);
            return [a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t];
        }
    }

    private static async Task<Dictionary<long, IGeometry>> ShapesAsync(
        IFeatureLayer layer, IEnumerable<long> oids, ISpatialReference? srs, IDatumTransformations? datumTransformations, CancellationToken cancellationToken)
    {
        var shapes = new Dictionary<long, IGeometry>();
        var featureClass = layer.FeatureClass;
        foreach (var chunk in oids.Chunk(OidsPerQuery))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var filter = new QueryFilter
            {
                SubFields = $"{featureClass.IDFieldName},{featureClass.ShapeFieldName}",
                WhereClause = $"{featureClass.IDFieldName} in ({string.Join(",", chunk)})"
            };
            if (srs is not null)
            {
                filter.SetFeatureSpatialReference(srs, datumTransformations);
            }

            using IFeatureCursor? cursor = await featureClass.GetFeatures(filter);
            if (cursor is null)
            {
                continue;
            }
            IFeature feature;
            while ((feature = await cursor.NextFeature()) is not null)
            {
                if (feature.Shape is not null)
                {
                    shapes[feature.OID] = feature.Shape;
                }
            }
        }
        return shapes;
    }
}
