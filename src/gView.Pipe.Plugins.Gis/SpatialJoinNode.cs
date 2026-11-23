using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Data;
using gView.Pipe.Abstraction.Nodes;
using SpatialNET;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis;

public enum GisJoinOperation
{
    [PipeEnumValue("One feature per overlapping pair of main and overlay, with the shared part as geometry and the properties of both")]
    Intersect,

    [PipeEnumValue("The part of each main feature that lies inside the overlay features, e.g. lines cut to a district")]
    Clip,

    [PipeEnumValue("The part of each main feature that lies outside the overlay features")]
    Difference,

    [PipeEnumValue("Adds the properties of matching overlay features to each main feature; the geometry stays")]
    Join,

    [PipeEnumValue("Splits the polygons or lines of the main input along the lines of the overlay, one item per piece with the properties of the main feature, e.g. parcels along a road")]
    Cut,

    [PipeEnumValue("Finds the nearest overlay line of each main point and adds where the point lies along it: position, fraction, offset and distance. The geometry stays, or becomes the point on the line", Label = "Locate along line")]
    LocateAlongLine
}

public enum GisJoinRelation
{
    [PipeEnumValue("Main and overlay touch or overlap")]
    Intersects,

    [PipeEnumValue("The main feature lies completely inside the overlay feature, e.g. a point in a parcel")]
    Within,

    [PipeEnumValue("The main feature completely contains the overlay feature")]
    Contains,

    [PipeEnumValue("The overlay feature closest to the main feature, within Max distance")]
    Nearest
}

public sealed class SpatialJoinParameters
{
    [PipeParameter(Label = "Operation", Order = 1, Expressions = false, Description = "How the main features are combined with the overlay features")]
    public GisJoinOperation Operation { get; set; } = GisJoinOperation.Join;

    [PipeParameter(Label = "Relation", Order = 2, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["join"],
        Description = "Which overlay features match a main feature")]
    public GisJoinRelation Relation { get; set; } = GisJoinRelation.Intersects;

    [PipeParameter(Label = "Max distance", Order = 3, Expressions = false, ShowWhen = nameof(Relation), ShowWhenValues = ["nearest"],
        Description = "Overlay features further away don't match, e.g. 50; 0 has no limit")]
    [Range(0, double.MaxValue)]
    public double MaxDistance { get; set; }

    [PipeParameter(Label = "First match only", Order = 4, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["intersect", "join"],
        Description = "One output item per main feature with the first match; else one per match")]
    public bool FirstMatchOnly { get; set; }

    [PipeParameter(Label = "Prefix", Order = 5, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["intersect", "join", "locateAlongLine"],
        Description = "Prefix of the overlay properties, e.g. join_ makes name join_name; for Locate along line also of position, fraction, offset and distance")]
    public string Prefix { get; set; } = "join_";

    [PipeParameter(Label = "Keep unmatched", Order = 6, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["join", "locateAlongLine"],
        Description = "Main features without a match stay without overlay properties; else they are dropped")]
    public bool KeepUnmatched { get; set; } = true;

    [PipeParameter(Label = "Locate max distance", Order = 8, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["locateAlongLine"],
        Description = "Lines further away from the point don't match, e.g. 20; 0 has no limit")]
    [Range(0, double.MaxValue)]
    public double LocateMaxDistance { get; set; }

    [PipeParameter(Label = "Point on the line", Order = 9, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["locateAlongLine"],
        Description = "The geometry becomes the point on the line nearest to the main point, e.g. to snap it; else the point stays where it is")]
    public bool PointOnLine { get; set; }

    [PipeParameter(Label = "Keep empty results", Order = 7, Expressions = false, Advanced = true, ShowWhen = nameof(Operation), ShowWhenValues = ["clip", "difference"],
        Description = "Keeps main features whose result is empty with geometry null; else they are dropped")]
    public bool KeepEmpty { get; set; }
}

[PipeNode("2b7d5f9e-8a1c-4e3b-9d6f-4c0a2e8b7d15", "Spatial Join",
    Category = "GIS",
    Description = "Combines GeoJSON items of two inputs by location: intersect, clip, difference, or join the properties of "
                  + "intersecting, containing or nearest overlay features. Works on the coordinates of the items with SpatialNET.",
    Inputs = [MainPort, OverlayPort])]
