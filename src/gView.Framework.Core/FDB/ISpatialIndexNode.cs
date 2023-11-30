using gView.Framework.Core.Geometry;
using System.Collections.Generic;

namespace gView.Framework.Core.FDB
{
    public interface ISpatialIndexNode
    {
        int NID { get; }
        int PID { get; }
        short Page { get; }
        IGeometry Rectangle { get; }
        List<int> IDs { get; }
    }
}
