namespace gView.Framework.Data
{
    public class Feature : Row, IFeature
    {
        private gView.Framework.Geometry.IGeometry _geometry;

        public Feature()
            : base()
        {

        }

        public Feature(IRow row)
            : base()
        {
            if (row == null)
            {
                return;
            }

            _oid = row.OID;
            _fields = row.Fields;
        }

        #region IFeature Member

        public gView.Framework.Geometry.IGeometry Shape
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

        public static void CopyFrom(IFeature original, IFeature feature)
        {
            if (feature == null || original == null)
            {
                return;
            }

            original.Shape = feature.Shape;
            if (feature.Fields != null)
            {
                foreach (IFieldValue fv in feature.Fields)
                {
                    original[fv.Name] = fv.Value;
                }
            }
        }
    }
}
