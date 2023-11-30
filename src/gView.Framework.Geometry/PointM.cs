using System;
using System.Collections.Generic;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry
{
    public class PointM : Point
    {
        private object _m;

        public PointM() : base()
        { }
        public PointM(double x, double y, object m)
            : base(x, y)
        {
            _m = m;
        }

        public PointM(double x, double y, double z, object m)
            : base(x, y, z)
        {
            _m = m;
        }

        public PointM(IPoint p, object m)
            : this(p.X, p.Y, p.Z, m)
        {
        }

        new public object M
        {
            get { return _m; }
            set { _m = value; }
        }

        #region Classes

        public class MComparer<T> : IComparer<PointM>
            where T : IComparable
        {
            #region IComparer<PointM> Member

            public int Compare(PointM x, PointM y)
            {
                return ((T)x.M).CompareTo(((T)y.M));
            }

            #endregion
        }

        #endregion
    }
}
