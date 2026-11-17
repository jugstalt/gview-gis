#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Exceptions;
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
/// parameters start, from, to, sources, sinks, node, filter, block, unblock, weight, maxCost, rules (ids),
/// geometry (true: the real parts of the line features), outSR (EPSG code), f = json | pjson | geojson | csv.
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

    /// <summary>Whether the request carries anything but f (else the form is shown).</summary>
    public static bool HasSearchParameters(IReadOnlyDictionary<string, string> parameters)
        => parameters.Any(p => !p.Key.Equals("f", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(p.Value));

    /// <summary>The result as text with its content type.</summary>
    public static async Task<(string Content, string ContentType)> RunAsync(
        NetworkServerService network, FileNetworkGraph graph, string op, IReadOnlyDictionary<string, string> parameters,
        Func<Task<IMap?>> loadMap, CancellationToken cancellationToken)
    {
        var operation = op.ToLowerInvariant();
        if (!NetworkQueryService.Operations.Contains(operation))
        {
            throw new MapServerException($"Unknown network operation '{op}'. Operations: {string.Join(", ", NetworkQueryService.Operations)}");
        }

        string? P(string name) => parameters.TryGetValue(name, out var v) && !string.IsNullOrWhiteSpace(v) ? v : null;

        var maxCost = P("maxCost") ?? P("max-cost");
        var request = new NetworkQueryRequest
        {
            Start = P("start"),
            From = P("from"),
            To = P("to"),
            Sources = P("sources"),
            Sinks = P("sinks"),
            Node = P("node"),
            Filter = P("filter"),
            Block = P("block"),
            Unblock = P("unblock"),
            Weight = P("weight"),
            MaxCost = maxCost is null ? null : double.Parse(maxCost.Replace(',', '.'), CultureInfo.InvariantCulture),
            RuleIds = P("rules") ?? P("rule"),
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
        var map = withGeometry && result.Edges.Count > 0 ? await loadMap() : null;
        var edgeCoordinates = map is not null
            ? await NetworkResultGeometry.EdgeCoordinatesAsync(map, result, cancellationToken)
            : [];

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
            return (NetworkResultWriter.ToGeoJson(result, e => edgeCoordinates.TryGetValue(e.Id, out var c) ? c : null), "application/geo+json");
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
            result.Nodes,
            Edges = result.Edges.Select(e => new
            {
                e.Id, e.Label, e.Layer, e.Oid, e.Type, e.Length, e.FromMeasure, e.ToMeasure, e.From, e.To,
                Coordinates = edgeCoordinates.TryGetValue(e.Id, out var c) ? c : (IReadOnlyList<double[]>)[[e.FromX, e.FromY], [e.ToX, e.ToY]]
            }),
            result.Violations,
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
