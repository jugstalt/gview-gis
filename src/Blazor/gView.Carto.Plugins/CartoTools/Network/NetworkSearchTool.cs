using gView.Carto.Core;
using gView.Carto.Core.Extensions;
using gView.Carto.Core.Models.ToolEvents;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.CartoTools.Network.Graphics;
using gView.Carto.Razor.Components.Tools.Context;
using gView.Carto.Razor.Components.Tools.ToolEvents;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.NetworkModel.Build;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Module;
using gView.Framework.NetworkModel.Query;
using gView.Framework.NetworkModel.Store;
using gView.GraphicsEngine;
using System.Globalization;

namespace gView.Carto.Plugins.CartoTools.Network;

/// <summary>
/// Searches on the SpatialNET network of the map (NetworkModule, built into its network directory): click a node,
/// choose trace, reach, path, supply, info or validate; the result is drawn with the real line parts and can become
/// the selection or a GeoJSON file.
/// </summary>
[RegisterPlugIn("B3A1C7E2-5D84-4F0B-9E61-2C7A9D5F3B18")]
internal class NetworkSearchTool : ICartoTool
{
    private static readonly ArgbColor EdgeColor = ArgbColor.FromArgb(200, 0, 120, 255);
    private static readonly ArgbColor NodeColor = ArgbColor.FromArgb(230, 0, 90, 200);
    private static readonly ArgbColor ProblemColor = ArgbColor.FromArgb(230, 220, 30, 30);

    private NetworkSearchContext? _context;

    #region ICartoButton

    public string Name => "Network Search";

    public string ToolTip => "Search the network of the map (trace, path, supply, validate)";

    public ToolType ToolType => ToolType.Click;

    public string Icon => "webgis:construct-edge-intersect";

    public CartoToolTarget Target => CartoToolTarget.Tools;

    public int SortOrder => 11;

    public void Dispose() { }

    public bool IsVisible(ICartoApplicationScopeService scope)
        => scope.Modules.OfType<NetworkModule>().FirstOrDefault()?.IsConfigured == true;

    public bool IsDisabled(ICartoApplicationScopeService scope) => false;

