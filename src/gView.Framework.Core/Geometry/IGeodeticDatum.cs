using gView.Framework.Core.IO;
using gView.Framework.Core.Common;

namespace gView.Framework.Core.Geometry
{
    public interface IGeodeticDatum : IPersistable, IClone
    {
        double X_Axis { get; }
        double Y_Axis { get; }
        double Z_Axis { get; }
        double X_Rotation { get; }
        double Y_Rotation { get; }
        double Z_Rotation { get; }
        double Scale_Diff { get; }
        string GridShiftFile { get; }

        string Name { get; set; }
        string Parameter { get; set; }

        bool IsEqual(IGeodeticDatum geodeticDatum, bool equalName, bool equalParameter);
    }
}
