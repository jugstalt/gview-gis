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
    private static readonly ArgbColor EdgeColor = ArgbColor.FromArgb(200, 0, 120, 255);
    private static readonly ArgbColor NodeColor = ArgbColor.FromArgb(230, 0, 90, 200);
    private static readonly ArgbColor ProblemColor = ArgbColor.FromArgb(230, 220, 30, 30);
    private static readonly ArgbColor DeadColor = ArgbColor.FromArgb(220, 255, 150, 0);

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
            _context.Tiers = graph.Metadata.Tiers.OrderBy(t => t.Order).Select(t => t.Name).ToList();
            if (!string.IsNullOrEmpty(_context.Tier))
            {
                // tiers that are gone after a refresh drop out of the selection
                var kept = _context.Tier.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(t => _context.Tiers.Contains(t, StringComparer.OrdinalIgnoreCase));
                _context.Tier = string.Join(",", kept);
            }
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
                NetworkSearchEventArgs { Action: NetworkSearchAction.ExportGeoJson } => await ExportGeoJson(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ZoomToViolation } a => await ZoomToViolation(scope, a.Index),
                NetworkSearchEventArgs { Action: NetworkSearchAction.RemovePick } a => await RemovePick(scope, a.Index, a.Target),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ZoomToPick } a => await ZoomToPick(scope, a.Index, a.Target),
                NetworkSearchEventArgs { Action: NetworkSearchAction.AreaFromExtent } => await AreaFromExtent(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.AreaFromSelection } => await AreaFromSelection(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.RemoveArea } => await RemoveArea(scope),
                NetworkSearchEventArgs { Action: NetworkSearchAction.ZoomToArea } => await ZoomToArea(scope),
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
            var hit = graph.Nearest(center.X, center.Y, radius).FirstOrDefault();
            if (hit == default)
            {
                _context.Info = "No network node at this location.";
                return false;
            }
            var entry = hit.Entry;
            bool helper = NetworkIds.IsHelper(entry.Node);
            // edge ends have no feature: they are taken by their location; real nodes and merged points by Layer#OID
            reference = helper ? NetworkLocations.Reference(entry.X, entry.Y) : graph.NodeLabel(entry.Id);
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
        if (_context.Pick == NetworkSearchPick.Start)
        {
            Pick(_context.StartItems, _context.Start, reference, label, ctrl && _context.StartAllowsMany);
            (_context.Start, _context.StartLabel) = Summary(_context.StartItems);
            _context.Info = null; // the picked items are listed in the report
            if (_context.Operation == "path")
            {
                _context.Pick = NetworkSearchPick.Target;
            }
        }
        else
        {
            Pick(_context.TargetItems, _context.Target, reference, label, ctrl && _context.TargetAllowsMany);
            (_context.Target, _context.TargetLabel) = Summary(_context.TargetItems);
            _context.Info = null;
        }
        DrawPicks(scope);

        await scope.EventBus.FireRefreshMapAsync(DrawPhase.Graphics);
        return true;
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
        var graph = Graph(scope);
        var config = Module(scope).Config;
        // with a tier its sources and sinks count, not the defaults of the module
        var tier = context.Operation == "info" ? null : context.Tier.NullIfEmpty();
        var request = new NetworkQueryRequest
        {
            Tier = tier,
            StopAtSources = context.Operation == "trace" && tier is not null ? context.StopAtSources : null,
            Start = context.Operation is "trace" or "reach" ? context.Start : null,
            From = context.Operation == "path" ? context.Start : null,
            To = context.Operation == "path" ? context.Target : null,
            // supply and outage: the defaults of the map's network module (they may differ from the built network)
            Sources = context.Operation switch
            {
                "supply" => context.Start.NullIfEmpty() ?? (tier is null ? config.SupplySources : null),
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

        _context!.Result = null;
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

        var coordinates = await NetworkResultGeometry.EdgeCoordinatesAsync(scope.Document.Map, result,
            snap: _context.SnapExport, tolerance: Graph(scope).Metadata.Tolerance);
        var directory = System.IO.Path.Combine(_context.NetworkDirectory, "results");
        Directory.CreateDirectory(directory);
        var path = System.IO.Path.Combine(directory, $"{result.Operation}-{DateTime.Now:yyyyMMdd-HHmmss}.geojson");
        await File.WriteAllTextAsync(path, NetworkResultWriter.ToGeoJson(result, e => coordinates.TryGetValue(e.Id, out var c) ? c : null));

        _context.Info = $"Written: {path}";
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
        return await ZoomToNetworkPoints(scope, result.Srs, [(violation.X, violation.Y)]);
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
            throw new InvalidOperationException($"The network is not built yet: build it on the network module page (Map, Modules, Network, tab Build) ({directory}).");
        }
        _context!.NetworkDirectory = directory;
        return NetworkGraphCache.Shared.Get(directory);
    }


    private static void RemoveResultGraphics(ICartoApplicationScopeService scope)
    {
        var graphics = scope.Document.Map.Display.GraphicsContainer.Elements;
        graphics.Remove(typeof(GraphicNetworkResultEdge));
        graphics.Remove(typeof(GraphicNetworkResultNode));
    }

    /// <summary>The edges as the parts of their lines, nodes and violations as points, in the display's spatial reference.</summary>
    private static async Task Draw(ICartoApplicationScopeService scope, NetworkQueryResult result, CancellationToken cancellationToken = default)
    {
        RemoveResultGraphics(scope);

        var map = scope.Document.Map;
        var graphics = map.Display.GraphicsContainer.Elements;
        var networkSrs = string.IsNullOrEmpty(result.Srs) ? scope.DisplayService.SpatialReference : SpatialReference.FromID(result.Srs);
        IGeometry ToDisplay(IGeometry geometry)
            => scope.GeoTransformer.Transform(geometry, networkSrs, scope.DisplayService.SpatialReference, scope.DisplayService.DatumTransformations);

        var coordinates = await NetworkResultGeometry.EdgeCoordinatesAsync(map, result, cancellationToken);
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
                graphics.Add(new GraphicNetworkResultEdge(displayed, edge.Kind switch
                {
                    "Disabled" => ProblemColor,
                    "Dead" => DeadColor,
                    _ => EdgeColor
                }));
            }
        }

        foreach (var node in result.Nodes.Where(n => n.AliasOf.Length == 0 && !double.IsNaN(n.X)))
        {
            var problem = node.Kind is "NotSupplied" or "Multiple" or "Blocked" or "Affected";
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