[PipeNodeExamples]
public sealed class SpatialJoinNode : IPipeNode<SpatialJoinParameters>
{
    public const string MainPort = PipePorts.Main;
    public const string OverlayPort = "overlay";

    public Task<PipeNodeResult> ExecuteAsync(IPipeNodeContext context, CancellationToken cancellationToken)
    {
        var main = context.GetInput(MainPort);
        var overlay = context.GetInput(OverlayPort);
        var parameters = context.GetParameters<SpatialJoinParameters>(main.Count > 0 ? main[0] : null);

        var srs = GisSpatial.CommonSrs(main, "The main items");
        var overlaySrs = GisSpatial.CommonSrs(overlay, "The overlay items");
        if (srs is not null && overlaySrs is not null && srs != overlaySrs)
        {
            throw new PipeException($"Main (EPSG:{srs}) and overlay (EPSG:{overlaySrs}) have different coordinate systems; project one of them first");
        }
        if (parameters.Operation == GisJoinOperation.Join && parameters.Relation == GisJoinRelation.Nearest && parameters.MaxDistance > 0)
        {
            GisSpatial.RequireProjected(srs ?? overlaySrs, "Max distance");
        }

        var overlays = overlay.Select(item => (Item: item, Geometry: GisSpatial.Geometry(item)))
            .Where(o => o.Geometry is not null)
            .Select(o => (o.Item, Geometry: o.Geometry!))
            .ToList();
        var index = overlays.ToSpatialIndex(o => o.Geometry.Envelope());
        var factory = GisSpatial.Factory;

        // clip and difference work against all overlay geometries at once
        IGeometry2D? union = parameters.Operation is GisJoinOperation.Clip or GisJoinOperation.Difference && overlays.Count > 0
            ? overlays.Select(o => o.Geometry).UnionAll(factory: factory)
            : null;

        // cut works with the lines of the overlay
        var cutLines = parameters.Operation == GisJoinOperation.Cut ? overlays.Select(o => o.Geometry).OfType<IPolyline2D>().ToList() : [];
        var cutter = cutLines.Count > 0 ? (IPolyline2D)cutLines.UnionAll(factory: factory) : null;
        if (parameters.Operation == GisJoinOperation.LocateAlongLine)
        {
            GisSpatial.RequireProjected(srs ?? overlaySrs, "Locate along line");
        }

        var output = new List<PipeItem>();
        foreach (var item in main)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var geometry = GisSpatial.Geometry(item);

            switch (parameters.Operation)
            {
                case GisJoinOperation.Clip:
                case GisJoinOperation.Difference:
                    IGeometry2D? result = geometry is null ? null
                        : union is null ? (parameters.Operation == GisJoinOperation.Clip ? null : geometry)
                        : parameters.Operation == GisJoinOperation.Clip ? geometry.Intersection(union, factory: factory) : geometry.Difference(union, factory: factory);
                    if (!GisSpatial.IsEmpty(result) || parameters.KeepEmpty)
                    {
                        output.Add(GisSpatial.WithGeometry(item, GisSpatial.IsEmpty(result) ? null : result));
                    }
                    break;

                case GisJoinOperation.Cut:
                    var pieces = geometry switch
                    {
                        null => [],
                        _ when cutter is null => [geometry],
                        IPolygon2D polygon => polygon.Cut(cutter, factory).Cast<IGeometry2D>().ToList(),
                        IPolyline2D line => line.Cut(cutter, factory).Cast<IGeometry2D>().ToList(),
                        _ => [geometry]   // points can't be cut
                    };
                    output.AddRange(pieces.Select(piece => GisSpatial.WithGeometry(item, piece)));
                    break;

                case GisJoinOperation.LocateAlongLine:
                    if (geometry is not null and not IPoint2D)
                    {
                        throw new PipeException("Locate along line needs points as main features, e.g. the centroids of the features");
                    }
                    var located = geometry is IPoint2D point ? Locate(point, overlays, parameters) : null;
                    if (located is null)
                    {
                        if (parameters.KeepUnmatched)
                        {
                            output.Add(item.Clone());
                        }
                        break;
                    }
                    var result2 = parameters.PointOnLine ? GisSpatial.WithGeometry(item, GisSpatial.Point(located.Value.Projection.Point)) : item.Clone();
                    var props = result2.Json["properties"] as JsonObject ?? new JsonObject();
                    result2.Json["properties"] = props;
                    props[$"{parameters.Prefix}position"] = located.Value.Projection.Position;
                    props[$"{parameters.Prefix}fraction"] = located.Value.Length > 0 ? located.Value.Projection.Position / located.Value.Length : 0;
                    props[$"{parameters.Prefix}offset"] = located.Value.Projection.Offset;
                    props[$"{parameters.Prefix}distance"] = located.Value.Projection.Distance;
                    output.Add(Joined(result2, located.Value.Line, parameters.Prefix));
                    break;

                case GisJoinOperation.Intersect:
                    if (geometry is null)
                    {
                        break;
                    }
                    foreach (var match in index.Query(geometry).Where(o => geometry.Intersects(o.Geometry)))
                    {
                        var shared = geometry.Intersection(match.Geometry, factory: factory);
                        if (GisSpatial.IsEmpty(shared))
                        {
                            continue;
                        }
                        output.Add(Joined(GisSpatial.WithGeometry(item, shared), match.Item, parameters.Prefix));
                        if (parameters.FirstMatchOnly)
                        {
                            break;
                        }
                    }
                    break;

                case GisJoinOperation.Join:
                    var matches = geometry is null ? [] : Matches(geometry, overlays, index, parameters).ToList();
                    if (matches.Count == 0)
                    {
                        if (parameters.KeepUnmatched)
                        {
                            output.Add(item.Clone());
                        }
                        break;
                    }
                    foreach (var match in parameters.FirstMatchOnly ? matches.Take(1) : matches)
                    {
                        output.Add(Joined(item.Clone(), match, parameters.Prefix));
                    }
                    break;
            }
        }

