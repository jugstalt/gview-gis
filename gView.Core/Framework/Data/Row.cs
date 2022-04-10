namespace gView.Framework.Data
{
    public class Row : RowData, IRow
    {
        protected int _oid;

        public Row()
            : base()
        {
        }

        #region IOID Member

        public int OID
        {
            get
            {
                return _oid;
            }
            set
            {
                _oid = value;
            }
        }

        #endregion
    }
}
