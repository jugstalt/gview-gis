#nullable enable

using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Query;
using gView.Server.EndPoints.Network;
using gView.Server.Models.Network;
using gView.Server.Services.Network;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Server.Controllers;

/// <summary>
/// The NetworkServer resource of a map service: the SpatialNET network built for its map (NetworkModule),
///   geoservices/rest/services/[{folder}/]{id}/networkserver        description, links to the operations
///   geoservices/rest/services/[{folder}/]{id}/networkserver/{op}   trace | reach | path | supply | info | validate
/// An operation without parameters shows a form. Needs the service right "network".
/// </summary>
public partial class GeoServicesRestController
{
    private NetworkServerService Network => HttpContext.RequestServices.GetRequiredService<NetworkServerService>();

    public Task<IActionResult> NetworkServer(string id, string folder = "")
        => SecureMethodHandler(identity => WithNetwork(identity, id, folder, (graph, _) =>
        {
            var metadata = graph.Metadata;
            return Task.FromResult(Result(new JsonNetworkServerDTO
            {
                CurrentVersion = Version,
                Name = metadata.Name,
                Created = metadata.Created.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                Srs = metadata.Srs,
                NodeCount = metadata.NodeCount,
                EdgeCount = metadata.EdgeCount,
                MergedPointCount = metadata.AliasCount,
                ShardCount = metadata.Shards.Count,
                Operations = NetworkQueryService.Operations.ToArray(),
                NodeLayers = metadata.FeatureClasses.Where(f => f.Kind == SpatialNET.Network.ElementKind.Node).Select(f => f.Layer).ToArray(),
                EdgeLayers = metadata.FeatureClasses.Where(f => f.Kind == SpatialNET.Network.ElementKind.Edge).Select(f => f.Layer).ToArray(),
                Weights = new[] { "length" }.Concat(metadata.Weights.Select(w => w.Name)).ToArray(),
                Types = metadata.Types.Where(t => t.Used).Select(t => new JsonNetworkServerDTO.TypeInfo { Id = t.Id, Kind = t.Kind.ToString(), Name = t.Name }).ToArray(),
                Tiers = graph.Tiers.Select(t => new JsonNetworkServerDTO.TierInfo { Name = t.Name, Order = t.Rank, Filter = t.Pattern(graph.Types) }).ToArray(),
                Rules = metadata.Rules.Select(r => new JsonNetworkServerDTO.RuleInfo { Id = r.Id, Severity = r.Severity.ToString(), Description = r.Description }).ToArray(),
            }, folder, id, "NetworkServer"));
        }, search: false));

    public Task<IActionResult> NetworkServerOperation(string id, string op, string folder = "")
        => SecureMethodHandler(async identity =>
        {
            // query, form or JSON body (also f)
            var parameters = await NetworkSearch.Parameters(Request);
            bool search = NetworkSearch.HasSearchParameters(parameters);

            return await WithNetwork(identity, id, folder, async (graph, loadMap) =>
            {
                // an operation without parameters shows the HTML form
                if (!search)
                {
                    object? form = op.ToLowerInvariant() switch
                    {
                        "trace" => new JsonNetworkTraceDTO(),
                        "reach" => new JsonNetworkReachDTO(),
                        "path" => new JsonNetworkPathDTO(),
                        "supply" => new JsonNetworkSupplyDTO(),
                        "outage" => new JsonNetworkOutageDTO(),
                        "feeders" => new JsonNetworkFeedersDTO(),
                        "info" => new JsonNetworkInfoDTO(),
                        "nearest" => new JsonNetworkNearestDTO(),
                        "validate" => new JsonNetworkValidateDTO(),
                        _ => null
                    };
                    if (form is not null)
                    {
                        return FormResult(form);
                    }
                }

                var mapService = _mapServerService.Instance.GetMapService(id, folder);
                bool mayReadAttributes = mapService is not null && (await mapService.GetAccessTypes(identity)).HasFlag(AccessTypes.Query);
                var (content, contentType) = await NetworkSearch.RunAsync(Network, graph, op, parameters, loadMap, HttpContext.RequestAborted, mayReadAttributes);
                return Content(content, contentType, Encoding.UTF8);
            }, search);
        });

    /// <summary>
    /// Checks the right "network" and finds the built network. A <paramref name="search"/> waits for one of the searches
    /// that may run at the same time (Network:MaxParallel), at most Network:TimeoutSeconds; the description and the forms don't.
    /// </summary>
    private async Task<IActionResult> WithNetwork(
        Identity identity, string id, string folder,
        Func<FileNetworkGraph, Func<Task<IMap?>>, Task<IActionResult>> action, bool search = true)
    {
        var mapService = _mapServerService.Instance.GetMapService(id, folder)
            ?? throw new MapServerException($"Unknown service: {id}");

        await mapService.CheckAccess(identity, _mapServerService.GetInterpreter(typeof(Interoperability.GeoServices.Request.GeoServicesRestInterperter)));
        if (!(await mapService.GetAccessTypes(identity)).HasFlag(AccessTypes.Network))
        {
            throw new NotAuthorizedException($"Forbidden: Network access required (user: {identity.UserName})");
        }

        var network = Network;
        var directory = network.NetworkDirectory(mapService)
            ?? throw new MapServerException("The service has no network.");
        if (!NetworkServerService.IsBuilt(directory))
        {
            throw new MapServerException("The network of the service is not built.");
        }

        IServiceMap? serviceMap = null;
        if (search && !await network.Searches.WaitAsync(TimeSpan.FromSeconds(Math.Max(1, network.Options.TimeoutSeconds)), HttpContext.RequestAborted))
        {
            throw new MapServerException("Too many network searches at the same time, try again later.") { StatusCode = 503 };
        }
        try
        {
            async Task<IMap?> LoadMap()
            {
                serviceMap ??= await _mapServerService.Instance.GetServiceMapAsync(mapService);
                return serviceMap as IMap;
            }

            return await action(NetworkGraphCache.Shared.Get(directory), LoadMap);
        }
        catch (NetworkRebuildRequiredException)
        {
            // a network built by an older version lacks the index files (spatial.idx, ...); the message names no server path
            throw new MapServerException("The network of the service was built by an older version: rebuild network.");
        }
        finally
        {
            serviceMap?.Dispose();
            if (search)
            {
                network.Searches.Release();
            }
        }
    }
}
