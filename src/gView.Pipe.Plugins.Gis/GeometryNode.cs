using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Data;
using gView.Pipe.Abstraction.Nodes;
using SpatialNET;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis;

public enum GisGeometryOperation
{
    [PipeEnumValue("A polygon around the geometry at a distance, e.g. 10 m around a line; needs a projected srs")]
    Buffer,

    [PipeEnumValue("The center of mass as point; it may lie outside of a polygon")]
    Centroid,

    [PipeEnumValue("A point that surely lies inside the polygon, e.g. for labels", Label = "Point on surface")]
    PointOnSurface,

    [PipeEnumValue("The smallest convex polygon around the geometry", Label = "Convex hull")]
    ConvexHull,

    [PipeEnumValue("Fewer vertices within a tolerance, e.g. 0.5 m; needs a projected srs")]
    Simplify,

    [PipeEnumValue("Repairs invalid polygons and lines, e.g. self-intersecting rings", Label = "Make valid")]
    MakeValid,

    [PipeEnumValue("Adds vertices so no segment is longer than a length, e.g. 10 m; needs a projected srs")]
    Densify,

    [PipeEnumValue("Writes the area into a property; the geometry stays. Needs a projected srs")]
    Area,

    [PipeEnumValue("Writes the length (the perimeter of polygons) into a property; the geometry stays. Needs a projected srs")]
    Length,

    [PipeEnumValue("A line parallel to the line (or the ring of a polygon) at a distance, left or right of its direction; needs a projected srs", Label = "Offset curve")]
    OffsetCurve,

    [PipeEnumValue("Rounds the corners of lines and polygons, e.g. for a river; adds vertices")]
    Smooth,

    [PipeEnumValue("The point on a line at a distance from its start or at a fraction of its length, optionally beside the line", Label = "Point at distance")]
    PointAtDistance
}

public enum GisCapStyle
{
    [PipeEnumValue("Round ends of lines")]
    Round,

    [PipeEnumValue("Lines end exactly at their end points")]
    Flat,

    [PipeEnumValue("Square ends, reaching past the end points by the distance")]
    Square
}

public enum GisJoinStyle
{
    [PipeEnumValue("Round corners")]
    Round,

    [PipeEnumValue("Sharp corners, cut off when they get too long")]
    Mitre,

    [PipeEnumValue("Corners cut off straight")]
    Bevel
}

public sealed class GeometryParameters
{
    [PipeParameter(Label = "Operation", Order = 1, Expressions = false, Description = "What to do with the geometry of each item")]
    public GisGeometryOperation Operation { get; set; } = GisGeometryOperation.Buffer;

    [PipeParameter(Label = "Distance", Order = 2, ShowWhen = nameof(Operation), ShowWhenValues = ["buffer", "offsetCurve"],
        Description = "Buffer distance in units of the srs, e.g. 10 or {{ $json.properties.width / 2 }}; negative shrinks polygons. For an offset curve positive is left of the direction of the line, negative right")]
    public JsonNode? Distance { get; set; } = 10;

    [PipeParameter(Label = "Tolerance", Order = 3, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["simplify"],
        Description = "Largest distance a removed vertex may have from the result, e.g. 0.5")]
    [Range(0, double.MaxValue)]
    public double Tolerance { get; set; } = 1;

    [PipeParameter(Label = "Preserve topology", Order = 4, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["simplify"],
        Description = "Keeps rings valid and apart, e.g. for neighboring parcels; slower")]
    public bool PreserveTopology { get; set; } = true;

    [PipeParameter(Label = "Max segment length", Order = 5, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["densify"],
        Description = "Longest segment after densifying, e.g. 10")]
    [Range(0, double.MaxValue)]
    public double MaxSegmentLength { get; set; } = 10;

    [PipeParameter(Label = "Property", Order = 6, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["area", "length"],
        Description = "Name of the property for the result, e.g. area_m2; empty is area or length")]
    public string Property { get; set; } = "";

    [PipeParameter(Label = "Iterations", Order = 11, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["smooth"],
        Description = "How often the corners are cut, e.g. 2; every round doubles the vertices")]
    [Range(1, 10)]
    public int Iterations { get; set; } = 2;

