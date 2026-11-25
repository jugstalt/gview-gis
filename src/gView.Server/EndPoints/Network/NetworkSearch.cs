#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Query;
using gView.Server.Services.Network;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace gView.Server.EndPoints.Network;

/// <summary>
/// Runs a network search for the NetworkServer resource
/// (geoservices/rest/services/[{folder}/]{id}/networkserver/{op}, see GeoServicesRestController):
/// parameters start, from, to, sources, sinks, branches (feeders), node, filter, block, unblock, disable (edges), weight, maxCost, rules (ids),
/// x, y, radius, count (nearest), area (searches and validate: only the nodes inside take part; xmin,ymin,xmax,ymax,
/// GeoJSON or Esri JSON polygon, WKT POLYGON/MULTIPOLYGON or shard:key1,key2),
/// inSR (EPSG code of x/y, the @x,y node references and the area; default the network's),
/// geometry (true: the real parts of the line features), snap (true: line ends set onto the nodes), attributes (true: the fields of the features; needs the right
/// "query"), outFields (comma separated, default all; per layer only the fields it has), outSR (EPSG code), f = json | pjson | geojson | csv.
/// Limits from appsettings "Network" (see <see cref="NetworkServerOptions"/>).
/// </summary>
public static class NetworkSearch
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    /// <summary>
    /// Whether the request asks for a result: an <c>f</c> or any search parameter (not only the token); otherwise the form
    /// is shown.
    /// </summary>
    public static bool HasSearchParameters(IReadOnlyDictionary<string, string> parameters)
        => parameters.Any(p => !p.Key.Equals("token", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(p.Value));

    /// <summary>The result as text with its content type.</summary>
    public static async Task<(string Content, string ContentType)> RunAsync(
        NetworkServerService network, FileNetworkGraph graph, string op, IReadOnlyDictionary<string, string> parameters,
        Func<Task<IMap?>> loadMap, CancellationToken cancellationToken, bool mayReadAttributes = false)
    {
        var operation = op.ToLowerInvariant();
        if (!NetworkQueryService.Operations.Contains(operation))
        {
            throw new MapServerException($"Unknown network operation '{op}'. Operations: {string.Join(", ", NetworkQueryService.Operations)}");
        }

        string? P(string name) => parameters.TryGetValue(name, out var v) && !string.IsNullOrWhiteSpace(v) ? v : null;

        double? Number(string name)
        {
            var value = P(name);
            if (value is null)
            {
                return null;
            }
            return double.TryParse(value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                ? number
                : throw new MapServerException($"{name}: '{value}' is no number.");
        }

        // inSR: x/y and the @x,y node references are given in another spatial reference than the network
        Func<double, double, (double X, double Y)> toNetwork = (x, y) => (x, y);
        using var inTransformer = P("inSR") is string inSR ? GeometricTransformerFactory.Create((await loadMap())?.Display?.DatumTransformations) : null;
        if (inTransformer is not null)
        {
            var inSRText = P("inSR")!;
            var from = SpatialReference.FromID(inSRText.Contains(':') ? inSRText : $"epsg:{inSRText}")
                       ?? throw new MapServerException($"Unknown spatial reference {inSRText}");
            var to = (string.IsNullOrEmpty(graph.Metadata.Srs) ? null : SpatialReference.FromID(graph.Metadata.Srs))
                     ?? throw new MapServerException("The network has no spatial reference, inSR can't be used.");
            inTransformer.SetSpatialReferences(from, to);
            toNetwork = (x, y) => inTransformer.Transform2D(new Point(x, y)) is IPoint p ? (p.X, p.Y) : throw new MapServerException($"{x}, {y} can't be projected.");
        }
        string? Nodes(string name) => P(name) is string value ? NetworkLocations.Transform(value, toNetwork) : null;

        NetworkArea? area;
        try
        {
            area = P("area") is string areaText ? NetworkArea.Parse(areaText) : null;
        }
        catch (ArgumentException ex)
        {
            throw new MapServerException(ex.Message);
        }
        if (area is not null && inTransformer is not null)
        {
            area = area.Transform(toNetwork);
        }

        var (x, y) = Number("x") is double x0 && Number("y") is double y0 ? toNetwork(x0, y0) : (double.NaN, double.NaN);
        var maxCost = Number("maxCost") ?? Number("max-cost");
        var request = new NetworkQueryRequest
        {
            Start = Nodes("start"),
            From = Nodes("from"),
            To = Nodes("to"),
            Sources = Nodes("sources"),
            Sinks = Nodes("sinks"),
            Branches = P("branches"),
            OpenEdges = "true".Equals(P("openEdges"), StringComparison.OrdinalIgnoreCase),
            Node = Nodes("node"),
            Filter = P("filter"),
            Tier = P("tier"),
            StopAtSources = P("stopAtSources") is string stop ? !stop.Equals("false", StringComparison.OrdinalIgnoreCase) : null,
            Block = Nodes("block"),
            Unblock = Nodes("unblock"),
            Disable = P("disable"),
            Weight = P("weight"),
            MaxCost = maxCost,
            RuleIds = P("rules") ?? P("rule"),
            X = double.IsNaN(x) ? null : x,
            Y = double.IsNaN(y) ? null : y,
            Radius = Number("radius"),
            Count = Number("count") is double count ? (int)count : null,
            Area = area,
        };

        NetworkQueryResult result;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, network.Options.TimeoutSeconds)));
        try
        {
            // searches without cancellation keep running in the background after the timeout, the answer is an error
            result = await Task.Run(() => new NetworkQueryService(graph).Run(operation, request, cancellationToken: timeout.Token), timeout.Token)
                               .WaitAsync(timeout.Token);
        }
        catch (ArgumentException ex)
        {
            throw new MapServerException(ex.Message);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new MapServerException($"The search took longer than {network.Options.TimeoutSeconds} s.") { StatusCode = 504 };
        }
        result.Truncate(network.Options.MaxResultNodes);

        var format = (P("f") ?? "json").ToLowerInvariant();
        if (format == "csv")
        {
            return (NetworkResultWriter.ToCsv(result), "text/csv");
        }

        var withGeometry = !"false".Equals(P("geometry"), StringComparison.OrdinalIgnoreCase);
        var withAttributes = "true".Equals(P("attributes"), StringComparison.OrdinalIgnoreCase);
        if (withAttributes && !mayReadAttributes)
        {
            throw new NotAuthorizedException("Forbidden: attributes need the right Query");
        }

        // the geometry and the attributes of the edges are read from the feature classes: within the same time limit
        IMap? map = null;
        Dictionary<string, Dictionary<long, Dictionary<string, object?>>>? attributes = null;
        Dictionary<long, IReadOnlyList<double[]>> edgeCoordinates;
        try
        {
            map = withGeometry && result.Edges.Count > 0 ? await loadMap() : null;
            edgeCoordinates = map is not null
                ? await NetworkResultGeometry.EdgeCoordinatesAsync(map, result, timeout.Token,
                    snap: "true".Equals(P("snap"), StringComparison.OrdinalIgnoreCase), tolerance: graph.Metadata.Tolerance)
                : [];

            if (withAttributes && (map ?? await loadMap()) is IMap attributeMap)
            {
                attributes = await NetworkResultGeometry.AttributesAsync(attributeMap, result, P("outFields"), timeout.Token);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new MapServerException($"Reading the geometry and attributes of the result took longer than {network.Options.TimeoutSeconds} s.") { StatusCode = 504 };
        }
        IReadOnlyDictionary<string, object?>? Attributes(string? layer, long? oid)
            => attributes is not null && layer is not null && oid is long id
               && attributes.TryGetValue(layer, out var byOid) && byOid.TryGetValue(id, out var values)
                ? values
                : null;

        if (P("outSR") is string outSR)
        {
            var to = SpatialReference.FromID(outSR.Contains(':') ? outSR : $"epsg:{outSR}")
                     ?? throw new MapServerException($"Unknown spatial reference {outSR}");
            var from = (string.IsNullOrEmpty(result.Srs) ? null : SpatialReference.FromID(result.Srs))
                       ?? throw new MapServerException("The network has no spatial reference, outSR can't be used.");
            NetworkResultGeometry.Project(result, edgeCoordinates, from, to, (map ?? await loadMap())?.Display?.DatumTransformations);
        }

        if (format == "geojson")
        {
            return (NetworkResultWriter.ToGeoJson(result, e => edgeCoordinates.TryGetValue(e.Id, out var c) ? c : null, Attributes), "application/geo+json");
        }

        var options = format == "pjson" ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true } : JsonOptions;
        return (JsonSerializer.Serialize(new
        {
            result.Operation,
            result.Success,
            result.ExceededLimit,
            result.Cost,
            result.Srs,
            result.Messages,
            Nodes = result.Nodes.Select(n => new
            {
                n.Id, n.Label, n.Layer, n.Oid, n.Type, n.Kind, n.Cost, n.Source, n.AliasOf, n.X, n.Y,
                Attributes = Attributes(n.Layer, n.Oid)
            }),
            Edges = result.Edges.Select(e => new
            {
                e.Id, e.Label, e.Kind, e.Layer, e.Oid, e.Type, e.Length, e.FromMeasure, e.ToMeasure, e.From, e.To,
                e.Source, e.Branch, e.Cost,
                Coordinates = edgeCoordinates.TryGetValue(e.Id, out var c) ? c : (IReadOnlyList<double[]>)[[e.FromX, e.FromY], [e.ToX, e.ToY]],
                Attributes = Attributes(e.Layer, e.Oid)
            }),
            Violations = result.Violations.Select(v => new
            {
                v.Rule, v.Severity, v.Message, v.Kind, v.Node, v.NodeLabel, v.Layer, v.Oid,
                v.Edge, v.EdgeLabel, v.EdgeLayer, v.EdgeOid, v.Type, v.Count, v.Min, v.Max, v.X, v.Y, v.Aliases,
                Attributes = Attributes(v.Layer, v.Oid),
                EdgeAttributes = Attributes(v.EdgeLayer, v.EdgeOid)
            }),
            result.Rules,
            ObjectIds = NetworkResultGeometry.ObjectIdsByLayer(result),
        }, options), "application/json");
    }

    /// <summary>Query, form and JSON body parameters.</summary>
    public static async Task<Dictionary<string, string>> Parameters(HttpRequest request)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in request.Query)
        {
            parameters[key] = value.ToString();
        }
        if (request.HasFormContentType)
        {
            foreach (var (key, value) in await request.ReadFormAsync())
            {
                parameters[key] = value.ToString();
            }
        }
        else if (request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            using var document = await JsonDocument.ParseAsync(request.Body);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    parameters[property.Name] = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() ?? "" : property.Value.GetRawText();
                }
            }
        }
        return parameters;
    }
}
