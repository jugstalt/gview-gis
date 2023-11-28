using gView.Framework.Geometry;
using System;
using System.Collections.Generic;

namespace gView.Framework.Geometry.Topology;

public class PointMComparerer<T> : IComparer<PointM>
   where T : IComparable
{
    public int Compare(PointM x, PointM y)
    {
        return ((T)x.M).CompareTo((T)y.M);
    }
}

public class PointM2Comparerer<T> : IComparer<PointM2>
   where T : IComparable
{
    public int Compare(PointM2 x, PointM2 y)
    {
        return ((T)x.M2).CompareTo((T)y.M2);
    }
}

public class PointM3Comparerer<T> : IComparer<PointM3>
   where T : IComparable
{
    public int Compare(PointM3 x, PointM3 y)
    {
        return ((T)x.M3).CompareTo((T)y.M3);
    }
}
