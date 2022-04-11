namespace gView.Framework.Geometry
{
    public class PointM3 : PointM2
    {
        private object _m3;

        public PointM3(double x, double y, object m, object m2, object m3)
            : base(x, y, m, m2)
        {
            _m3 = m3;
        }
        public PointM3(double x, double y, double z, object m, object m2, object m3)
            : base(x, y, z, m, m2)
        {
            _m3 = m3;
        }
        public PointM3(IPoint p, object m, object m2, object m3)
            : this(p.X, p.Y, p.Z, m, m2, m3)
        {
        }

        public object M3
        {
            get { return _m3; }
            set { _m3 = value; }
        }
    }
}
