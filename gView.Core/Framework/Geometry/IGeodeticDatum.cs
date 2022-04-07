using gView.Framework.IO;
using gView.Framework.system;

namespace gView.Framework.Geometry
{
    public interface IGeodeticDatum : IPersistable, IClone
    {
        double X_Axis { get; set; }
        double Y_Axis { get; set; }
        double Z_Axis { get; set; }
        double X_Rotation { get; set; }
        double Y_Rotation { get; set; }
        double Z_Rotation { get; set; }
        double Scale_Diff { get; set; }

        string Name { get; set; }
        string Parameter { get; set; }
    }
}
