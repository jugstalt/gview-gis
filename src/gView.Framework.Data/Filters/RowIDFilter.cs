using System.Collections.Generic;
using System.Text;
using gView.Framework.Core.Data.Filters;

namespace gView.Framework.Data.Filters
{
    public class RowIDFilter : QueryFilter, IRowIDFilter
    {
        private List<int> _IDs;
        private string _idField;

        private RowIDFilter() : base() { }
        public RowIDFilter(string IDField)
            : base()
        {
            _IDs = new List<int>();
            _idField = IDField;
            this.AddField(_idField);
        }
        public RowIDFilter(string IDField, List<int> ids)
            : base()
        {
            _IDs = (ids != null) ? ids : new List<int>();
            _idField = IDField;
            this.AddField(_idField);
        }

        #region IRowIDFilter Member

        public List<int> IDs
        {
            get
            {
                return _IDs;
            }
            set
            {
                _IDs = value;
            }
        }
        public string RowIDWhereClause
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (int i in _IDs)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append(i.ToString());
                }

                string append = (this.WhereClause != "") ? " AND " + this.WhereClause : "";
                return this.fieldPrefix + _idField + this.fieldPostfix + " in (" + sb.ToString() + ")" + append;
            }
        }
        public string IdFieldName
        {
            get { return _idField; }
        }
        #endregion

        public override object Clone()
        {
            RowIDFilter clone = new RowIDFilter();
            clone._idField = _idField;
            clone._IDs = _IDs;
            base.CopyTo(clone);
            return clone;
        }
    }
}
