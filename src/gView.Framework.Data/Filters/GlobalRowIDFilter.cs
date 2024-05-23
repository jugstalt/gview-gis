using System.Collections.Generic;
using System.Text;
using gView.Framework.Core.Data.Filters;

namespace gView.Framework.Data.Filters
{
    public class GlobalRowIDFilter : QueryFilter, IGlobalRowIDFilter
    {
        private List<long> _IDs;
        private string _idField;

        private GlobalRowIDFilter() : base() { }
        public GlobalRowIDFilter(string IDField)
            : base()
        {
            _IDs = new List<long>();
            _idField = IDField;
            this.AddField(_idField);
        }
        public GlobalRowIDFilter(string IDField, List<long> ids)
            : base()
        {
            _IDs = (ids != null) ? ids : new List<long>();
            _idField = IDField;
            this.AddField(_idField);
        }

        #region IRowIDFilter Member

        public List<long> IDs
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
                foreach (long i in _IDs)
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

        #endregion

        public override object Clone()
        {
            GlobalRowIDFilter clone = new GlobalRowIDFilter();
            clone._idField = _idField;
            clone._IDs = _IDs;
            base.CopyTo(clone);
            return clone;
        }
    }
}
