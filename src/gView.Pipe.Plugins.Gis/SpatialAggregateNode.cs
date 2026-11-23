using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Data;
using gView.Pipe.Abstraction.Nodes;
using Microsoft.Extensions.Logging;
using SpatialNET;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis;

public enum GisAggregateOperation
{
    [PipeEnumValue("Merges the geometries per group of equal field values into one feature, e.g. all lines of a feeder; with aggregates of fields")]
    Dissolve,

    [PipeEnumValue("Merges all geometries into one feature without properties", Label = "Union all")]
    UnionAll,

    [PipeEnumValue("One cell per point: the area closer to it than to any other point; the cell keeps the properties of its point")]
    Voronoi,

    [PipeEnumValue("Triangles between all vertices, one feature per triangle")]
    Delaunay,

    [PipeEnumValue("The smallest convex polygon around all geometries", Label = "Convex hull")]
    ConvexHull,

    [PipeEnumValue("A tighter polygon around all geometries, e.g. around the houses of a village", Label = "Concave hull")]
    ConcaveHull,

    [PipeEnumValue("Joins lines that meet at their ends into longer lines", Label = "Line merge")]
    LineMerge,

    [PipeEnumValue("One polygon per area enclosed by the lines, e.g. parcels from boundary lines; lines that enclose nothing are left out")]
    Polygonize
}

public sealed class SpatialAggregateParameters
{
    [PipeParameter(Label = "Operation", Order = 1, Expressions = false, Description = "What to compute from all input items together")]
    public GisAggregateOperation Operation { get; set; } = GisAggregateOperation.Dissolve;

    [PipeParameter(Label = "Group by", Order = 2, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["dissolve"],
        Description = "Properties whose equal values form a group, e.g. feeder; empty merges everything into one feature")]
    public List<string> GroupBy { get; set; } = new();

    [PipeParameter(Label = "Aggregates", Order = 3, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["dissolve"],
        Description = "Output property → function, e.g. lines → count, total → sum(length); functions count, sum, min, max, avg, first, list")]
    public Dictionary<string, string> Aggregates { get; set; } = new();

    [PipeParameter(Label = "Edge length ratio", Order = 4, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["concaveHull"],
        Description = "Between 0 and 1: 1 is the convex hull, smaller values follow the shape more closely, e.g. 0.3")]
    [Range(0.0, 1.0)]
    public double EdgeLengthRatio { get; set; } = 0.3;

    [PipeParameter(Label = "Clip geometry", Order = 5, ShowWhen = nameof(Operation), ShowWhenValues = ["voronoi"],
        Description = "Polygon (GeoJSON geometry or feature) or bounding box [minx, miny, maxx, maxy] the cells are cut to; empty uses an extent around the points")]
    public JsonNode? ClipGeometry { get; set; }

    [PipeParameter(Label = "Clip geometry SRS", Order = 6, ShowWhen = nameof(Operation), ShowWhenValues = ["voronoi"],
        Description = "EPSG code of the clip geometry, e.g. 31256; it must be the srs of the items, empty takes it as that")]
    public JsonNode? ClipSrs { get; set; }
}

[PipeNode("9c4f1e7a-3d6b-4a2e-8f5c-b7a1d3e9f046", "Spatial Aggregate",
    Category = "GIS",
    Description = "Computes new features from all GeoJSON items together: dissolve by fields with aggregates, union, "
                  + "Voronoi cells, Delaunay triangles, hulls, line merge. Works on the coordinates of the items with SpatialNET.")]
[PipeNodeExamples]
public sealed class SpatialAggregateNode : IPipeNode<SpatialAggregateParameters>
{
    static private readonly Regex AggregatePattern = new(@"^\s*(?<function>[a-z]+)\s*(\(\s*(?<field>[^)]*?)\s*\))?\s*$", RegexOptions.IgnoreCase);

