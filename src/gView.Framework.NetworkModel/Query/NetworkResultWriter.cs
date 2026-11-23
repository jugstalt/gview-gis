using System.Globalization;
using System.Text;
using System.Text.Json;

namespace gView.Framework.NetworkModel.Query;

/// <summary>Writes a <see cref="NetworkQueryResult"/> as CSV (one row per node or violation, per edge for feeders) or GeoJSON.</summary>
public static class NetworkResultWriter
{
    /// <summary>The coordinates of an edge (x, y pairs); null for a straight line between its nodes.</summary>
    public delegate IReadOnlyList<double[]>? EdgeCoordinates(NetworkResultEdge edge);

    public static bool IsGeoJson(string path)
        => path.EndsWith(".geojson", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

    public static void Write(string path, NetworkQueryResult result)
        => File.WriteAllText(path, IsGeoJson(path) ? ToGeoJson(result) : ToCsv(result));

    public static string ToCsv(NetworkQueryResult result)
    {
        var sb = new StringBuilder();
        if (result.Operation == "validate")
        {
            sb.Append("rule;severity;kind;node;edge;type;count;min;max;x;y;aliases\n");
            foreach (var v in result.Violations)
            {
                sb.Append(v.Rule).Append(';').Append(v.Severity).Append(';').Append(v.Kind).Append(';')
                  .Append(v.NodeLabel).Append(';').Append(v.EdgeLabel).Append(';').Append(v.Type).Append(';')
                  .Append(v.Count).Append(';').Append(v.Min).Append(';').Append(v.Max?.ToString(CultureInfo.InvariantCulture) ?? "").Append(';')
                  .Append(v.X.ToString("0.###", CultureInfo.InvariantCulture)).Append(';')
                  .Append(v.Y.ToString("0.###", CultureInfo.InvariantCulture)).Append(';')
                  .Append(string.Join(";", v.Aliases)).Append('\n');
            }
            return sb.ToString();
        }

        if (result.Operation == "feeders")
        {
            sb.Append("id;edge;layer;oid;type;kind;source;branch;cost;length\n");
            foreach (var e in result.Edges)
            {
                sb.Append(e.Id).Append(';').Append(e.Label).Append(';').Append(e.Layer).Append(';').Append(e.Oid).Append(';')
                  .Append(e.Type).Append(';').Append(e.Kind).Append(';').Append(e.Source).Append(';').Append(e.Branch).Append(';')
                  .Append(e.Cost?.ToString("0.###", CultureInfo.InvariantCulture)).Append(';')
                  .Append(e.Length.ToString("0.###", CultureInfo.InvariantCulture)).Append('\n');
            }
            return sb.ToString();
        }

        sb.Append("id;node;type;kind;cost;source;alias_of\n");
        foreach (var n in result.Nodes)
        {
            sb.Append(n.Id).Append(';').Append(n.Label).Append(';').Append(n.Type).Append(';')
              .Append(n.Kind).Append(';').Append(n.Cost.ToString("0.###", CultureInfo.InvariantCulture)).Append(';').Append(n.Source).Append(';')
              .Append(n.AliasOf).Append('\n');
        }
        return sb.ToString();
    }

    /// <summary>Nodes and violations as points, edges as lines (straight, or from <paramref name="edgeCoordinates"/>).</summary>
    /// <summary>The attributes of a feature (layer path, object id); null for none.</summary>
    public delegate IReadOnlyDictionary<string, object?>? FeatureAttributes(string? layer, long? oid);

    public static string ToGeoJson(NetworkQueryResult result, EdgeCoordinates? edgeCoordinates = null, FeatureAttributes? attributes = null)
    {
        var features = new List<object>();
        foreach (var n in result.Nodes.Where(n => !double.IsNaN(n.X)))
        {
            features.Add(new
            {
                type = "Feature",
                geometry = new { type = "Point", coordinates = new[] { n.X, n.Y } },
                properties = new { kind = n.Kind, id = n.Id.ToString(), node = n.Label, layer = n.Layer, oid = n.Oid, type = n.Type, cost = n.Cost, source = n.Source, alias_of = n.AliasOf, attributes = attributes?.Invoke(n.Layer, n.Oid) }
            });
        }
        foreach (var e in result.Edges)
        {
            var coordinates = edgeCoordinates?.Invoke(e) ?? [[e.FromX, e.FromY], [e.ToX, e.ToY]];
            features.Add(new
            {
                type = "Feature",
                geometry = new { type = "LineString", coordinates },
                properties = new { kind = e.Kind, id = e.Id.ToString(), edge = e.Label, layer = e.Layer, oid = e.Oid, type = e.Type, length = e.Length, from_measure = e.FromMeasure, to_measure = e.ToMeasure, from_node = e.From.ToString(), to_node = e.To.ToString(), source = e.Source, branch = e.Branch, cost = e.Cost, attributes = attributes?.Invoke(e.Layer, e.Oid) }
            });
        }
        foreach (var v in result.Violations)
        {
            features.Add(new
            {
                type = "Feature",
                geometry = new { type = "Point", coordinates = new[] { v.X, v.Y } },
                properties = new
                {
                    rule = v.Rule,
                    severity = v.Severity,
                    message = v.Message,
                    kind = v.Kind,
                    node = v.NodeLabel,
                    layer = v.Layer,
                    oid = v.Oid,
                    edge = v.EdgeLabel ?? "",
                    edge_layer = v.EdgeLayer,
                    edge_oid = v.EdgeOid,
                    type = v.Type,
                    count = v.Count,
                    min = v.Min,
                    max = v.Max,
                    aliases = string.Join(";", v.Aliases),
                    attributes = attributes?.Invoke(v.Layer, v.Oid),
                    edge_attributes = attributes?.Invoke(v.EdgeLayer, v.EdgeOid)
                }
            });
        }
        return JsonSerializer.Serialize(new
        {
            type = "FeatureCollection",
            crs = result.Srs is null ? null : new { type = "name", properties = new { name = result.Srs } },
            features
        });
    }
}
