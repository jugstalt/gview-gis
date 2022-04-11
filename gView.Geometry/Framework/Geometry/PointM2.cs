namespace gView.Framework.Geometry
{
    public class PointM2 : PointM
    {
        private object _m2;

        public PointM2(double x, double y, object m, object m2)
            : base(x, y, m)
        {
            _m2 = m2;
        }
        public PointM2(double x, double y, double z, object m, object m2)
            : base(x, y, z, m)
        {
            _m2 = m2;
        }
        public PointM2(IPoint p, object m, object m2)
            : this(p.X, p.Y, p.Z, m, m2)
        {
        }

        public object M2
        {
            get { return _m2; }
            set { _m2 = value; }
        }
    }
}
