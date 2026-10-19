using gView.Framework.Core.Geometry;

namespace gView.Interoperability.OgcApi.Features;

/// <summary>
/// A queried feature, independent of the output encoding (GeoJSON, HTML).
/// The geometry is already in the requested output CRS (gView x/y order).
/// </summary>
public sealed class OgcFeature
{
    public object? Id { get; set; }

    public IGeometry? Geometry { get; set; }

    public List<KeyValuePair<string, object?>> Properties { get; } = new();
}