    public Task<PipeNodeResult> ExecuteAsync(IPipeNodeContext context, CancellationToken cancellationToken)
    {
        var items = context.GetInput();
        var parameters = context.GetParameters<SpatialAggregateParameters>(items.Count > 0 ? items[0] : null);
        var srs = GisSpatial.CommonSrs(items);
        var factory = GisSpatial.Factory;

        var features = items.Select(item => (Item: item, Geometry: GisSpatial.Geometry(item)))
            .Where(f => f.Geometry is not null)
            .Select(f => (f.Item, Geometry: f.Geometry!))
            .ToList();

        List<PipeItem> output;
        switch (parameters.Operation)
        {
            case GisAggregateOperation.Dissolve:
                output = Dissolve(items, parameters, srs, cancellationToken);
                break;
            case GisAggregateOperation.UnionAll:
                output = features.Count == 0 ? [] : [GisSpatial.Feature(features.Select(f => f.Geometry).UnionAll(factory: factory), new JsonObject(), srs)];
                break;
            case GisAggregateOperation.Voronoi:
                output = Voronoi(features, srs, Clip(parameters, srs));
                break;
            case GisAggregateOperation.Delaunay:
                output = features.Count == 0 ? []
                    : GisSpatial.MultiPoint(features.SelectMany(f => GisSpatial.Coordinates(f.Geometry))).DelaunayTriangles(factory)
                        .Select(triangle => GisSpatial.Feature(triangle, new JsonObject(), srs)).ToList();
                break;
            case GisAggregateOperation.ConvexHull:
                output = features.Count == 0 ? []
                    : [GisSpatial.Feature(GisSpatial.MultiPoint(features.SelectMany(f => GisSpatial.Coordinates(f.Geometry))).ConvexHull(factory), new JsonObject(), srs)];
                break;
            case GisAggregateOperation.ConcaveHull:
                output = features.Count == 0 ? []
                    : [GisSpatial.Feature(GisSpatial.MultiPoint(features.SelectMany(f => GisSpatial.Coordinates(f.Geometry))).ConcaveHullByRatio(parameters.EdgeLengthRatio, factory: factory), new JsonObject(), srs)];
                break;
            case GisAggregateOperation.LineMerge:
                var lines = features.Select(f => f.Geometry).OfType<IPolyline2D>().ToList();
                output = lines.Count == 0 ? [] : [GisSpatial.Feature(lines.UnionAll(factory: factory).LineMerge(factory: factory), new JsonObject(), srs)];
                break;
            case GisAggregateOperation.Polygonize:
                var boundaries = features.Select(f => f.Geometry).OfType<IPolyline2D>().ToList();
                if (boundaries.Count == 0)
                {
                    output = [];
                    break;
                }
                // the union nodes the lines where they cross, so enclosed areas are found
                var noded = (IPolyline2D)boundaries.UnionAll(factory: factory);
                var polygons = noded.Polygonize(factory);
                context.Logger.LogInformation("{Polygons} polygon(s) enclosed by {Lines} line(s)", polygons.Count, boundaries.Count);
                output = polygons.Select(polygon => GisSpatial.Feature(polygon, new JsonObject(), srs)).ToList();
                break;
            default:
                throw new PipeException($"Unknown operation {parameters.Operation}");
        }

        return Task.FromResult(PipeNodeResult.FromMain(output.Where(f => f.Json["geometry"] is not null)));
    }