    [PipeParameter(Label = "Position", Order = 12, ShowWhen = nameof(Operation), ShowWhenValues = ["pointAtDistance"],
        Description = "Distance from the start of the line in units of the srs, e.g. 50 or {{ $json.properties.pos }}; with Position is a fraction a value between 0 and 1")]
    public JsonNode? Position { get; set; } = 0;

    [PipeParameter(Label = "Position is a fraction", Order = 13, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["pointAtDistance"],
        Description = "Position is a fraction of the length (0 start, 0.5 middle, 1 end) instead of a distance")]
    public bool PositionIsFraction { get; set; }

    [PipeParameter(Label = "Side offset", Order = 14, ShowWhen = nameof(Operation), ShowWhenValues = ["pointAtDistance"],
        Description = "Distance beside the line, e.g. 2; positive left of the direction of the line, negative right")]
    public JsonNode? SideOffset { get; set; } = 0;

    [PipeParameter(Label = "End cap", Order = 7, Expressions = false, Advanced = true, ShowWhen = nameof(Operation), ShowWhenValues = ["buffer"],
        Description = "Ends of buffered lines")]
    public GisCapStyle CapStyle { get; set; } = GisCapStyle.Round;

    [PipeParameter(Label = "Join style", Order = 8, Expressions = false, Advanced = true, ShowWhen = nameof(Operation), ShowWhenValues = ["buffer", "offsetCurve"],
        Description = "Corners of the buffer or offset curve")]
    public GisJoinStyle JoinStyle { get; set; } = GisJoinStyle.Round;

    [PipeParameter(Label = "Segments per quarter circle", Order = 9, Expressions = false, Advanced = true, ShowWhen = nameof(Operation), ShowWhenValues = ["buffer", "offsetCurve"],
        Description = "Vertices of round parts, e.g. 8; more is smoother")]
    [Range(1, 100)]
    public int QuadrantSegments { get; set; } = 8;

    [PipeParameter(Label = "Keep empty results", Order = 10, Expressions = false, Advanced = true,
        Description = "Keeps items whose result is empty with geometry null, e.g. a negative buffer of a thin polygon; else they are dropped")]
    public bool KeepEmpty { get; set; }
}

[PipeNode("5a8e2c41-7b3d-4f9a-b1e6-0c4d8f2a6e93", "Geometry",
    Category = "GIS",
    Description = "Changes the geometry of each GeoJSON item: buffer, offset curve, smooth, point at a distance, centroid, hull, simplify, make valid, densify; "
                  + "or writes its area or length into a property. Works on the coordinates of the items with SpatialNET.")]
[PipeNodeExamples]
public sealed class GeometryNode : IPipeNode<GeometryParameters>
{
    public Task<PipeNodeResult> ExecuteAsync(IPipeNodeContext context, CancellationToken cancellationToken)
    {
        var items = context.GetInput();
        var output = new List<PipeItem>();

        for (var i = 0; i < items.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = items[i];
            var parameters = context.GetParameters<GeometryParameters>(item, i);
            var geometry = GisSpatial.Geometry(item);

            if (parameters.Operation is GisGeometryOperation.Area or GisGeometryOperation.Length)
            {
                GisSpatial.RequireProjected(GisSpatial.Srs(item), parameters.Operation.ToString());
                var result = item.Clone();
                var properties = result.Json["properties"] as JsonObject ?? new JsonObject();
                result.Json["properties"] = properties;
                var name = string.IsNullOrWhiteSpace(parameters.Property) ? parameters.Operation.ToString().ToLowerInvariant() : parameters.Property.Trim();
                properties[name] = geometry is null ? null
                    : parameters.Operation == GisGeometryOperation.Area ? geometry.Area() : geometry.Length();
                output.Add(result);
                continue;
            }

            var changed = geometry is null ? null : Apply(geometry, parameters, GisSpatial.Srs(item));
            if (changed is null || GisSpatial.IsEmpty(changed))
            {
                if (parameters.KeepEmpty)
                {
                    output.Add(GisSpatial.WithGeometry(item, null));
                }
                continue;
            }
            output.Add(GisSpatial.WithGeometry(item, changed));
        }

        return Task.FromResult(PipeNodeResult.FromMain(output));
    }

