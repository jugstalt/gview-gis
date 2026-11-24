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
using gView.Framework.NetworkModel.Extensions;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Module;
using gView.Framework.NetworkModel.Query;
using gView.Framework.NetworkModel.Store;
using gView.GraphicsEngine;
using System.Globalization;

namespace gView.Carto.Plugins.CartoTools.Network;

/// <summary>
/// Searches on the SpatialNET network of the map (NetworkModule, built into its network directory): click a node (also
/// an edge end, found in the spatial index of the network) or for outage a line, choose trace, reach, path, supply,
/// outage, info or validate; the result is drawn with the real line parts and can become the selection or a GeoJSON file.
/// </summary>
[RegisterPlugIn("B3A1C7E2-5D84-4F0B-9E61-2C7A9D5F3B18")]
internal class NetworkSearchTool : ICartoTool
{
    private NetworkSearchContext? _context;

    #region ICartoButton

    public string Name => "Network Search";

    public string ToolTip => "Search the network of the map (trace, path, supply, validate)";

    // Ctrl-drag draws a box for the area
    public ToolType ToolType => ToolType.Click | ToolType.BBox;

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
            _context.Tiers = graph.Tiers.Select(t => t.Name).ToList();
            if (!string.IsNullOrEmpty(_context.Tier))
            {
                // tiers that are gone after a refresh drop out of the selection
                var kept = _context.Tier.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(t => _context.Tiers.Contains(t, StringComparer.OrdinalIgnoreCase));
                _context.Tier = string.Join(",", kept);
            }
        }
        catch (Exception) when (_context.IsProblems)
        {
            // not built: the build reports are there also when the build failed
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
                NetworkSearchEventArgs { Action: NetworkSearchAction.Refresh } => RefreshNetwork(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.Cancel } => CancelRun(),
                NetworkSearchEventArgs { Action: NetworkSearchAction.Clear } => await Clear(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ToSelection } => await ToSelection(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ZoomToViolation } a => await ZoomToViolation(scope, a.Index),
                NetworkSearchEventArgs { Action: NetworkSearchAction.RemovePick } a => await RemovePick(scope, a.Index, a.Target),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ZoomToPick } a => await ZoomToPick(scope, a.Index, a.Target),
                NetworkSearchEventArgs { Action: NetworkSearchAction.AreaFromExtent } => await AreaFromExtent(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.AreaFromSelection } => await AreaFromSelection(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.RemoveArea } => await RemoveArea(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ZoomToArea } => await ZoomToArea(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ToggleLegend } a => await ToggleLegend(scope, a.Key),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ZoomToLegend } a => await ZoomToLegend(scope, a.Key),
                NetworkSearchEventArgs { Action: NetworkSearchAction.SelectLegend } a => await SelectLegend(scope, a.Key, a.Target),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ZoomToElement } a => await ZoomToElement(scope, a.Key, a.Index),
                NetworkSearchEventArgs { Action: NetworkSearchAction.Redraw } => await Redraw(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.Download } a => await Download(scope, a.Key),
                NetworkSearchEventArgs { Action: NetworkSearchAction.SelectProblem } a => await SelectProblem(scope, a.Index, a.Target),
                MapBBoxEventArgs bbox => await AreaFromBox(scope, bbox),
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

    private bool RefreshNetwork(ICartoApplicationScopeService scope)
    {
        Refresh(scope);
        return true;
    }

    /// <summary>
    /// Snaps the click to the nearest node of the network (real nodes, edge ends, merged points) and sets it as start or
    /// target; for outage it picks the part of the line under the click as the edge to switch off.
    /// </summary>
    private async Task<bool> PickNode(ICartoApplicationScopeService scope, ToolEventArgs eventArgs)
    {
        if (_context!.Pick == NetworkSearchPick.None || scope.DisplayService.SpatialReference is null)
        {
            return false;
        }
        if (_context.Pick == NetworkSearchPick.Area)
        {
            return await AreaFromFeature(scope, eventArgs);
        }

        var envelope = eventArgs.GetGeometry().ToProjectedEnvelope(scope, scope.DisplayService.SpatialReference);
        if (envelope is null)
        {
            return false;
        }

        var graph = Graph(scope);
        var displaySrs = scope.DisplayService.SpatialReference;
        var networkSrs = string.IsNullOrEmpty(graph.Metadata.Srs) ? displaySrs : SpatialReference.FromID(graph.Metadata.Srs) ?? displaySrs;
        if (scope.GeoTransformer.Transform(envelope, displaySrs, networkSrs, scope.DisplayService.DatumTransformations)?.Envelope is not IEnvelope networkEnvelope)
        {
            return false;
        }
        var center = networkEnvelope.Center;
        double radius = Math.Max(networkEnvelope.Width, networkEnvelope.Height) / 2;

        string reference, label;
        IPoint location;
        if (_context.PicksEdge)
        {
            var edge = await PickEdge(scope, graph, networkEnvelope, networkSrs);
            if (edge is null)
            {
                _context.Info = "No network line at this location.";
                return false;
            }
            (reference, label, location) = edge.Value;
        }
        else
        {
            // several nodes may lie at one place (separate tiers): the selected tier narrows them, and clicking the same
            // place again takes the next one
            var tier = NetworkTiers.Select(graph, _context.Tier)?.Union();
            var hits = graph.NodesAt(center.X, center.Y, radius, tier);
            if (hits.Count == 0)
            {
                _context.Info = tier is null ? "No network node at this location." : $"No network node of the tier {tier.Name} at this location.";
                return false;
            }
            var place = hits.Select(h => h.Entry.Node).ToList();
            _placeIndex = place.SequenceEqual(_place) ? (_placeIndex + 1) % place.Count : 0;
            _place = place;
            var entry = hits[_placeIndex].Entry;
            bool helper = NetworkIds.IsHelper(entry.Node);
            // edge ends have no feature: they are taken by their location (by id when other nodes share the place);
            // real nodes and merged points by Layer#OID
            reference = helper
                ? (hits.Count > 1 ? entry.Node.ToString(System.Globalization.CultureInfo.InvariantCulture) : NetworkLocations.Reference(entry.X, entry.Y))
                : graph.NodeLabel(entry.Id);
            label = entry.Id == entry.Node ? graph.NodeLabel(entry.Node) : $"{graph.NodeLabel(entry.Id)} (on {graph.NodeLabel(entry.Node)})";
            location = new Point(entry.X, entry.Y);
        }

        if (scope.GeoTransformer.Transform(location, networkSrs, displaySrs, scope.DisplayService.DatumTransformations) is not IPoint displayPoint)
        {
            return false;
        }

        _pickPoints[reference] = displayPoint;
        _pickLocations[reference] = (location.X, location.Y);
        bool ctrl = eventArgs is MapClickEventArgs { CtrlKey: true };
        // with several nodes at the place the next click there takes the next one instead of adding another pick
        var placeInfo = _place.Count > 1 && !_context.PicksEdge
            ? $"{_place.Count} nodes at this place: {string.Join(", ", _place.Select((n, i) => (i == _placeIndex ? "▶ " : "") + graph.Describe(n)))}. Click again to take the next one."
            : null;
        if (placeInfo is not null && _placeIndex > 0)
        {
            ReplaceLastPick(_context.Pick == NetworkSearchPick.Start ? _context.StartItems : _context.TargetItems);
        }
        if (_context.Pick == NetworkSearchPick.Start)
        {
            Pick(_context.StartItems, _context.Start, reference, label, ctrl && _context.StartAllowsMany);
            (_context.Start, _context.StartLabel) = Summary(_context.StartItems);
            _context.Info = placeInfo; // otherwise the picked items are listed in the report
            if (_context.Operation == "path")
            {
                _context.Pick = NetworkSearchPick.Target;
            }
        }
        else
        {
            Pick(_context.TargetItems, _context.Target, reference, label, ctrl && _context.TargetAllowsMany);
            (_context.Target, _context.TargetLabel) = Summary(_context.TargetItems);
            _context.Info = placeInfo;
        }
        DrawPicks(scope);

        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Graphics);
        return true;
    }

    /// <summary>The nodes at the place of the last click (several with separate tiers), and which of them was taken.</summary>
    private List<long> _place = [];
    private int _placeIndex;

    /// <summary>Drops the item picked last, before the next node of the same place replaces it.</summary>
    private static void ReplaceLastPick<T>(List<T> items)
    {
        if (items.Count > 0)
        {
            items.RemoveAt(items.Count - 1);
        }
    }

    /// <summary>The display locations of the picked references, for the markers.</summary>
    private readonly Dictionary<string, IPoint> _pickPoints = [];

    /// <summary>The locations of the picked references in the spatial reference of the network, for zooming.</summary>
    private readonly Dictionary<string, (double X, double Y)> _pickLocations = [];

    /// <summary>Removes a picked item (✕ in the report).</summary>
    private async Task<bool> RemovePick(ICartoApplicationScopeService scope, int index, bool target)
    {
        var items = target ? _context!.TargetItems : _context!.StartItems;
        if (index < 0 || index >= items.Count)
        {
            return false;
        }
        items.RemoveAt(index);
        if (target)
        {
            (_context.Target, _context.TargetLabel) = Summary(items);
        }
        else
        {
            (_context.Start, _context.StartLabel) = Summary(items);
        }
        DrawPicks(scope);
        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Graphics);
        return true;
    }

    /// <summary>
    /// Zooms to a picked item: where it was clicked, or for a typed Layer#OID or @x,y where the network has it
    /// (a node, or the ends of the edges of a line); type patterns have no place.
    /// </summary>
    private async Task<bool> ZoomToPick(ICartoApplicationScopeService scope, int index, bool target)
    {
        var items = target ? _context!.TargetItems : _context!.StartItems;
        if (index < 0 || index >= items.Count)
        {
            return false;
        }
        var reference = items[index].Reference;
        var graph = Graph(scope);
        var points = new List<(double X, double Y)>();
        if (_pickLocations.TryGetValue(reference, out var picked))
        {
            points.Add(picked);
        }
        else if (!target && _context.PicksEdge)
        {
            foreach (var id in graph.ResolveEdges(reference))
            {
                if (graph.TryFindEdge(id, out var edge))
                {
                    foreach (var node in new[] { edge.Edge.From, edge.Edge.To })
                    {
                        if (graph.TryGetNode(node, out var n))
                        {
                            points.Add((n.Node.Location.X, n.Node.Location.Y));
                        }
                    }
                }
            }
        }
        else if (reference.Contains('#') || reference.StartsWith('@') || long.TryParse(reference, out _))
        {
            foreach (var id in graph.ResolveNodes(reference))
            {
                if (graph.TryGetNode(id, out var n))
                {
                    points.Add((n.Node.Location.X, n.Node.Location.Y));
                }
            }
        }
        if (points.Count == 0)
        {
            _context.Info = $"{reference} has no single place to zoom to.";
            return false;
        }
        return await ZoomToNetworkPoints(scope, graph.Metadata.Srs, points);
    }

    /// <summary>Zooms to points in the spatial reference of the network with a little room around them.</summary>
    private static async Task<bool> ZoomToNetworkPoints(ICartoApplicationScopeService scope, string? srs, IReadOnlyCollection<(double X, double Y)> points)
    {
        var networkSrs = string.IsNullOrEmpty(srs) ? scope.DisplayService.SpatialReference : SpatialReference.FromID(srs);
        const double room = 25; // the network is metric
        var corners = new[]
            {
                new Point(points.Min(p => p.X) - room, points.Min(p => p.Y) - room),
                new Point(points.Max(p => p.X) + room, points.Max(p => p.Y) + room)
            }
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

    /// <summary>
    /// A click replaces the picked items, a Ctrl-click adds the item or removes it when it is picked already. Text typed into
    /// the field before is kept as one item. Returns false when the item was removed.
    /// </summary>
    private static bool Pick(List<NetworkPickedItem> items, string? current, string reference, string label, bool toggle)
    {
        if (!toggle)
        {
            items.Clear();
            items.Add(new NetworkPickedItem(reference, label));
            return true;
        }
        if (items.Count == 0 && !string.IsNullOrWhiteSpace(current))
        {
            items.Add(new NetworkPickedItem(current, current));
        }
        if (items.RemoveAll(i => i.Reference == reference) > 0)
        {
            return false;
        }
        items.Add(new NetworkPickedItem(reference, label));
        return true;
    }

    /// <summary>The references for the search and the label of the button: the item, or how many.</summary>
    private (string? References, string? Label) Summary(List<NetworkPickedItem> items) => items.Count switch
    {
        0 => (null, null),
        1 => (items[0].Reference, items[0].Label),
        _ => (string.Join(", ", items.Select(i => i.Reference)), $"{items.Count} {(_context!.PicksEdge ? "edges" : "nodes")}")
    };

    /// <summary>One marker per picked start and target.</summary>
    private void DrawPicks(ICartoApplicationScopeService scope)
    {
        var graphics = scope.Document.Map.Display.GraphicsContainer.Elements;
        graphics.Remove(typeof(GraphicStartPoint));
        graphics.Remove(typeof(GraphicTargetPoint));
        foreach (var item in _context!.StartItems)
        {
            if (_pickPoints.TryGetValue(item.Reference, out var point))
            {
                graphics.Add(new GraphicStartPoint(point));
            }
        }
        foreach (var item in _context.TargetItems)
        {
            if (_pickPoints.TryGetValue(item.Reference, out var point))
            {
                graphics.Add(new GraphicTargetPoint(point));
            }
        }
    }

    /// <summary>
    /// The part of a network line nearest to the click: Layer#OID/part (the edge between two nodes, not the whole line),
    /// its label and the point on the line, in the spatial reference of the network.
    /// </summary>
    private async Task<(string Reference, string Label, IPoint Location)?> PickEdge(
        ICartoApplicationScopeService scope, FileNetworkGraph graph, IEnvelope networkEnvelope, ISpatialReference networkSrs)
    {
        var module = Module(scope);
        var edgeLayerIds = module.Config.Edges.Select(e => e.LayerId).OfType<int>().ToHashSet();
        var center = networkEnvelope.Center;

        (MapLayer Layer, long Oid, double Distance, double Measure, IPoint Point)? best = null;
        foreach (var layer in MapLayers.FeatureLayers(scope.Document.Map).Where(l => edgeLayerIds.Contains(l.Id)))
        {
            var featureClass = layer.Layer.FeatureClass;
            var filter = new SpatialFilter
            {
                SubFields = $"{featureClass.IDFieldName} {featureClass.ShapeFieldName}",
                FilterSpatialReference = networkSrs,
                Geometry = networkEnvelope
            };
            // measures are lengths in the network's spatial reference
            filter.SetFeatureSpatialReference(networkSrs, scope.DisplayService.DatumTransformations);

            using IFeatureCursor? cursor = await featureClass.GetFeatures(filter);
            if (cursor is null)
            {
                continue;
            }
            IFeature feature;
            while ((feature = await cursor.NextFeature()) is not null)
            {
                if (feature.Shape is IPolyline polyline && Closest(polyline, center) is var (distance, measure, point) && (best is null || distance < best.Value.Distance))
                {
                    best = (layer, feature.OID, distance, measure, point);
                }
            }
        }
        if (best is not { } found)
        {
            return null;
        }

        var featureClassIndex = graph.Metadata.FeatureClasses
            .FirstOrDefault(f => f.Kind == SpatialNET.Network.ElementKind.Edge && f.Layer.Equals(found.Layer.Path, StringComparison.OrdinalIgnoreCase))?.Index
            ?? throw new InvalidOperationException($"{found.Layer.Path} is not in the built network; build it again.");
        foreach (var id in graph.EdgePartsOf(featureClassIndex, found.Oid))
        {
            if (graph.TryFindEdge(id, out var edge)
                && found.Measure >= Math.Min(edge.FromMeasure, edge.ToMeasure) - 1e-6
                && found.Measure <= Math.Max(edge.FromMeasure, edge.ToMeasure) + 1e-6)
            {
                var label = graph.EdgeLabel(id);
                return (label, label, found.Point);
            }
        }
        throw new InvalidOperationException($"{found.Layer.Path}#{found.Oid} is no edge of the built network (left out by a filter, or the network is out of date).");
    }

    /// <summary>The distance of a point to a line, the measure (length from the line's start) and the point on the line nearest to it.</summary>
    private static (double Distance, double Measure, IPoint Point)? Closest(IPolyline polyline, IPoint p)
    {
        (double Distance, double Measure, IPoint Point)? best = null;
        double measure = 0;
        for (int i = 0; i < polyline.PathCount; i++)
        {
            var path = polyline[i];
            for (int j = 1; j < path.PointCount; j++)
            {
                var a = path[j - 1];
                var b = path[j];
                double dx = b.X - a.X, dy = b.Y - a.Y, length2 = dx * dx + dy * dy;
                double t = length2 == 0 ? 0 : Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / length2, 0, 1);
                double x = a.X + dx * t, y = a.Y + dy * t;
                double distance = Math.Sqrt((p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y));
                if (best is null || distance < best.Value.Distance)
                {
                    best = (distance, measure + Math.Sqrt(length2) * t, new Point(x, y));
                }
                measure += Math.Sqrt(length2);
            }
        }
        return best;
    }

    private async Task<bool> Run(ICartoApplicationScopeService scope)
    {
        var context = _context!;
        scope.Document.Map.Display.GraphicsContainer.Elements.Remove(typeof(GraphicNetworkProblemMarker));
        if (context.IsProblems)
        {
            return await ShowProblems(scope);
        }
        var graph = Graph(scope);
        var config = Module(scope).Config;
        // with a tier its sources and sinks count, not the defaults of the module
        var tier = context.Operation == "info" ? null : context.Tier.NullIfEmpty();
        var request = new NetworkQueryRequest
        {
            Tier = tier,
            StopAtSources = context.Operation == "trace" && tier is not null ? context.StopAtSources : null,
            Start = context.Operation is "trace" or "reach" ? context.Start : null,
            Branches = context.Operation == "feeders" ? context.Branches.NullIfEmpty() : null,
            OpenEdges = context.Operation == "feeders" && context.IncludeOpen,
            From = context.Operation == "path" ? context.Start : null,
            To = context.Operation == "path" ? context.Target : null,
            // supply and outage: the defaults of the map's network module (they may differ from the built network)
            Sources = context.Operation switch
            {
                "supply" or "feeders" => context.Start.NullIfEmpty() ?? (tier is null ? config.SupplySources : null),
                "outage" => context.Sources.NullIfEmpty() ?? (tier is null ? config.SupplySources : null),
                _ => null
            },
            Sinks = context.Operation switch
            {
                "supply" => context.Target.NullIfEmpty() ?? (tier is null ? config.Customers : null),
                "outage" => context.Customers.NullIfEmpty() ?? (tier is null ? config.Customers : null),
                _ => null
            },
            Disable = context.Operation == "outage" ? context.Start : context.Disable.NullIfEmpty(),
            Node = context.Operation == "info" ? context.Start : null,
            Weight = context.Weight,
            MaxCost = string.IsNullOrWhiteSpace(context.MaxCost) ? null : double.Parse(context.MaxCost.Replace(',', '.'), CultureInfo.InvariantCulture),
            Filter = context.Filter.NullIfEmpty(),
            Block = context.Block.NullIfEmpty(),
            RuleIds = context.RuleIds.NullIfEmpty(),
            // the rules of the map (module settings), so they can be tried without building again
            Rules = context.Operation == "validate" ? NetworkModule.Clone(Module(scope).Config.Rules) : null,
            Area = context.Operation is "info" ? null : context.Area,
        };

        using var cancel = new CancellationTokenSource();
        _runCancel = cancel;
        context.Busy = true;
        context.Result = null;
        context.Legend = null;
        context.Progress = "starting...";
        context.NotifyChanged();
        var progress = new Progress(line =>
        {
            context.Progress = line;
            context.NotifyChanged();
        });

        try
        {
            var result = await Task.Run(() => new NetworkQueryService(graph).Run(context.Operation, request, progress, cancel.Token), cancel.Token);
            context.Result = result;
            // hidden rows stay hidden when the same operation runs again
            if (context.HiddenOperation != result.Operation)
            {
                context.HiddenKeys.Clear();
                context.HiddenOperation = result.Operation;
            }
            context.Legend = NetworkResultLegend.Build(result, context.FeederColoring, context.FeederPalette);
            context.Progress = "drawing the result...";
            context.NotifyChanged();
            await Draw(scope, result, cancel.Token);
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested)
        {
            context.Info = $"Cancelled{(string.IsNullOrEmpty(context.Progress) ? "" : $" ({context.Progress})")}.";
        }
        finally
        {
            _runCancel = null;
            context.Busy = false;
            context.Progress = null;
        }
        return true;
    }

    /// <summary>The run in progress, to cancel it.</summary>
    private CancellationTokenSource? _runCancel;

    private bool CancelRun()
    {
        _runCancel?.Cancel();
        return true;
    }

    /// <summary>Progress lines from the search thread (not tied to a synchronization context).</summary>
    private sealed class Progress(Action<string> report) : IProgress<string>
    {
        public void Report(string value) => report(value);
    }

    private async Task<bool> Clear(ICartoApplicationScopeService scope)
    {
        RemoveResultGraphics(scope);
        var graphics = scope.Document.Map.Display.GraphicsContainer.Elements;
        graphics.Remove(typeof(GraphicStartPoint));
        graphics.Remove(typeof(GraphicTargetPoint));
        graphics.Remove(typeof(GraphicNetworkProblemMarker));

        _context!.Result = null;
        _context.Reports = null;
        _context.Legend = null;
        _drawn = null;
        _context.Start = _context.Target = _context.StartLabel = _context.TargetLabel = null;
        _context.StartItems.Clear();
        _context.TargetItems.Clear();
        _pickPoints.Clear();
        _pickLocations.Clear();
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

        var (count, _) = Select(scope, NetworkResultGeometry.ObjectIdsByLayer(result), add: false);

        _context.Info = $"{count} feature(s) selected.";
        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Selection);
        return true;
    }

    /// <summary>The result with the real line parts as GeoJSON (line ends snapped onto the nodes if set).</summary>
    private async Task WriteGeoJson(ICartoApplicationScopeService scope, NetworkQueryResult result, string path)
    {
        if (result.Operation == NetworkQueryService.ProblemsOperation)
        {
            // points only, and the network may not be built (failed build)
            await File.WriteAllTextAsync(path, NetworkResultWriter.ToGeoJson(result));
            return;
        }
        var coordinates = await NetworkResultGeometry.EdgeCoordinatesAsync(scope.Document.Map, result,
            snap: _context!.SnapExport, tolerance: Graph(scope).Metadata.Tolerance);
        await File.WriteAllTextAsync(path, NetworkResultWriter.ToGeoJson(result, e => coordinates.TryGetValue(e.Id, out var c) ? c : null));
    }

    /// <summary>
    /// Writes the result (with a key only the elements of that legend row or source) as GeoJSON into a temporary file
    /// the panel streams to the browser: users often can't reach the network directory on the server.
    /// </summary>
    private async Task<bool> Download(ICartoApplicationScopeService scope, string? key)
    {
        if (_context!.Result is not NetworkQueryResult result)
        {
            return false;
        }
        string part = "";
        if (key is not null)
        {
            if (_context.Legend is not NetworkResultLegend legend || legend.Result != result)
            {
                return false;
            }
            var entries = legend.Find(key);
            result = legend.Subset(entries);
            part = "-" + (entries.Count == 1 ? entries[0].Name : entries.FirstOrDefault()?.Group ?? key);
        }
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"gview-network-{Guid.NewGuid():N}.geojson");
        await WriteGeoJson(scope, result, path);
        _context.PendingDownload = new NetworkDownload(path, SafeFileName($"{result.Operation}{part}-{DateTime.Now:yyyyMMdd-HHmmss}.geojson"));
        return true;
    }

    /// <summary>Mittelspannung/Anlage#5268 → Mittelspannung_Anlage_5268.</summary>
    private static string SafeFileName(string name)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars().Concat(['#', ' ', '(', ')']).ToHashSet();
        return new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
    }

    /// <summary>Zooms to the violation (index in the result) with a little room around it.</summary>
    private async Task<bool> ZoomToViolation(ICartoApplicationScopeService scope, int index)
    {
        if (_context!.Result is not NetworkQueryResult result || index < 0 || index >= result.Violations.Count)
        {
            return false;
        }

        if (result.Operation == NetworkQueryService.ProblemsOperation)
        {
            return await ZoomToProblem(scope, result, index);
        }
        var violation = result.Violations[index];
        return await ZoomToNetworkPoints(scope, result.Srs, [(violation.X, violation.Y)]);
    }

    #endregion

    #region Problems

    /// <summary>The scale a click on a problem zooms to; a map that shows more detail is only panned.</summary>
    private const double ProblemScale = 500;

    /// <summary>
    /// problems: reads the build reports next to the network (also after a failed build) and shows the problems of the
    /// chosen category (the first one with problems when none is chosen) like a result: list, legend, points in the map.
    /// </summary>
    private async Task<bool> ShowProblems(ICartoApplicationScopeService scope)
    {
        var context = _context!;
        var directory = NetworkDirectory(scope);
        context.Busy = true;
        context.Result = null;
        context.Legend = null;
        context.Progress = "reading the build reports...";
        context.NotifyChanged();
        try
        {
            await ShowProblems(scope, context, directory);
        }
        finally
        {
            context.Busy = false;
            context.Progress = null;
        }
        return true;
    }

    private async Task ShowProblems(ICartoApplicationScopeService scope, NetworkSearchContext context, string directory)
    {
        // hundreds of thousands of problems are possible: off the thread of the UI
        var reports = await Task.Run(() => NetworkBuildReports.Read(directory));
        context.Reports = reports;
        scope.Document.Map.Display.GraphicsContainer.Elements.Remove(typeof(GraphicNetworkProblemMarker));
        if (!reports.Any)
        {
            context.Result = null;
            context.Legend = null;
            _drawn = null;
            RemoveResultGraphics(scope);
            context.Info = $"Build the network first: there are no build reports in {directory}.";
            await scope.EventBus.FireRefreshMapAsync(DrawPhase.Graphics);
            return;
        }

        var category = context.ProblemCategory
            ?? NetworkBuildReports.Categories.FirstOrDefault(c => reports.Of(c).Count > 0, NetworkProblemCategory.Diagnostics);
        context.ProblemCategory = category;
        // the spatial reference of the built network, after a failed build the one of the module, else the map's
        var srs = reports.Metadata?.Srs.NullIfEmpty() ?? Module(scope).Config.Srs.NullIfEmpty();
        context.Progress = $"{category}: {reports.Of(category).Count:N0} problem(s), building the list...";
        context.NotifyChanged();
        var (result, legend) = await Task.Run(() =>
        {
            var result = reports.ToResult(category, srs);
            return (result, NetworkResultLegend.Build(result));
        });

        context.Result = result;
        var hiddenFor = $"{result.Operation}:{category}";
        if (context.HiddenOperation != hiddenFor)
        {
            context.HiddenKeys.Clear();
            context.HiddenOperation = hiddenFor;
        }
        context.Legend = legend;
        context.Progress = "drawing...";
        context.NotifyChanged();
        await Draw(scope, result);
    }

    /// <summary>Zooms to a problem (1:500, or only pans when the map shows more detail) and sets the pin there.</summary>
    private async Task<bool> ZoomToProblem(ICartoApplicationScopeService scope, NetworkQueryResult result, int index)
    {
        if (index < 0 || index >= result.Violations.Count)
        {
            return false;
        }
        var problem = result.Violations[index];
        var displaySrs = scope.DisplayService.SpatialReference;
        var networkSrs = string.IsNullOrEmpty(result.Srs) ? displaySrs : SpatialReference.FromID(result.Srs) ?? displaySrs;
        var bounds = scope.DisplayService.MapBounds;
        if (displaySrs is null || bounds is null || bounds.Width <= 0
            || scope.GeoTransformer.Transform(new Point(problem.X, problem.Y), networkSrs, displaySrs, scope.DisplayService.DatumTransformations) is not IPoint point)
        {
            return await ZoomToNetworkPoints(scope, result.Srs, [(problem.X, problem.Y)]);
        }

        var graphics = scope.Document.Map.Display.GraphicsContainer.Elements;
        graphics.Remove(typeof(GraphicNetworkProblemMarker));
        graphics.Add(new GraphicNetworkProblemMarker(point, problem.Rule));

        double mapScale = scope.Document.Map.Display.MapScale;
        double factor = mapScale > ProblemScale ? ProblemScale / mapScale : 1;
        double width = bounds.Width * factor, height = bounds.Height * factor;
        await scope.EventBus.FireMapZoomToAsync(new Envelope(point.X - width / 2, point.Y - height / 2, point.X + width / 2, point.Y + height / 2));
        return true;
    }

    /// <summary>The features of a problem become the selection of their layers; <paramref name="onlyFirst"/>: the dropped or merged point only.</summary>
    private async Task<bool> SelectProblem(ICartoApplicationScopeService scope, int index, bool onlyFirst)
    {
        if (_context!.Result is not NetworkQueryResult result || index < 0 || index >= result.Violations.Count)
        {
            return false;
        }
        var problem = result.Violations[index];
        var features = problem.Features ?? [];
        if (onlyFirst)
        {
            features = features.Take(1).ToList();
        }
        if (features.Count == 0)
        {
            _context.Info = "This problem names no features (an older build, or a helper node).";
            return false;
        }
        var ids = features
            .GroupBy(f => f.Layer, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(f => f.Oid).ToHashSet(), StringComparer.OrdinalIgnoreCase);
        var (count, layerCount) = Select(scope, ids, add: false);
        _context.Notice = count == 0
            ? $"The layer{(ids.Count == 1 ? "" : "s")} {string.Join(", ", ids.Keys)} {(ids.Count == 1 ? "is" : "are")} not in the map."
            : $"{count} feature{(count == 1 ? "" : "s")} in {layerCount} layer{(layerCount == 1 ? "" : "s")} selected";
        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Selection);
        return true;
    }

    #endregion

    #region Legend

    /// <summary>Hides or shows the elements of a legend row (or of every branch of a source) in the map.</summary>
    private async Task<bool> ToggleLegend(ICartoApplicationScopeService scope, string? key)
    {
        if (key is null || _context!.Legend is not NetworkResultLegend legend)
        {
            return false;
        }
        var entries = legend.Find(key);
        bool hide = entries.Any(e => !_context.HiddenKeys.Contains(e.Key));
        foreach (var entry in entries)
        {
            if (hide)
            {
                _context.HiddenKeys.Add(entry.Key);
            }
            else
            {
                _context.HiddenKeys.Remove(entry.Key);
            }
        }
        AddResultGraphics(scope);
        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Graphics);
        return true;
    }

    /// <summary>Builds the legend again (feeders colouring or palette changed) and draws the result with it.</summary>
    private async Task<bool> Redraw(ICartoApplicationScopeService scope)
    {
        if (_context!.Result is not NetworkQueryResult result)
        {
            return false;
        }
        _context.Legend = NetworkResultLegend.Build(result, _context.FeederColoring, _context.FeederPalette);
        AddResultGraphics(scope);
        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Graphics);
        return true;
    }

    private async Task<bool> ZoomToLegend(ICartoApplicationScopeService scope, string? key)
    {
        if (key is null || _context!.Legend is not NetworkResultLegend legend)
        {
            return false;
        }
        var points = legend.Find(key).SelectMany(e => legend.Items(e)).SelectMany(i => i.Points).Where(p => !double.IsNaN(p.X)).ToList();
        return points.Count > 0 && await ZoomToNetworkPoints(scope, legend.Result.Srs, points);
    }

    /// <summary>Zooms to one element of a legend row (index in <see cref="NetworkResultLegend.Items"/>).</summary>
    private async Task<bool> ZoomToElement(ICartoApplicationScopeService scope, string? key, int index)
    {
        if (key is null || _context!.Legend is not NetworkResultLegend legend)
        {
            return false;
        }
        if (legend.Result.Operation == NetworkQueryService.ProblemsOperation)
        {
            // the list of a problems row is its violations in order
            var violations = legend.Find(key).SelectMany(e => e.Violations).ToList();
            return index >= 0 && index < violations.Count && await ZoomToProblem(scope, legend.Result, violations[index]);
        }
        var items = legend.Find(key).SelectMany(e => legend.Items(e)).ToList();
        if (index < 0 || index >= items.Count)
        {
            return false;
        }
        var points = items[index].Points.Where(p => !double.IsNaN(p.X)).ToList();
        return points.Count > 0 && await ZoomToNetworkPoints(scope, legend.Result.Srs, points);
    }

    /// <summary>
    /// The features of a legend row become the selection of their layers (the selection of other layers stays);
    /// <paramref name="add"/> (Ctrl) adds them to the selection. Helper nodes have no feature and are left out.
    /// </summary>
    private async Task<bool> SelectLegend(ICartoApplicationScopeService scope, string? key, bool add)
    {
        if (key is null || _context!.Legend is not NetworkResultLegend legend)
        {
            return false;
        }
        var (features, layerCount) = Select(scope, legend.ObjectIdsByLayer(legend.Find(key)), add);
        _context.Notice = $"{features} feature{(features == 1 ? "" : "s")} in {layerCount} layer{(layerCount == 1 ? "" : "s")} {(add ? "added to the selection" : "selected")}";
        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Selection);
        return true;
    }

    /// <summary>Sets (or adds to) the selection of the layers; returns how many features and layers.</summary>
    private static (int Features, int Layers) Select(ICartoApplicationScopeService scope, Dictionary<string, HashSet<long>> idsByLayer, bool add)
    {
        var layers = MapLayers.FeatureLayers(scope.Document.Map);
        int count = 0, layerCount = 0;
        foreach (var (layerPath, oids) in idsByLayer)
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
            if (add && selection.SelectionSet is IIDSelectionSet { Count: > 0 } existing)
            {
                set.AddIDs(existing.IDs.ToList());
            }
            set.AddIDs(oids.Select(oid => (int)oid).ToList());
            selection.SelectionSet = set;
            selection.FireSelectionChangedEvent();
            count += oids.Count;
            layerCount++;
        }
        return (count, layerCount);
    }

    #endregion

    #region Area

    /// <summary>Ctrl-drag: the box becomes the area.</summary>
    private async Task<bool> AreaFromBox(ICartoApplicationScopeService scope, ToolEventArgs eventArgs)
    {
        var displaySrs = scope.DisplayService.SpatialReference;
        if (displaySrs is null || eventArgs.GetGeometry().ToProjectedEnvelope(scope, displaySrs) is not IEnvelope box)
        {
            return false;
        }
        return await SetArea(scope, [ToNetwork(scope, Graph(scope), BoxPolygon(box))], "box");
    }

    /// <summary>The current map extent becomes the area.</summary>
    private async Task<bool> AreaFromExtent(ICartoApplicationScopeService scope)
    {
        var bounds = scope.DisplayService.MapBounds;
        if (scope.DisplayService.SpatialReference is null || bounds is null || bounds.Width <= 0 || bounds.Height <= 0)
        {
            _context!.Info = "The map has no extent yet.";
            return false;
        }
        return await SetArea(scope, [ToNetwork(scope, Graph(scope), BoxPolygon(bounds))], "map extent");
    }

    /// <summary>A click in area mode: the polygon feature under it (visible layers, the top one in the TOC first).</summary>
    private async Task<bool> AreaFromFeature(ICartoApplicationScopeService scope, ToolEventArgs eventArgs)
    {
        var graph = Graph(scope);
        var displaySrs = scope.DisplayService.SpatialReference!;
        var networkSrs = NetworkSrs(scope, graph);
        if (eventArgs.GetGeometry().ToProjectedEnvelope(scope, displaySrs) is not IEnvelope envelope
            || scope.GeoTransformer.Transform(envelope, displaySrs, networkSrs, scope.DisplayService.DatumTransformations)?.Envelope is not IEnvelope networkEnvelope)
        {
            return false;
        }
        var center = networkEnvelope.Center;

        foreach (var layer in PolygonLayers(scope).Where(l => l.Layer.Visible).OrderBy(l => l.TocOrder))
        {
            var featureClass = layer.Layer.FeatureClass;
            var filter = new SpatialFilter
            {
                SubFields = $"{featureClass.IDFieldName} {featureClass.ShapeFieldName}",
                FilterSpatialReference = networkSrs,
                Geometry = networkEnvelope
            };
            filter.SetFeatureSpatialReference(networkSrs, scope.DisplayService.DatumTransformations);

            using IFeatureCursor? cursor = await featureClass.GetFeatures(filter);
            if (cursor is null)
            {
                continue;
            }
            IFeature feature;
            while ((feature = await cursor.NextFeature()) is not null)
            {
                if (feature.Shape is IPolygon polygon && NetworkArea.FromGeometries([AsPolygon2D(polygon)]).Contains(center.X, center.Y))
                {
                    return await SetArea(scope, [polygon], $"{layer.Name}#{feature.OID}");
                }
            }
        }
        _context!.Info = "No polygon feature at this location (visible polygon layers).";
        return false;
    }

    /// <summary>The selected polygon features of all layers become the area together.</summary>
    private async Task<bool> AreaFromSelection(ICartoApplicationScopeService scope)
    {
        var graph = Graph(scope);
        var networkSrs = NetworkSrs(scope, graph);
        var polygons = new List<IPolygon>();
        foreach (var layer in PolygonLayers(scope))
        {
            if (layer.Layer is not IFeatureSelection { SelectionSet: IIDSelectionSet { Count: > 0 } selected })
            {
                continue;
            }
            var featureClass = layer.Layer.FeatureClass;
            var filter = new RowIDFilter(featureClass.IDFieldName, selected.IDs.ToList())
            {
                SubFields = $"{featureClass.IDFieldName} {featureClass.ShapeFieldName}"
            };
            filter.SetFeatureSpatialReference(networkSrs, scope.DisplayService.DatumTransformations);

            using IFeatureCursor? cursor = await featureClass.GetFeatures(filter);
            if (cursor is null)
            {
                continue;
            }
            IFeature feature;
            while ((feature = await cursor.NextFeature()) is not null)
            {
                if (feature.Shape is IPolygon polygon)
                {
                    polygons.Add(polygon);
                }
            }
        }
        if (polygons.Count == 0)
        {
            _context!.Info = "No polygon features selected.";
            return false;
        }
        return await SetArea(scope, polygons, $"selection ({polygons.Count} polygon{(polygons.Count == 1 ? "" : "s")})");
    }

    /// <summary>Sets the area (polygons in the spatial reference of the network) and draws its outline.</summary>
    private async Task<bool> SetArea(ICartoApplicationScopeService scope, IReadOnlyList<IPolygon> networkPolygons, string label)
    {
        var graph = Graph(scope);
        _context!.Area = NetworkArea.FromGeometries(networkPolygons.Select(AsPolygon2D));
        _context.AreaLabel = label;
        _context.Info = $"Area: {label}";
        // back to picking nodes, the area stays until it is removed
        _context.Pick = _context.NeedsStart ? NetworkSearchPick.Start : NetworkSearchPick.None;

        var graphics = scope.Document.Map.Display.GraphicsContainer.Elements;
        graphics.Remove(typeof(GraphicNetworkArea));
        var networkSrs = NetworkSrs(scope, graph);
        foreach (var polygon in networkPolygons)
        {
            if (scope.GeoTransformer.Transform(polygon, networkSrs, scope.DisplayService.SpatialReference, scope.DisplayService.DatumTransformations) is IPolygon displayed)
            {
                graphics.Add(new GraphicNetworkArea(displayed));
            }
        }
        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Graphics);
        return true;
    }

    private async Task<bool> RemoveArea(ICartoApplicationScopeService scope)
    {
        _context!.Area = null;
        _context.AreaLabel = null;
        if (_context.Pick == NetworkSearchPick.Area)
        {
            _context.Pick = _context.NeedsStart ? NetworkSearchPick.Start : NetworkSearchPick.None;
        }
        scope.Document.Map.Display.GraphicsContainer.Elements.Remove(typeof(GraphicNetworkArea));
        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Graphics);
        return true;
    }

    private async Task<bool> ZoomToArea(ICartoApplicationScopeService scope)
    {
        if (_context!.Area is not NetworkArea area || area.Kind == NetworkAreaKind.Shards)
        {
            return false;
        }
        return await ZoomToNetworkPoints(scope, Graph(scope).Metadata.Srs, [(area.MinX, area.MinY), (area.MaxX, area.MaxY)]);
    }

    private static IEnumerable<MapLayer> PolygonLayers(ICartoApplicationScopeService scope)
        => MapLayers.FeatureLayers(scope.Document.Map).Where(l => l.Layer.FeatureClass?.GeometryType == GeometryType.Polygon);

    private static IPolygon BoxPolygon(IEnvelope box)
    {
        var ring = new Ring();
        ring.AddPoint(new Point(box.MinX, box.MinY));
        ring.AddPoint(new Point(box.MaxX, box.MinY));
        ring.AddPoint(new Point(box.MaxX, box.MaxY));
        ring.AddPoint(new Point(box.MinX, box.MaxY));
        ring.AddPoint(new Point(box.MinX, box.MinY));
        return new Polygon(ring);
    }

    /// <summary>A display polygon in the spatial reference of the network.</summary>
    private static IPolygon ToNetwork(ICartoApplicationScopeService scope, FileNetworkGraph graph, IPolygon polygon)
        => scope.GeoTransformer.Transform(polygon, scope.DisplayService.SpatialReference, NetworkSrs(scope, graph), scope.DisplayService.DatumTransformations) as IPolygon
           ?? throw new InvalidOperationException("The area can't be projected into the spatial reference of the network.");

    /// <summary>gView polygons are SpatialNET polygons too (other implementations are copied).</summary>
    private static SpatialNET.IGeometry2D AsPolygon2D(IPolygon polygon)
        => polygon as SpatialNET.IPolygon2D ?? new Polygon(polygon);

    private static ISpatialReference NetworkSrs(ICartoApplicationScopeService scope, FileNetworkGraph graph)
    {
        var displaySrs = scope.DisplayService.SpatialReference ?? throw new InvalidOperationException("The map has no spatial reference.");
        return string.IsNullOrEmpty(graph.Metadata.Srs) ? displaySrs : SpatialReference.FromID(graph.Metadata.Srs) ?? displaySrs;
    }

    #endregion

    #region Helper

    private static NetworkModule Module(ICartoApplicationScopeService scope)
        => scope.Modules.OfType<NetworkModule>().FirstOrDefault(m => m.IsConfigured)
           ?? throw new InvalidOperationException("The map has no network configured.");

    /// <summary>The directory the network of the map is built into (next to the map), built or not.</summary>
    private string NetworkDirectory(ICartoApplicationScopeService scope)
    {
        var mapPath = scope.Document.FilePath;
        if (string.IsNullOrEmpty(mapPath))
        {
            throw new InvalidOperationException("Save the map first, the network is built next to it.");
        }
        var directory = Module(scope).Config.ResolveDirectory(mapPath);
        _context!.NetworkDirectory = directory;
        return directory;
    }

    private FileNetworkGraph Graph(ICartoApplicationScopeService scope)
    {
        var directory = NetworkDirectory(scope);
        if (!File.Exists(System.IO.Path.Combine(directory, FileNetworkStore.MetadataFile)))
        {
            throw new InvalidOperationException($"The network is not built yet: build it on the network module page (Map, Modules, Network, tab Build) ({directory}).");
        }
        return NetworkGraphCache.Shared.Get(directory);
    }


    private static void RemoveResultGraphics(ICartoApplicationScopeService scope)
    {
        var graphics = scope.Document.Map.Display.GraphicsContainer.Elements;
        graphics.Remove(typeof(GraphicNetworkResultEdge));
        graphics.Remove(typeof(GraphicNetworkResultNode));
        graphics.Remove(typeof(GraphicNetworkResultPoints));
    }

    /// <summary>The geometries of the drawn result in the display's spatial reference, by index in the result.</summary>
    private sealed record DrawnResult(NetworkQueryResult Result, Dictionary<int, IPolyline> Edges, Dictionary<int, IPoint> Nodes, Dictionary<int, IPoint> Violations);

    /// <summary>The result drawn last, so legend rows can be hidden and recoloured without reading the lines again.</summary>
    private DrawnResult? _drawn;

    /// <summary>The edges as the parts of their lines, nodes and violations as points, in the display's spatial reference.</summary>
    private async Task Draw(ICartoApplicationScopeService scope, NetworkQueryResult result, CancellationToken cancellationToken = default)
    {
        RemoveResultGraphics(scope);

        var map = scope.Document.Map;
        var networkSrs = string.IsNullOrEmpty(result.Srs) ? scope.DisplayService.SpatialReference : SpatialReference.FromID(result.Srs);
        IGeometry ToDisplay(IGeometry geometry)
            => scope.GeoTransformer.Transform(geometry, networkSrs, scope.DisplayService.SpatialReference, scope.DisplayService.DatumTransformations);

        var drawn = new DrawnResult(result, [], [], []);
        var coordinates = await NetworkResultGeometry.EdgeCoordinatesAsync(map, result, cancellationToken);
        for (int i = 0; i < result.Edges.Count; i++)
        {
            var edge = result.Edges[i];
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
                drawn.Edges[i] = displayed;
            }
        }
        // points in one transformation each: a transformer per point is far too slow for many (build problems: 100,000s)
        var nodeIndexes = Enumerable.Range(0, result.Nodes.Count)
            .Where(i => result.Nodes[i].AliasOf.Length == 0 && !double.IsNaN(result.Nodes[i].X))
            .ToList();
        var violations = result.Violations;
        var (nodePoints, violationPoints) = await Task.Run(() => (
            ToDisplayPoints(scope, networkSrs, nodeIndexes.Select(i => (result.Nodes[i].X, result.Nodes[i].Y)).ToList()),
            ToDisplayPoints(scope, networkSrs, violations.Select(v => (v.X, v.Y)).ToList())), cancellationToken);
        for (int k = 0; k < nodeIndexes.Count; k++)
        {
            if (nodePoints[k] is IPoint point)
            {
                drawn.Nodes[nodeIndexes[k]] = point;
            }
        }
        for (int i = 0; i < violationPoints.Count; i++)
        {
            if (violationPoints[i] is IPoint point)
            {
                drawn.Violations[i] = point;
            }
        }
        _drawn = drawn;

        AddResultGraphics(scope);
        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Graphics);
    }

    /// <summary>The drawn result with the colours of the legend, without the hidden rows; lines first, dashed ones below.</summary>
    private void AddResultGraphics(ICartoApplicationScopeService scope)
    {
        RemoveResultGraphics(scope);
        if (_drawn is null || _context?.Legend is not NetworkResultLegend legend || legend.Result != _drawn.Result)
        {
            return;
        }
        var graphics = scope.Document.Map.Display.GraphicsContainer.Elements;
        var visible = legend.Entries.Where(e => !_context.HiddenKeys.Contains(e.Key)).ToList();
        foreach (var entry in visible.Where(e => e.Shape == NetworkLegendShape.Edge).OrderBy(e => e.Dashed ? 0 : 1))
        {
            foreach (var i in entry.Edges)
            {
                if (_drawn.Edges.TryGetValue(i, out var polyline))
                {
                    graphics.Add(new GraphicNetworkResultEdge(polyline, entry.Color, entry.Size, entry.Dashed));
                }
            }
        }
        // the small markers first, the large ones (problems) on top; one element per row, it draws only what is in
        // the map and one marker per spot on the screen
        foreach (var entry in visible.Where(e => e.Shape != NetworkLegendShape.Edge).OrderBy(e => e.Size))
        {
            var points = new List<IPoint>(entry.Nodes.Count + entry.Violations.Count);
            foreach (var i in entry.Nodes)
            {
                if (_drawn.Nodes.TryGetValue(i, out var point))
                {
                    points.Add(point);
                }
            }
            foreach (var i in entry.Violations)
            {
                if (_drawn.Violations.TryGetValue(i, out var point))
                {
                    points.Add(point);
                }
            }
            if (points.Count > 0)
            {
                graphics.Add(new GraphicNetworkResultPoints(points, entry.Color, entry.Size));
            }
        }
    }

    /// <summary>
    /// Points from the spatial reference of the network into the display's, in one transformation; null where a point
    /// can't be transformed.
    /// </summary>
    private static IReadOnlyList<IPoint?> ToDisplayPoints(ICartoApplicationScopeService scope, ISpatialReference? networkSrs, IReadOnlyList<(double X, double Y)> xy)
    {
        var points = new IPoint?[xy.Count];
        if (xy.Count == 0)
        {
            return points;
        }
        var displaySrs = scope.DisplayService.SpatialReference;
        var multi = new MultiPoint();
        foreach (var (x, y) in xy)
        {
            multi.AddPoint(new Point(x, y));
        }
        if (scope.GeoTransformer.Transform(multi, networkSrs, displaySrs, scope.DisplayService.DatumTransformations) is IPointCollection transformed
            && transformed.PointCount == xy.Count)
        {
            for (int i = 0; i < xy.Count; i++)
            {
                var p = transformed[i];
                points[i] = double.IsFinite(p.X) && double.IsFinite(p.Y) ? new Point(p.X, p.Y) : null;
            }
            return points;
        }
        // the transformation dropped points: one by one
        for (int i = 0; i < xy.Count; i++)
        {
            points[i] = scope.GeoTransformer.Transform(new Point(xy[i].X, xy[i].Y), networkSrs, displaySrs, scope.DisplayService.DatumTransformations) as IPoint;
        }
        return points;
    }

    #endregion
}