        return Task.FromResult(PipeNodeResult.FromMain(output));
    }

    // the nearest line of the overlay and where the point lies along it
    static private (PipeItem Line, LineProjection Projection, double Length)? Locate(IPoint2D point,
        List<(PipeItem Item, IGeometry2D Geometry)> overlays, SpatialJoinParameters parameters)
    {
        var nearest = overlays
            .Where(o => o.Geometry is IPolyline2D)
            .Select(o => (o.Item, Line: (IPolyline2D)o.Geometry, Distance: point.Distance(o.Geometry)))
            .Where(o => parameters.LocateMaxDistance <= 0 || o.Distance <= parameters.LocateMaxDistance)
            .OrderBy(o => o.Distance)
            .FirstOrDefault();
        if (nearest.Item is null || nearest.Line.ProjectDetailed(point) is not { } projection)
        {
            return null;
        }
        return (nearest.Item, projection, nearest.Line.Length());
    }

    static private IEnumerable<PipeItem> Matches(IGeometry2D geometry, List<(PipeItem Item, IGeometry2D Geometry)> overlays,
        SpatialIndex<(PipeItem Item, IGeometry2D Geometry)> index, SpatialJoinParameters parameters)
    {
        if (parameters.Relation == GisJoinRelation.Nearest)
        {
            var nearest = overlays
                .Select(o => (o.Item, Distance: geometry.Distance(o.Geometry)))
                .Where(o => parameters.MaxDistance <= 0 || o.Distance <= parameters.MaxDistance)
                .OrderBy(o => o.Distance)
                .FirstOrDefault();
            return nearest.Item is null ? [] : [nearest.Item];
        }

        return index.Query(geometry)
            .Where(o => parameters.Relation switch
            {
                GisJoinRelation.Within => geometry.Within(o.Geometry),
                GisJoinRelation.Contains => geometry.Contains(o.Geometry),
                _ => geometry.Intersects(o.Geometry)
            })
            .Select(o => o.Item);
    }

    // the properties of the overlay feature, with a prefix, next to those of the main feature
    static private PipeItem Joined(PipeItem result, PipeItem overlay, string prefix)
    {
        var properties = result.Json["properties"] as JsonObject ?? new JsonObject();
        result.Json["properties"] = properties;
        foreach (var (name, value) in GisSpatial.Properties(overlay))
        {
            properties[$"{prefix}{name}"] = value?.DeepClone();
        }
        if (overlay.Json["id"] is { } id)
        {
            properties[$"{prefix}id"] = id.DeepClone();
        }
        return result;
    }
}
