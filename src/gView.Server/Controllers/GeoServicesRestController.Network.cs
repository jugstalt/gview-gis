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
                Tiers = metadata.Tiers.OrderBy(t => t.Order).Select(t => new JsonNetworkServerDTO.TierInfo { Name = t.Name, Order = t.Order, Filter = t.Filter }).ToArray(),
                Rules = metadata.Rules.Select(r => new JsonNetworkServerDTO.RuleInfo { Id = r.Id, Severity = r.Severity.ToString(), Description = r.Description }).ToArray(),
            }, folder, id, "NetworkServer"));
        }));

    public Task<IActionResult> NetworkServerOperation(string id, string op, string folder = "")
        => SecureMethodHandler(identity => WithNetwork(identity, id, folder, async (graph, loadMap) =>
        {
            var parameters = await NetworkSearch.Parameters(Request);
            // like the other resources: without f the HTML form
            if (ResultFormat() == "")
            {
                object? form = op.ToLowerInvariant() switch
                {
                    "trace" => new JsonNetworkTraceDTO(),
                    "reach" => new JsonNetworkReachDTO(),
                    "path" => new JsonNetworkPathDTO(),
                    "supply" => new JsonNetworkSupplyDTO(),
                    "outage" => new JsonNetworkOutageDTO(),
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
        }));

    /// <summary>Checks the right "network", finds the built network and limits the searches running at the same time.</summary>
    private async Task<IActionResult> WithNetwork(
        Identity identity, string id, string folder,
        Func<FileNetworkGraph, Func<Task<IMap?>>, Task<IActionResult>> action)
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
        await network.Searches.WaitAsync(HttpContext.RequestAborted);
        try
        {
            async Task<IMap?> LoadMap()
            {
                serviceMap ??= await _mapServerService.Instance.GetServiceMapAsync(mapService);
                return serviceMap as IMap;
            }

            return await action(NetworkGraphCache.Shared.Get(directory), LoadMap);
        }
        finally
        {
            serviceMap?.Dispose();
            network.Searches.Release();
        }
    }
}
