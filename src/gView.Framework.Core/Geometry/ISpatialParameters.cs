using gView.Framework.Core.Carto;
using gView.Framework.Core.system;

namespace gView.Framework.Core.Geometry
{
    public interface ISpatialParameters : IClone
    {
        GeoUnits Unit { get; }
        bool IsGeographic { get; }

        double lat_0 { get; }
        double lon_0 { get; }
        double x_0 { get; }
        double y_0 { get; }
    }
}
