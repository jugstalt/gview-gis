using System;

namespace gView.Framework.Core.Geometry
{
    public enum GeometryType
    {
        Point = 0,
        Multipoint = 1,
        Polyline = 2,
        Polygon = 3,
        Aggregate = 4,
        Envelope = 5,
        Unknown = 6,
        Network = 7
    }

    public enum AxisDirection
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    public enum GmlVersion
    {
        v1 = 0,
        v2 = 2,
        v3 = 3
    }

    [Flags]
    public enum CleanGemetryMethods
    {
        IdentNeighbors = 1,
        ZeroParts = 2,
        LineArtifacts = 4
    }
}
