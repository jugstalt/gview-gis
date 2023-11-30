namespace gView.Framework.Core.Data
{
    public class GlobalRow : RowData, IGlobalRow
    {
        protected long _oid;

        public GlobalRow()
            : base()
        {
        }

        #region IOID Member

        public long GlobalOID
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
