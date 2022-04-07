using gView.Framework.IO;
using gView.Framework.system;

namespace gView.Framework.Geometry
{
    public interface ISpatialReference : IPersistable, IClone, IXmlString, IBase64String
    {
        string Name { get; }
        string Description { get; }

        string[] Parameters
        {
            get;
        }

        ISpatialParameters SpatialParameters { get; }

        AxisDirection Gml3AxisX { get; }
        AxisDirection Gml3AxisY { get; }

        int EpsgCode { get; }

        IGeodeticDatum Datum
        {
            get;
            set;
        }

        bool Equals(ISpatialReference sRef);
    }
}
