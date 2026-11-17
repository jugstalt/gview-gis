#nullable enable

using gView.Endpoints.Abstractions;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Framework.Geometry;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Module;
using gView.Framework.NetworkModel.Query;
using gView.Interoperability.GeoServices.Request;
using gView.Server.AppCode;
using gView.Server.Services.MapServer;
using gView.Server.Services.Network;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace gView.Server.EndPoints.Network;

/// <summary>
/// Network searches on the network built for a map service (NetworkModule in the mxl, network directory next to it):
///   GET          geoservices/rest/services/[{folder}/]{id}/mapserver/network           what the network offers
///   GET | POST   geoservices/rest/services/[{folder}/]{id}/mapserver/network/{op}      trace | reach | path | supply | info | validate
/// Parameters (query or form): start, from, to, sources, sinks, node, filter, block, unblock, weight, maxCost, rules (ids),
/// geometry (true: the real parts of the line features, false: straight lines), outSR (EPSG code), f = json | geojson | csv.
/// Limits from appsettings "Network" (see <see cref="NetworkServerOptions"/>).
/// Needs the service right "network".
/// </summary>
public class NetworkSearch : IApiEndpoint
{
    private const string Base = "geoservices/rest/services";

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    public void Register(IEndpointRouteBuilder app)
    {
        app.MapGet($"{Base}/{{folder}}/{{id}}/mapserver/network", Describe);
        app.MapGet($"{Base}/{{id}}/mapserver/network", (HttpContext c, [FromServices] MapServiceManager m, [FromServices] LoginManager l, [FromServices] ILogger<NetworkSearch> g, [FromServices] NetworkServerService n, string id)
            => Describe(c, m, l, g, n, "", id));
        app.MapMethods($"{Base}/{{folder}}/{{id}}/mapserver/network/{{op}}", ["GET", "POST"], Search);
        app.MapMethods($"{Base}/{{id}}/mapserver/network/{{op}}", ["GET", "POST"], (HttpContext c, [FromServices] MapServiceManager m, [FromServices] LoginManager l, [FromServices] ILogger<NetworkSearch> g, [FromServices] NetworkServerService n, string id, string op)
            => Search(c, m, l, g, n, "", id, op));
    }

    private static Task<IResult> Describe(
        HttpContext httpContext, [FromServices] MapServiceManager mapServiceManager, [FromServices] LoginManager loginManager,
        [FromServices] ILogger<NetworkSearch> logger, [FromServices] NetworkServerService network, string folder, string id)
        => HandleAsync(httpContext, mapServiceManager, loginManager, logger, network, folder, id, "network", (graph, _) =>
        {
            var metadata = graph.Metadata;
            return Task.FromResult(Results.Text(JsonSerializer.Serialize(new
            {
                metadata.Name,
                metadata.Created,
                metadata.Srs,
                metadata.NodeCount,
                metadata.EdgeCount,
                metadata.AliasCount,
                Shards = metadata.Shards.Count,
                Operations = NetworkQueryService.Operations,
                NodeLayers = metadata.FeatureClasses.Where(f => f.Kind == SpatialNET.Network.ElementKind.Node).Select(f => f.Layer),
                EdgeLayers = metadata.FeatureClasses.Where(f => f.Kind == SpatialNET.Network.ElementKind.Edge).Select(f => f.Layer),
                Weights = new[] { "length" }.Concat(metadata.Weights.Select(w => w.Name)),
                Types = metadata.Types.Where(t => t.Used).Select(t => new { t.Id, Kind = t.Kind.ToString(), t.Name }),
                Rules = metadata.Rules.Select(r => new { r.Id, Severity = r.Severity.ToString(), r.Description }),
            }, JsonOptions), "application/json", Encoding.UTF8));
        });

