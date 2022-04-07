using gView.Framework.system;

namespace gView.Framework.Geometry
{
    public interface ISpatialParameters : IClone
    {
        gView.Framework.Carto.GeoUnits Unit { get; }
        bool IsGeographic { get; }

        double lat_0 { get; }
        double lon_0 { get; }
        double x_0 { get; }
        double y_0 { get; }
    }
}
