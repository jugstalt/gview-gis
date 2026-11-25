namespace gView.GeoJsonService.DTOs;

/// <summary>
/// The features of several layers at a place: a point (with a tolerance in pixels) or a line/polygon, as on the
/// map the client shows (map extent, image size and dpi give the scale and the tolerance in map units)
/// </summary>
public class IdentifyRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.Identify;

    public Geometry? Geometry { get; set; }

    /// <summary>The CRS of the geometry and the map extent</summary>
    public CoordinateReferenceSystem? CRS { get; set; }

    /// <summary>visible (default): the visible layers in scale, top: only the topmost of them with a hit, all: every queryable layer</summary>
    public string LayerOption { get; set; } = "visible";

    /// <summary>Layer ids like GetMap: null = the default visibility, ids = exactly these, +id/-id = add/remove</summary>
    public string[]? Layers { get; set; }

    /// <summary>Tolerance around a point in pixels</summary>
    public int Tolerance { get; set; } = 3;

    public BBox? MapExtent { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public double? Dpi { get; set; }

    public GeometryResult ReturnGeometry { get; set; } = GeometryResult.Geometry;
    public CoordinateReferenceSystem? OutCRS { get; set; }

    /// <summary>Max. features per layer (default and maximum: the max. record count of the service)</summary>
    public int? Limit { get; set; }
}
