namespace gView.Framework.Core.Data
{
    public class GlobalFeature : GlobalRow, IGlobalFeature, IFeature
    {
        private Geometry.IGeometry _geometry;

        public GlobalFeature()
            : base()
        {

        }

        public GlobalFeature(IGlobalRow row)
            : base()
        {
            if (row == null)
            {
                return;
            }

            _oid = row.GlobalOID;
            _fields = row.Fields;
        }
        #region IGlobalFeature Member

        public Geometry.IGeometry Shape
        {
            get
            {
                return _geometry;
            }
            set
            {
                _geometry = value;
            }
        }

        #endregion

        #region IOID Member

        public int OID
        {
            get { return (int)(_oid & 0xffffffff); }
        }

        #endregion
    }
}
