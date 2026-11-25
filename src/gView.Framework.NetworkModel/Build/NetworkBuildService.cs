using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Store;
using gView.Framework.NetworkModel.Templates;
using gView.Framework.NetworkModel.Types;
using SpatialNET;
using SpatialNET.Network;
using SpatialNET.Network.Building;
using System.Globalization;

namespace gView.Framework.NetworkModel.Build;

/// <summary>The outcome of a build.</summary>
public sealed record NetworkBuildReport(
    NetworkMetadata Metadata, IReadOnlyList<BuildDiagnostic> Diagnostics, IReadOnlyList<string> Warnings,
    IReadOnlyList<DuplicatePoint> Duplicates, IReadOnlyList<MergedPoint> Merged);

/// <summary>A point left out because another point of the same layer lies within the tolerance.</summary>
public sealed record DuplicatePoint(string Layer, long KeptOid, long DroppedOid, double X, double Y);

/// <summary>A point merged into the point of another layer within the tolerance (<see cref="NodeConflictMode.PriorityByToc"/>).</summary>
public sealed record MergedPoint(string KeptLayer, long KeptOid, string MergedLayer, long MergedOid, double X, double Y);

/// <summary>
/// Builds a network from the layers of a map as the config defines it: reads points and lines, gives them types,
/// ids and attributes, lets the SpatialNET builder join, split and shard them, and writes the result to the store.
/// </summary>
public sealed class NetworkBuildService
{
    private readonly Action<string> _log;

    /// <summary>The warnings of the last build, also when it failed.</summary>
    public List<string> Warnings { get; } = [];

    /// <summary>The points left out as duplicates in the last build, also when it failed.</summary>
    public List<DuplicatePoint> Duplicates { get; } = [];

    /// <summary>The points merged into the point of another layer in the last build.</summary>
    public List<MergedPoint> Merged { get; } = [];

    /// <summary>The connect rules of the last build once resolved; null without rules. Names the rule of reported connections.</summary>
    internal NetworkConnectRules? ConnectRules { get; private set; }

    /// <summary>The metadata of the last build as far as it got, e.g. to name the feature classes of conflicts.</summary>
    public NetworkMetadata? Metadata { get; private set; }

    /// <summary>The layer path of a feature class index of the last build.</summary>
    public string LayerOf(int featureClassIndex)
        => Metadata?.FeatureClasses.FirstOrDefault(f => f.Index == featureClassIndex)?.Layer ?? featureClassIndex.ToString();

    public NetworkBuildService(Action<string>? log = null) => _log = log ?? (_ => { });