    private static Task<IResult> Search(
        HttpContext httpContext, [FromServices] MapServiceManager mapServiceManager, [FromServices] LoginManager loginManager,
        [FromServices] ILogger<NetworkSearch> logger, [FromServices] NetworkServerService network, string folder, string id, string op)
        => HandleAsync(httpContext, mapServiceManager, loginManager, logger, network, folder, id, op, async (graph, loadMap) =>
        {
            var operation = op.ToLowerInvariant();
            if (!NetworkQueryService.Operations.Contains(operation))
            {
                throw new MapServerException($"Unknown network operation '{op}'. Operations: {string.Join(", ", NetworkQueryService.Operations)}");
            }

            var parameters = await Parameters(httpContext.Request);
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
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(httpContext.RequestAborted);
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
            catch (OperationCanceledException) when (!httpContext.RequestAborted.IsCancellationRequested)
            {
                throw new MapServerException($"The search took longer than {network.Options.TimeoutSeconds} s.") { StatusCode = 504 };
            }
            result.Truncate(network.Options.MaxResultNodes);

            var format = (P("f") ?? "json").ToLowerInvariant();
            if (format == "csv")
            {
                return Results.Text(NetworkResultWriter.ToCsv(result), "text/csv", Encoding.UTF8);
            }

            var withGeometry = !"false".Equals(P("geometry"), StringComparison.OrdinalIgnoreCase);
            var map = withGeometry && result.Edges.Count > 0 ? await loadMap() : null;
            var edgeCoordinates = map is not null
                ? await NetworkResultGeometry.EdgeCoordinatesAsync(map, result, httpContext.RequestAborted)
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
                return Results.Text(
                    NetworkResultWriter.ToGeoJson(result, e => edgeCoordinates.TryGetValue(e.Id, out var c) ? c : null),
                    "application/geo+json", Encoding.UTF8);
            }

            return Results.Text(JsonSerializer.Serialize(new
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
            }, JsonOptions), "application/json", Encoding.UTF8);
        });

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext, MapServiceManager mapServiceManager, LoginManager loginManager, ILogger logger, NetworkServerService network,
        string folder, string id, string operation, Func<FileNetworkGraph, Func<Task<IMap?>>, Task<IResult>> action)
    {
        IServiceMap? serviceMap = null;
        bool entered = false;
        try
        {
            var authToken = loginManager.GetAuthToken(httpContext.Request);
            var identity = new Identity(authToken.Username, authToken.IsManageUser);

            var mapService = mapServiceManager.Instance.GetMapService(id, folder)
                ?? throw new MapServerException("Unknown service");

            await mapService.CheckAccess(identity, mapServiceManager.GetInterpreter(typeof(GeoServicesRestInterperter)));
            if (!(await mapService.GetAccessTypes(identity)).HasFlag(AccessTypes.Network))
            {
                throw new NotAuthorizedException($"Forbidden: Network access required (user: {identity.UserName})");
            }

            var directory = network.NetworkDirectory(mapService)
                ?? throw new MapServerException("The service has no network.") { StatusCode = 404 };
            if (!NetworkServerService.IsBuilt(directory))
            {
                throw new MapServerException("The network of the service is not built.") { StatusCode = 404 };
            }

            // searches have their own limit, so long searches don't take the slots of map requests
            await network.Searches.WaitAsync(httpContext.RequestAborted);
            entered = true;

            async Task<IMap?> LoadMap()
            {
                serviceMap ??= await mapServiceManager.Instance.GetServiceMapAsync(mapService);
                return serviceMap as IMap;
            }

            return await action(NetworkGraphCache.Shared.Get(directory), LoadMap);
        }
        catch (MapServerAuthException)
        {
            throw; // Handled in AuthenticationExceptionMiddleware
        }
        catch (MapServerException mse)
        {
            logger.LogWarning("Network request: {message}", mse.Message);
            return Results.Json(new { error = new { code = mse.StatusCode ?? 400, message = mse.Message } }, statusCode: mse.StatusCode ?? 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Network request: {message}", ex.Message);
            return Results.Json(new { error = new { code = 500, message = "internal server error" } }, statusCode: 500);
        }
        finally
        {
            serviceMap?.Dispose();
            if (entered)
            {
                network.Searches.Release();
            }
        }
    }

    private static async Task<Dictionary<string, string>> Parameters(HttpRequest request)
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