    static private List<PipeItem> Dissolve(IReadOnlyList<PipeItem> items, SpatialAggregateParameters parameters, int? srs, CancellationToken cancellationToken)
    {
        var groupBy = parameters.GroupBy.Select(g => g.Trim()).Where(g => g.Length > 0).ToList();
        var aggregates = parameters.Aggregates
            .Where(a => a.Key.Trim().Length > 0)
            .Select(a => (Name: a.Key.Trim(), Aggregate: ParseAggregate(a.Value)))
            .ToList();

        var output = new List<PipeItem>();
        foreach (var group in items.GroupBy(item => string.Join("\u001f", groupBy.Select(g => GisSpatial.Properties(item)[g]?.ToJsonString() ?? "null"))))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var members = group.ToList();
            var properties = new JsonObject();
            foreach (var field in groupBy)
            {
                properties[field] = GisSpatial.Properties(members[0])[field]?.DeepClone();
            }
            foreach (var (name, (function, field)) in aggregates)
            {
                properties[name] = Aggregate(function, field, members);
            }

            var geometries = members.Select(GisSpatial.Geometry).OfType<IGeometry2D>().ToList();
            var geometry = geometries.Count == 0 ? null : geometries.UnionAll(factory: GisSpatial.Factory);
            output.Add(GisSpatial.Feature(geometry, properties, srs));
        }
        return output;
    }

    static private (string Function, string? Field) ParseAggregate(string text)
    {
        var match = AggregatePattern.Match(text ?? "");
        var function = match.Groups["function"].Value.ToLowerInvariant();
        var field = match.Groups["field"].Success && match.Groups["field"].Value.Length > 0 ? match.Groups["field"].Value : null;
        if (!match.Success || function is not ("count" or "sum" or "min" or "max" or "avg" or "first" or "list"))
        {
            throw new PipeException($"Unknown aggregate '{text}'; use count, sum(field), min(field), max(field), avg(field), first(field) or list(field)");
        }
        if (field is null && function != "count")
        {
            throw new PipeException($"The aggregate '{text}' needs a field, e.g. {function}(length)");
        }
        return (function, field);
    }

    static private JsonNode? Aggregate(string function, string? field, List<PipeItem> members)
    {
        var values = field is null ? [] : members.Select(m => GisSpatial.Properties(m)[field]).ToList();
        var numbers = values.Select(Number).OfType<double>().ToList();
        return function switch
        {
            "count" => field is null ? members.Count : values.Count(v => v is not null),
            "sum" => numbers.Sum(),
            "min" => numbers.Count > 0 ? numbers.Min() : null,
            "max" => numbers.Count > 0 ? numbers.Max() : null,
            "avg" => numbers.Count > 0 ? numbers.Average() : null,
            "first" => values.FirstOrDefault(v => v is not null)?.DeepClone(),
            _ => new JsonArray(values.Select(v => v?.DeepClone()).ToArray())
        };
    }

    static private double? Number(JsonNode? value)
        => value is JsonValue v && double.TryParse(v.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ? number : null;

    // the clip polygon of Voronoi: a polygon or a bounding box in the srs of the items
    static private IPolygon2D? Clip(SpatialAggregateParameters parameters, int? srs)
    {
        var node = parameters.ClipGeometry;
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            node = string.IsNullOrWhiteSpace(text) ? null : JsonNode.Parse(text);
        }
        if (node is null)
        {
            return null;
        }

        if (GisSpatialReferences.ToEpsg(parameters.ClipSrs) is { } clipSrs && srs is { } itemsSrs && clipSrs != itemsSrs)
        {
            throw new PipeException($"The clip geometry is in EPSG:{clipSrs}, the items in EPSG:{itemsSrs}; project one of them first");
        }

        if (node is JsonArray bbox)
        {
            if (bbox.Count != 4 || bbox.Any(v => v is not JsonValue number || !number.TryGetValue<double>(out _)))
            {
                throw new PipeException("A bounding box needs four numbers: [minx, miny, maxx, maxy]");
            }
            var b = bbox.Select(v => v!.GetValue<double>()).ToArray();
            node = new JsonObject
            {
                ["type"] = "Polygon",
                ["coordinates"] = new JsonArray(new JsonArray(
                    new JsonArray(b[0], b[1]), new JsonArray(b[2], b[1]), new JsonArray(b[2], b[3]), new JsonArray(b[0], b[3]), new JsonArray(b[0], b[1])))
            };
        }
        if (node is JsonObject feature && feature["type"]?.ToString() == "Feature")
        {
            // a feature item brings its own srs
            if (GisSpatialReferences.ToEpsg(feature["srs"]) is { } featureSrs && srs is { } itemsSrs2 && featureSrs != itemsSrs2)
            {
                throw new PipeException($"The clip geometry is in EPSG:{featureSrs}, the items in EPSG:{itemsSrs2}; project one of them first");
            }
            node = feature["geometry"];
        }

        return GisSpatial.Geometry(node) as IPolygon2D
            ?? throw new PipeException("The clip geometry must be a polygon or a bounding box");
    }

    // the cell keeps the properties of the point (site) it belongs to
    static private List<PipeItem> Voronoi(List<(PipeItem Item, IGeometry2D Geometry)> features, int? srs, IPolygon2D? clip)
    {
        var sites = new Dictionary<Coordinate, PipeItem>();
        foreach (var (item, geometry) in features)
        {
            if (geometry is not IPoint2D point)
            {
                throw new PipeException("Voronoi needs points as input, e.g. the centroids of the features");
            }
            sites.TryAdd(new Coordinate(point.X, point.Y), item);
        }
        if (sites.Count == 0)
        {
            return [];
        }

        return GisSpatial.MultiPoint(sites.Keys).VoronoiDiagram(clip, GisSpatial.Factory)
            .Select(cell =>
            {
                var site = sites.TryGetValue(new Coordinate(cell.Site.X, cell.Site.Y), out var item) ? item : null;
                return GisSpatial.Feature(cell.Cell, site is null ? new JsonObject() : (JsonObject)GisSpatial.Properties(site).DeepClone(), srs);
            })
            .ToList();
    }
}