    public async Task<NetworkBuildReport> BuildAsync(IMap map, NetworkConfig config, INetworkStore store, CancellationToken cancellationToken = default)
    {
        config.Validate();

        var previous = store.ReadMetadata();
        var srs = ResolveSpatialReference(map, config);
        var datumTransformations = map.Display?.DatumTransformations;
        var layers = MapLayers.FeatureLayers(map);
        var warnings = Warnings;
        var duplicates = Duplicates;
        warnings.Clear();
        duplicates.Clear();
        Merged.Clear();

        var types = new TypeCatalog(config.Facets, previous);
        var weights = ResolveWeights(config, previous);

        var metadata = new NetworkMetadata
        {
            Name = config.Name,
            Created = DateTime.UtcNow,
            Srs = srs?.Name,
            Tolerance = config.Tolerance,
            Weights = weights.Select(w => new NetworkWeightMetadata { Id = w.Id, Name = w.Name, Kind = w.Kind, Fallback = w.Fallback }).ToList(),
            ShardKeys = new(previous?.ShardKeys ?? [], StringComparer.OrdinalIgnoreCase),
            Rules = config.Rules,
            Tiers = config.Tiers,
            SupplySources = config.SupplySources,
            Customers = config.Customers
        };
        Metadata = metadata;

        // feature class indices: stable by layer path
        int FeatureClassIndex(MapLayer layer, ElementKind kind)
        {
            var existing = metadata.FeatureClasses.FirstOrDefault(f => f.Layer.Equals(layer.Path, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                return existing.Index;
            }
            var prev = previous?.FeatureClasses.FirstOrDefault(f => f.Layer.Equals(layer.Path, StringComparison.OrdinalIgnoreCase));
            int index = prev is not null && !metadata.FeatureClasses.Any(f => f.Index == prev.Index)
                ? prev.Index
                : Enumerable.Range(0, NetworkIds.MaxFeatureClass + 1)
                    .First(i => !metadata.FeatureClasses.Any(f => f.Index == i)
                             && !(previous?.FeatureClasses.Any(f => f.Index == i) ?? false));
            metadata.FeatureClasses.Add(new NetworkFeatureClassMetadata
            {
                Index = index,
                Layer = layer.Path,
                FeatureClass = layer.Layer.FeatureClass.Name,
                Kind = kind
            });
            return index;
        }

        var options = new NetworkBuildOptions
        {
            Tolerance = config.Tolerance,
            FallbackShardCellSize = config.FallbackShardCellSize,
            Weights = weights,
            ShardRegions = config.Shards is null
                ? []
                : await ReadShardRegions(MapLayers.Find(layers, config.Shards.LayerId, config.Shards.Layer), config.Shards, metadata, srs, datumTransformations, warnings, cancellationToken)
        };
        _log($"Shard regions: {options.ShardRegions.Count}");

        // the connection rules are resolved once all types are known; the builder asks them only while building
        NetworkConnectRules? connectRules = null;
        ConnectRules = null;
        bool rulesNeeded = NetworkConnectRules.Needed(config);
        options = NetworkConnectRules.Attach(options, config, () => connectRules);

        var ids = new NetworkIds(config.Tolerance);
        var builder = new NetworkBuilder<long, long, FeatureRef>(options, ids);
        var tocOrderOf = new Dictionary<int, int>();
        builder.ResolveCoincidentNodes = NodeConflictResolver(config, () => connectRules, group => ResolveByToc(group, tocOrderOf));

        // nodes first, then lines
        foreach (var nodeConfig in config.Nodes)
        {
            var layer = MapLayers.Find(layers, nodeConfig.LayerId, nodeConfig.Layer);
            int fcIndex = FeatureClassIndex(layer, ElementKind.Node);
            tocOrderOf[fcIndex] = layer.TocOrder;
            var typeTemplates = nodeConfig.Type.Select(t => ValueTemplate.Parse(t, layer.Name, layer.GroupPath)).ToArray();
            var texts = new string[typeTemplates.Length];
            int count = 0, skipped = 0;
            var points = new List<(long Oid, Coordinate Location, int TypeId, bool Blocked)>();

            await ForEachFeature(layer, nodeConfig.Where, srs, datumTransformations, cancellationToken, feature =>
            {
                var location = feature.Shape switch
                {
                    IPoint p => new Coordinate(p.X, p.Y),
                    IMultiPoint mp when mp.PointCount > 0 => new Coordinate(mp[0].X, mp[0].Y),
                    _ => (Coordinate?)null
                };
                if (location is null)
                {
                    skipped++;
                    return;
                }

                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i] = typeTemplates[i].Evaluate(f => FieldText(feature, f));
                }
                int typeId = types.Resolve(ElementKind.Node, texts);
                bool blocked = nodeConfig.Blocked is not null && nodeConfig.Blocked.Matches(FieldText(feature, nodeConfig.Blocked.Field));

                points.Add((feature.OID, location.Value, typeId, blocked));
            });

            // points of the same layer within the tolerance are duplicates: the lowest object id stays, the others are left out
            var kept = nodeConfig.RemoveDuplicates
                ? RemoveDuplicates(points, config.Tolerance, layer.Path, duplicates)
                : points;
            foreach (var point in kept)
            {
                var source = new FeatureRef(fcIndex, point.Oid);
                builder.AddNode(source, NetworkIds.NodeId(source), point.Location, point.TypeId, point.Blocked);
                count++;
            }

            _log($"Nodes  {layer.Path} (TOC {layer.TocOrder}): {count}{(points.Count > count ? $" ({points.Count - count} duplicates left out)" : "")}");
            if (skipped > 0)
            {
                warnings.Add($"{layer.Path}: {skipped} feature(s) without point geometry skipped.");
            }
        }