    static private IGeometry2D? Apply(IGeometry2D geometry, GeometryParameters parameters, int? srs)
    {
        var factory = GisSpatial.Factory;
        switch (parameters.Operation)
        {
            case GisGeometryOperation.Buffer:
                GisSpatial.RequireProjected(srs, "Buffer");
                var options = new BufferOptions
                {
                    CapStyle = parameters.CapStyle switch { GisCapStyle.Flat => CapStyle.Flat, GisCapStyle.Square => CapStyle.Square, _ => CapStyle.Round },
                    JoinStyle = parameters.JoinStyle switch { GisJoinStyle.Mitre => JoinStyle.Mitre, GisJoinStyle.Bevel => JoinStyle.Bevel, _ => JoinStyle.Round },
                    QuadrantSegments = parameters.QuadrantSegments
                };
                return geometry.Buffer(GisSpatial.Number(parameters.Distance, "Distance"), options, factory);
            case GisGeometryOperation.Centroid:
                return geometry.Centroid() is { } centroid ? GisSpatial.Point(centroid) : null;
            case GisGeometryOperation.PointOnSurface:
                return geometry.PointOnSurface() is { } point ? GisSpatial.Point(point) : null;
            case GisGeometryOperation.ConvexHull:
                return geometry.ConvexHull(factory);
            case GisGeometryOperation.Simplify:
                GisSpatial.RequireProjected(srs, "Simplify");
                return geometry switch
                {
                    IPolygon2D polygon => polygon.Simplify(parameters.Tolerance, parameters.PreserveTopology, factory),
                    IPolyline2D polyline => polyline.Simplify(parameters.Tolerance, parameters.PreserveTopology, factory),
                    _ => geometry   // points have nothing to simplify
                };
            case GisGeometryOperation.MakeValid:
                return geometry.MakeValid(factory: factory);
            case GisGeometryOperation.Densify:
                GisSpatial.RequireProjected(srs, "Densify");
                if (parameters.MaxSegmentLength <= 0)
                {
                    throw new PipeException("Max segment length must be greater than 0");
                }
                return geometry.Densify(parameters.MaxSegmentLength, factory);
            case GisGeometryOperation.OffsetCurve:
                GisSpatial.RequireProjected(srs, "Offset curve");
                var offsetOptions = new OffsetOptions
                {
                    JoinStyle = parameters.JoinStyle switch { GisJoinStyle.Mitre => JoinStyle.Mitre, GisJoinStyle.Bevel => JoinStyle.Bevel, _ => JoinStyle.Round },
                    QuadrantSegments = parameters.QuadrantSegments
                };
                var offset = GisSpatial.Number(parameters.Distance, "Distance");
                return geometry switch
                {
                    IPolyline2D polyline => polyline.OffsetCurve(offset, offsetOptions, factory),
                    IPolygon2D polygon => polygon.OffsetCurve(offset, offsetOptions, factory),
                    _ => throw new PipeException("An offset curve needs lines or polygons")
                };
            case GisGeometryOperation.Smooth:
                return geometry.Smooth(parameters.Iterations, SmoothingMethod.Chaikin, factory);
            case GisGeometryOperation.PointAtDistance:
                if (geometry is not IPolyline2D line)
                {
                    throw new PipeException("A point at a distance needs lines");
                }
                var position = GisSpatial.Number(parameters.Position, "Position");
                var side = GisSpatial.Number(parameters.SideOffset, "Side offset");
                if (parameters.PositionIsFraction)
                {
                    if (position is < 0 or > 1)
                    {
                        throw new PipeException("A fraction of the length lies between 0 and 1");
                    }
                    position *= line.Length();
                }
                else
                {
                    GisSpatial.RequireProjected(srs, "Point at distance");
                }
                if (side != 0)
                {
                    GisSpatial.RequireProjected(srs, "Point at distance with a side offset");
                }
                return line.Interpolate(position, side) is { } at ? GisSpatial.Point(at) : null;
            default:
                throw new PipeException($"Unknown operation {parameters.Operation}");
        }
    }
}