    public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        if (_context is not null)
        {
            _context.Error = null;
            Refresh(scope);
        }
        return Task.FromResult(true);
    }

    /// <summary>Reads the status of the built network and its weights (Error when it is not built).</summary>
    private void Refresh(ICartoApplicationScopeService scope)
    {
        var module = scope.Modules.OfType<NetworkModule>().FirstOrDefault(m => m.IsConfigured);
        _context!.Build = module?.BuildState;
        _context.Status = module?.GetStatus(scope.Document.FilePath);
        try
        {
            var graph = Graph(scope);
            _context.Weights = new[] { "length" }.Concat(graph.Metadata.Weights.Select(w => w.Name)).ToList();
        }
        catch (Exception ex)
        {
            _context.Error = ex.Message;
        }
    }

    #endregion

    #region ICartoTool

    public object? ToolContext => _context;

    public string ToolBoxTitle(ICartoApplicationScopeService scope) => "Network Search";

    public void InitializeScope(ICartoApplicationScopeService scope)
    {
        _context = new NetworkSearchContext();
    }

    public Type UIComponent => typeof(Razor.Components.Tools.NetworkSearch);

    public async Task<bool> OnEvent(ICartoApplicationScopeService scope, ToolEventArgs eventArgs)
    {
        if (_context is null)
        {
            return false;
        }

        try
        {
            _context.Error = null;
            return eventArgs switch
            {
                NetworkSearchEventArgs { Action: NetworkSearchAction.Run } => await Run(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.Clear } => await Clear(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ToSelection } => await ToSelection(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ExportGeoJson } => await ExportGeoJson(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.Build } => Build(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ZoomToViolation } a => await ZoomToViolation(scope, a.Index),
                _ => await PickNode(scope, eventArgs)
            };
        }
        catch (Exception ex)
        {
            _context.Error = ex.Message;
            _context.Busy = false;
            return false;
        }
        finally
        {
            _context.NotifyChanged();
        }
    }

    #endregion

    #region Actions

    /// <summary>Snaps the click to the nearest point of the node layers and sets it as start or target.</summary>
    private async Task<bool> PickNode(ICartoApplicationScopeService scope, ToolEventArgs eventArgs)
    {
        if (_context!.Pick == NetworkSearchPick.None || scope.DisplayService.SpatialReference is null)
        {
            return false;
        }

        var envelope = eventArgs.GetGeometry().ToProjectedEnvelope(scope, scope.DisplayService.SpatialReference);
        if (envelope is null)
        {
            return false;
        }

        var module = Module(scope);
        var layers = MapLayers.FeatureLayers(scope.Document.Map);
        var nodeLayerIds = module.Config.Nodes.Select(n => n.LayerId).OfType<int>().ToHashSet();

        string? best = null;
        IPoint? bestPoint = null;
        double bestDistance = double.MaxValue;
        foreach (var layer in layers.Where(l => nodeLayerIds.Contains(l.Id)))
        {
            var featureClass = layer.Layer.FeatureClass;
            var filter = new SpatialFilter
            {
                SubFields = $"{featureClass.IDFieldName} {featureClass.ShapeFieldName}",
                FilterSpatialReference = scope.DisplayService.SpatialReference,
                Geometry = envelope
            };
            filter.SetFeatureSpatialReference(scope.DisplayService.SpatialReference, scope.DisplayService.DatumTransformations);

            using IFeatureCursor? cursor = await featureClass.GetFeatures(filter);
            if (cursor is null)
            {
                continue;
            }
            IFeature feature;
            while ((feature = await cursor.NextFeature()) is not null)
            {
                var point = feature.Shape switch
                {
                    IPoint p => p,
                    IMultiPoint mp when mp.PointCount > 0 => mp[0],
                    _ => null
                };
                if (point is null)
                {
                    continue;
                }
                double distance = envelope.Center.Distance(point);
                if (distance < bestDistance)
                {
                    (bestDistance, best, bestPoint) = (distance, $"{layer.Path}#{feature.OID}", point);
                }
            }
        }

        if (best is null || bestPoint is null)
        {
            _context.Info = "No network node at this location.";
            return false;
        }

        var graphics = scope.Document.Map.Display.GraphicsContainer.Elements;
        _context.Info = null;
        if (_context.Pick == NetworkSearchPick.Start)
        {
            _context.Start = best;
            graphics.Remove(typeof(GraphicStartPoint));
            graphics.Add(new GraphicStartPoint(bestPoint));
            if (_context.Operation == "path")
            {
                _context.Pick = NetworkSearchPick.Target;
            }
        }
        else
        {
            _context.Target = best;
            graphics.Remove(typeof(GraphicTargetPoint));
            graphics.Add(new GraphicTargetPoint(bestPoint));
        }

        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Graphics);
        return true;
    }

    private async Task<bool> Run(ICartoApplicationScopeService scope)
    {
        var context = _context!;
        var graph = Graph(scope);
        var request = new NetworkQueryRequest
        {
            Start = context.Operation is "trace" or "reach" ? context.Start : null,
            From = context.Operation == "path" ? context.Start : null,
            To = context.Operation == "path" ? context.Target : null,
            Sources = context.Operation == "supply" ? context.Start : null,
            Sinks = context.Operation == "supply" ? context.Target : null,
            Node = context.Operation == "info" ? context.Start : null,
            Weight = context.Weight,
            MaxCost = string.IsNullOrWhiteSpace(context.MaxCost) ? null : double.Parse(context.MaxCost.Replace(',', '.'), CultureInfo.InvariantCulture),
            Filter = NullIfEmpty(context.Filter),
            Block = NullIfEmpty(context.Block),
            RuleIds = NullIfEmpty(context.RuleIds),
            // the rules of the map (module settings), so they can be tried without building again
            Rules = context.Operation == "validate" ? NetworkModule.Clone(Module(scope).Config.Rules) : null,
        };

        context.Busy = true;
        context.Result = null;
        context.NotifyChanged();

        NetworkQueryResult result;
        try
        {
            result = await Task.Run(() => new NetworkQueryService(graph).Run(context.Operation, request));
        }
        finally
        {
            context.Busy = false;
        }
        context.Result = result;

        await Draw(scope, result);
        return true;
    }

    private async Task<bool> Clear(ICartoApplicationScopeService scope)
    {
        RemoveResultGraphics(scope);
        var graphics = scope.Document.Map.Display.GraphicsContainer.Elements;
        graphics.Remove(typeof(GraphicStartPoint));
        graphics.Remove(typeof(GraphicTargetPoint));

        _context!.Result = null;
        _context.Start = _context.Target = null;
        _context.Info = null;
        _context.Pick = NetworkSearchPick.Start;

        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Graphics);
        return true;
    }

    /// <summary>The features of the result become the selection of their layers (whole features).</summary>
    private async Task<bool> ToSelection(ICartoApplicationScopeService scope)
    {
        if (_context!.Result is not NetworkQueryResult result)
        {
            return false;
        }

        var layers = MapLayers.FeatureLayers(scope.Document.Map);
        int count = 0;
        foreach (var (layerPath, oids) in NetworkResultGeometry.ObjectIdsByLayer(result))
        {
            MapLayer layer;
            try
            {
                layer = MapLayers.Find(layers, layerPath);
            }
            catch (InvalidDataException)
            {
                continue;
            }
            if (layer.Layer is not IFeatureSelection selection)
            {
                continue;
            }
            var set = new IDSelectionSet();
            set.AddIDs(oids.Select(oid => (int)oid).ToList());
            selection.SelectionSet = set;
            selection.FireSelectionChangedEvent();
            count += oids.Count;
        }

        _context.Info = $"{count} feature(s) selected.";
        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Selection);
        return true;
    }

    /// <summary>Writes the result with the real line parts to {network}/results/{operation}-{time}.geojson.</summary>
    private async Task<bool> ExportGeoJson(ICartoApplicationScopeService scope)
    {
        if (_context!.Result is not NetworkQueryResult result || string.IsNullOrEmpty(_context.NetworkDirectory))
        {
            return false;
        }

        var coordinates = await NetworkResultGeometry.EdgeCoordinatesAsync(scope.Document.Map, result);
        var directory = System.IO.Path.Combine(_context.NetworkDirectory, "results");
        Directory.CreateDirectory(directory);
        var path = System.IO.Path.Combine(directory, $"{result.Operation}-{DateTime.Now:yyyyMMdd-HHmmss}.geojson");
        await File.WriteAllTextAsync(path, NetworkResultWriter.ToGeoJson(result, e => coordinates.TryGetValue(e.Id, out var c) ? c : null));

        _context.Info = $"Written: {path}";
        return true;
    }

    /// <summary>Builds the network in the background (log and state in the module), the panel follows its progress.</summary>
    private bool Build(ICartoApplicationScopeService scope)
    {
        var module = Module(scope);
        if (!module.StartBuild(scope.Document.FilePath))
        {
            _context!.Info = "A build is already running.";
            return false;
        }

        var state = module.BuildState!;
        _context!.Build = state;
        _context.Info = "Build started, see Report.";
        state.Changed += () =>
        {
            if (!state.Running)
            {
                _context.Error = null;
                Refresh(scope);
                _context.Info = state.Succeeded == true ? "Build finished." : "Build failed, see Report.";
            }
            _context.NotifyChanged();
        };
        return true;
    }

    /// <summary>Zooms to the violation (index in the result) with a little room around it.</summary>
    private async Task<bool> ZoomToViolation(ICartoApplicationScopeService scope, int index)
    {
        if (_context!.Result is not NetworkQueryResult result || index < 0 || index >= result.Violations.Count)
        {
            return false;
        }

        var violation = result.Violations[index];
        var networkSrs = string.IsNullOrEmpty(result.Srs) ? scope.DisplayService.SpatialReference : SpatialReference.FromID(result.Srs);
        const double room = 25; // the network is metric
        var corners = new[] { new Point(violation.X - room, violation.Y - room), new Point(violation.X + room, violation.Y + room) }
            .Select(p => scope.GeoTransformer.Transform(p, networkSrs, scope.DisplayService.SpatialReference, scope.DisplayService.DatumTransformations) as IPoint)
            .ToList();
        if (corners.Any(c => c is null))
        {
            return false;
        }

        await scope.EventBus.FireMapZoomToAsync(new Envelope(
            Math.Min(corners[0]!.X, corners[1]!.X), Math.Min(corners[0]!.Y, corners[1]!.Y),
            Math.Max(corners[0]!.X, corners[1]!.X), Math.Max(corners[0]!.Y, corners[1]!.Y)));
        return true;
    }

    #endregion

    #region Helper

    private static NetworkModule Module(ICartoApplicationScopeService scope)
        => scope.Modules.OfType<NetworkModule>().FirstOrDefault(m => m.IsConfigured)
           ?? throw new InvalidOperationException("The map has no network configured.");

    private FileNetworkGraph Graph(ICartoApplicationScopeService scope)
    {
        var mapPath = scope.Document.FilePath;
        if (string.IsNullOrEmpty(mapPath))
        {
            throw new InvalidOperationException("Save the map first, the network is built next to it.");
        }
        var directory = Module(scope).Config.ResolveDirectory(mapPath);
        if (!File.Exists(System.IO.Path.Combine(directory, FileNetworkStore.MetadataFile)))
        {
            throw new InvalidOperationException($"The network is not built yet: use Build in this tool or on the network module page ({directory}).");
        }
        _context!.NetworkDirectory = directory;
        return NetworkGraphCache.Shared.Get(directory);
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static void RemoveResultGraphics(ICartoApplicationScopeService scope)
    {
        var graphics = scope.Document.Map.Display.GraphicsContainer.Elements;
        graphics.Remove(typeof(GraphicNetworkResultEdge));
        graphics.Remove(typeof(GraphicNetworkResultNode));
    }

    /// <summary>The edges as the parts of their lines, nodes and violations as points, in the display's spatial reference.</summary>
    private static async Task Draw(ICartoApplicationScopeService scope, NetworkQueryResult result)
    {
        RemoveResultGraphics(scope);

        var map = scope.Document.Map;
        var graphics = map.Display.GraphicsContainer.Elements;
        var networkSrs = string.IsNullOrEmpty(result.Srs) ? scope.DisplayService.SpatialReference : SpatialReference.FromID(result.Srs);
        IGeometry ToDisplay(IGeometry geometry)
            => scope.GeoTransformer.Transform(geometry, networkSrs, scope.DisplayService.SpatialReference, scope.DisplayService.DatumTransformations);

        var coordinates = await NetworkResultGeometry.EdgeCoordinatesAsync(map, result);
        foreach (var edge in result.Edges)
        {
            var points = coordinates.TryGetValue(edge.Id, out var c) ? c : [[edge.FromX, edge.FromY], [edge.ToX, edge.ToY]];
            var path = new gView.Framework.Geometry.Path();
            foreach (var xy in points)
            {
                path.AddPoint(new Point(xy[0], xy[1]));
            }
            var polyline = new Polyline();
            polyline.AddPath(path);
            if (ToDisplay(polyline) is IPolyline displayed)
            {
                graphics.Add(new GraphicNetworkResultEdge(displayed, EdgeColor));
            }
        }

        foreach (var node in result.Nodes.Where(n => n.AliasOf.Length == 0 && !double.IsNaN(n.X)))
        {
            var problem = node.Kind is "NotSupplied" or "Multiple" or "Blocked";
            if (ToDisplay(new Point(node.X, node.Y)) is IPoint point)
            {
                graphics.Add(new GraphicNetworkResultNode(point, problem ? ProblemColor : NodeColor, problem ? 10 : 7));
            }
        }
        foreach (var violation in result.Violations)
        {
            if (ToDisplay(new Point(violation.X, violation.Y)) is IPoint point)
            {
                graphics.Add(new GraphicNetworkResultNode(point, ProblemColor, 11));
            }
        }

        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Graphics);
    }

    #endregion
}