        foreach (var edgeConfig in config.Edges)
        {
            var layer = MapLayers.Find(layers, edgeConfig.LayerId, edgeConfig.Layer);
            int fcIndex = FeatureClassIndex(layer, ElementKind.Edge);
            var typeTemplates = edgeConfig.Type.Select(t => ValueTemplate.Parse(t, layer.Name, layer.GroupPath)).ToArray();
            var lengthTemplate = string.IsNullOrWhiteSpace(edgeConfig.Length) ? null : ValueTemplate.Parse(edgeConfig.Length, layer.Name, layer.GroupPath);
            var directionTemplate = string.IsNullOrWhiteSpace(edgeConfig.Direction) ? null : ValueTemplate.Parse(edgeConfig.Direction, layer.Name, layer.GroupPath);
            var weightTemplates = edgeConfig.Weights
                .Select(w => (Definition: weights.First(d => d.Name.Equals(w.Key, StringComparison.OrdinalIgnoreCase)),
                              Template: ValueTemplate.Parse(w.Value, layer.Name, layer.GroupPath)))
                .ToArray();
            var texts = new string[typeTemplates.Length];
            int count = 0, skipped = 0, badValues = 0;

            await ForEachFeature(layer, edgeConfig.Where, srs, datumTransformations, cancellationToken, feature =>
            {
                if (feature.Shape is not SpatialNET.IPolyline2D line || feature.Shape is not IPolyline polyline || polyline.PathCount == 0)
                {
                    skipped++;
                    return;
                }

                string? Field(string name) => FieldText(feature, name);

                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i] = typeTemplates[i].Evaluate(Field);
                }
                int typeId = types.Resolve(ElementKind.Edge, texts);

                double? length = null;
                if (lengthTemplate is not null)
                {
                    var text = lengthTemplate.Evaluate(Field);
                    if (TryParseNumber(text, out var value) && value > 0)
                    {
                        length = value;
                    }
                    else if (text.Length > 0)
                    {
                        badValues++;
                    }
                }

                var direction = EdgeDirection.Both;
                if (directionTemplate is not null)
                {
                    var text = directionTemplate.Evaluate(Field);
                    if (text.Length > 0 && !Enum.TryParse(text, true, out direction))
                    {
                        direction = EdgeDirection.Both;
                        badValues++;
                    }
                }

                List<EdgeWeight>? edgeWeights = null;
                foreach (var (definition, template) in weightTemplates)
                {
                    var text = template.Evaluate(Field);
                    if (TryParseNumber(text, out var value))
                    {
                        (edgeWeights ??= []).Add(new EdgeWeight(definition.Id, value));
                    }
                    else if (text.Length > 0)
                    {
                        badValues++;
                    }
                }

                bool disabled = edgeConfig.Disabled is not null && edgeConfig.Disabled.Matches(Field(edgeConfig.Disabled.Field));

                builder.AddLine(new FeatureRef(fcIndex, feature.OID), line, typeId, new LineAttributes(direction, disabled, length, edgeWeights));
                count++;
            });

            _log($"Lines  {layer.Path}: {count}");
            if (skipped > 0)
            {
                warnings.Add($"{layer.Path}: {skipped} feature(s) without line geometry skipped.");
            }
            if (badValues > 0)
            {
                warnings.Add($"{layer.Path}: {badValues} value(s) for length, direction or weights could not be read and were ignored.");
            }
        }

        // the type registry is complete only now (with the helper types of the tiers); SpatialNET checks types against it
        // when the network is used
        if (rulesNeeded)
        {
            connectRules = NetworkConnectRules.Create(config, types);
            ConnectRules = connectRules;
        }
        metadata.Facets = types.FacetMetadata().ToList();
        metadata.Types = types.TypeMetadata().ToList();
        var registry = types.ToRegistry(); // validates the facets and types
        var tiers = config.ResolveTiers(registry, checkRules: false); // and the tiers
        // the rules are copied into network.json but never stop the build (validate checks them)
        foreach (var problem in config.RuleProblems(registry, tiers))
        {
            warnings.Add($"Validation rules: {problem} (validate fails until it is fixed)");
        }
        connectRules?.Resolve(config, registry);
        if (tiers.Count > 0)
        {
            _log($"Tiers: {string.Join(", ", tiers.Select(t => t.Name))}{(config.SeparateTiers ? " (separate)" : "")}, connect rules: {config.ConnectRules.Count(r => r.Enabled)}{(config.ConnectRules.Any(r => !r.Enabled) ? $" ({config.ConnectRules.Count(r => !r.Enabled)} off)" : "")}");
        }

        using var writer = store.CreateWriter();
        var progress = new Progress<BuildProgress>(p =>
        {
            if (p.Done == 0 || p.Done == p.Total)
            {
                _log($"  {p.Phase}: {p.Done}/{p.Total}");
            }
        });

        var result = builder.Build(writer, progress, cancellationToken);
        writer.Commit(metadata);

        _log($"Built: {result.NodeCount} nodes, {result.EdgeCount} edges, {metadata.Shards.Count} shards, {result.Diagnostics.Count} diagnostics");
        foreach (var group in result.Diagnostics.GroupBy(d => d.Kind).OrderBy(g => g.Key))
        {
            _log($"  {group.Key}: {group.Count()}");
        }

        return new NetworkBuildReport(metadata, result.Diagnostics, warnings, duplicates, Merged);
    }

    /// <summary>
    /// How points within the tolerance of each other are resolved: with <see cref="NodeConflictMode.PriorityByToc"/> by the
    /// TOC. Otherwise (<see cref="NodeConflictMode.Error"/>) they are a conflict, except with separate tiers: points of
    /// different tiers that a line end connects to are merged by the TOC in every mode (e.g. a transformer of MS that is a
    /// source of NS and an NS box at its place); points of the same tier stay a conflict.
    /// </summary>
    internal static CoincidentNodesResolver<long, FeatureRef>? NodeConflictResolver(
        NetworkConfig config, Func<NetworkConnectRules?> rules, Func<IReadOnlyList<CoincidentNode<long, FeatureRef>>, int> byToc)
    {
        if (config.NodeConflicts == NodeConflictMode.PriorityByToc)
        {
            return group => byToc(group);
        }
        if (config.SeparateTiers)
        {
            return group => SamePartitionTwice(group, rules()) ? -1 : byToc(group);
        }
        return null;
    }

    /// <summary>Whether two points of a group of coincident points are in the same partition (tier), i.e. a real conflict.</summary>
    internal static bool SamePartitionTwice(IReadOnlyList<CoincidentNode<long, FeatureRef>> group, NetworkConnectRules? rules)
    {
        if (rules is null)
        {
            return true;
        }
        var partitions = new HashSet<int>();
        foreach (var node in group)
        {
            if (!partitions.Add(rules.Partition(node.TypeId)))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>The point of the layer highest in the TOC stays, within a layer the lowest object id; the others are kept as merged.</summary>
    private int ResolveByToc(IReadOnlyList<CoincidentNode<long, FeatureRef>> group, Dictionary<int, int> tocOrderOf)
    {
        int winner = 0;
        for (int i = 1; i < group.Count; i++)
        {
            if (Rank(group[i]).CompareTo(Rank(group[winner])) < 0)
            {
                winner = i;
            }
        }

        var kept = group[winner].Source;
        for (int i = 0; i < group.Count; i++)
        {
            if (i != winner)
            {
                var merged = group[i];
                Merged.Add(new MergedPoint(LayerOf(kept.FeatureClass), kept.Oid, LayerOf(merged.Source.FeatureClass), merged.Source.Oid, merged.Location.X, merged.Location.Y));
            }
        }
        return winner;

        (int, int, long) Rank(CoincidentNode<long, FeatureRef> node)
            => (tocOrderOf.GetValueOrDefault(node.Source.FeatureClass, int.MaxValue), node.Source.FeatureClass, node.Source.Oid);
    }

    internal static List<(long Oid, Coordinate Location, int TypeId, bool Blocked)> RemoveDuplicates(
        List<(long Oid, Coordinate Location, int TypeId, bool Blocked)> points, double tolerance, string layerPath, List<DuplicatePoint> duplicates)
    {
        // sorted by object id, so the result does not depend on the order of the data
        points.Sort((a, b) => a.Oid.CompareTo(b.Oid));

        var grid = new Dictionary<(long, long), List<int>>();
        var kept = new List<(long Oid, Coordinate Location, int TypeId, bool Blocked)>(points.Count);
        double toleranceSquared = tolerance * tolerance;

        foreach (var point in points)
        {
            long cx = (long)Math.Floor(point.Location.X / tolerance), cy = (long)Math.Floor(point.Location.Y / tolerance);
            int? duplicateOf = null;
            for (long dx = -1; dx <= 1 && duplicateOf is null; dx++)
            {
                for (long dy = -1; dy <= 1 && duplicateOf is null; dy++)
                {
                    if (!grid.TryGetValue((cx + dx, cy + dy), out var cell))
                    {
                        continue;
                    }
                    foreach (var index in cell)
                    {
                        double ddx = kept[index].Location.X - point.Location.X, ddy = kept[index].Location.Y - point.Location.Y;
                        if (ddx * ddx + ddy * ddy <= toleranceSquared)
                        {
                            duplicateOf = index;
                            break;
                        }
                    }
                }
            }

            if (duplicateOf is int keptIndex)
            {
                duplicates.Add(new DuplicatePoint(layerPath, kept[keptIndex].Oid, point.Oid, point.Location.X, point.Location.Y));
                continue;
            }

            if (!grid.TryGetValue((cx, cy), out var own))
            {
                grid[(cx, cy)] = own = [];
            }
            own.Add(kept.Count);
            kept.Add(point);
        }
        return kept;
    }

    private static ISpatialReference? ResolveSpatialReference(IMap map, NetworkConfig config)
    {
        if (!string.IsNullOrWhiteSpace(config.Srs))
        {
            return SpatialReference.FromID(config.Srs) ?? throw new InvalidDataException($"Unknown spatial reference '{config.Srs}'.");
        }
        return map.Display?.SpatialReference;
    }

    private static List<WeightDefinition> ResolveWeights(NetworkConfig config, NetworkMetadata? previous)
    {
        var result = new List<WeightDefinition>();
        foreach (var weight in config.Weights)
        {
            var prev = previous?.Weights.FirstOrDefault(w => w.Name.Equals(weight.Name, StringComparison.OrdinalIgnoreCase));
            int id = prev is not null && !result.Any(w => w.Id == prev.Id)
                ? prev.Id
                : Math.Max(result.Select(w => w.Id).DefaultIfEmpty(0).Max(), previous?.Weights.Select(w => w.Id).DefaultIfEmpty(0).Max() ?? 0) + 1;
            result.Add(new WeightDefinition(id, weight.Name, weight.Kind, weight.Fallback));
        }
        return result;
    }

    private async Task<IReadOnlyList<ShardRegion>> ReadShardRegions(
        MapLayer layer, ShardConfig shardConfig, NetworkMetadata metadata,
        ISpatialReference? srs, IDatumTransformations? datumTransformations, List<string> warnings, CancellationToken cancellationToken)
    {
        var regions = new List<ShardRegion>();
        int skipped = 0;

        await ForEachFeature(layer, shardConfig.Where, srs, datumTransformations, cancellationToken, feature =>
        {
            var key = FieldText(feature, shardConfig.Field);
            if (feature.Shape is not SpatialNET.IPolygon2D polygon || string.IsNullOrEmpty(key))
            {
                skipped++;
                return;
            }
            regions.Add(new ShardRegion(polygon, ShardId(key, metadata)));
        });

        if (skipped > 0)
        {
            warnings.Add($"{layer.Path}: {skipped} shard polygon(s) without geometry or {shardConfig.Field} skipped.");
        }
        return regions;
    }

    /// <summary>
    /// A numeric key is the shard id itself; any other key gets an id from long.MinValue upwards, kept in the metadata.
    /// That band is out of reach of the grid cell keys of the builder ((cx &lt;&lt; 32) ^ cy), which would need cells
    /// around 2^31 * cell size away.
    /// </summary>
    private const long TextKeyBandEnd = long.MinValue + (1L << 40);

    private static long ShardId(string key, NetworkMetadata metadata)
    {
        if (long.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            return id;
        }
        if (TryParseNumber(key, out var number) && number == Math.Floor(number) && Math.Abs(number) < long.MaxValue)
        {
            return (long)number;
        }
        if (!metadata.ShardKeys.TryGetValue(key, out id))
        {
            id = metadata.ShardKeys.Values.Where(v => v < TextKeyBandEnd).DefaultIfEmpty(long.MinValue).Max() + 1;
            metadata.ShardKeys[key] = id;
        }
        return id;
    }

    private static async Task ForEachFeature(
        MapLayer layer, string? where, ISpatialReference? srs, IDatumTransformations? datumTransformations,
        CancellationToken cancellationToken, Action<IFeature> action)
    {
        var filter = new QueryFilter { SubFields = "*" };
        var clauses = new[] { layer.Layer.FilterQuery?.WhereClause, where }.Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
        if (clauses.Length > 0)
        {
            filter.WhereClause = string.Join(" AND ", clauses.Select(c => $"({c})"));
        }
        if (srs is not null)
        {
            filter.SetFeatureSpatialReference(srs, datumTransformations);
        }

        using IFeatureCursor cursor = await layer.Layer.FeatureClass.GetFeatures(filter)
            ?? throw new InvalidOperationException($"Query on layer '{layer.Path}' failed.");

        IFeature feature;
        while ((feature = await cursor.NextFeature()) is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            action(feature);
        }
    }

    private static string? FieldText(IFeature feature, string fieldName)
    {
        var value = feature[fieldName];
        return value switch
        {
            null or DBNull => null,
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    private static bool TryParseNumber(string? text, out double value)
        => double.TryParse(text?.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
