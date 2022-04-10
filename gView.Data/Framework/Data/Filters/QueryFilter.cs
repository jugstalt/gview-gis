using gView.Data.Framework.Data.Extensions;
using gView.Framework.Geometry;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Framework.Data.Filters
{
    public class QueryFilter : UserData, IQueryFilter, IClone
    {
        protected List<string> m_fields;
        protected Dictionary<string, string> m_alias;
        protected string m_whereClause = String.Empty, m_whereClause2 = String.Empty, m_orderBy = "";
        protected int m_beginrecord;
        protected int m_featureCount;
        protected bool m_hasmore, m_nolock = false;
        protected string m_fieldPrefix = "", m_fieldPostfix = "";
        protected ISpatialReference _featureSRef = null;

        public QueryFilter()
        {
            m_fields = new List<string>();
            m_alias = new Dictionary<string, string>();
            m_beginrecord = 1;
            m_featureCount = 0;
            m_hasmore = m_nolock = false;

            this.Limit = 0;

            this.IgnoreUndefinedFields = false;
        }
        public QueryFilter(IQueryFilter filter)
            : this()
        {
            if (filter == null)
            {
                return;
            }

            string fieldPrefix = filter.fieldPrefix;
            string fieldPostfix = filter.fieldPostfix;
            // Remove Postfix
            filter.fieldPostfix = filter.fieldPrefix = "";

            // Get Subfields (without pre, postfix
            this.SubFields = filter.SubFields;

            // Add Postfix
            this.fieldPostfix = filter.fieldPostfix = fieldPostfix;
            this.fieldPrefix = filter.fieldPrefix = fieldPrefix;

            this.WhereClause = filter.WhereClause;
            this.BeginRecord = filter.BeginRecord;
            this.OrderBy = filter.OrderBy;
            this._featureSRef = (filter.FeatureSpatialReference != null) ? filter.FeatureSpatialReference.Clone() as ISpatialReference : null;
            this.ContextLayerDefaultSpatialReference = filter.ContextLayerDefaultSpatialReference != null ?
                filter.ContextLayerDefaultSpatialReference.Clone() as ISpatialReference : null;

            this.Limit = filter.Limit;

            this.IgnoreUndefinedFields = filter.IgnoreUndefinedFields;
            this.CancelTracker = filter.CancelTracker;

            filter.CopyUserDataTo(this);
        }

        public string fieldPrefix
        {
            get { return m_fieldPrefix; }
            set { m_fieldPrefix = value; }
        }
        public string fieldPostfix
        {
            get { return m_fieldPostfix; }
            set { m_fieldPostfix = value; }
        }

        protected void CopyTo(QueryFilter copyto)
        {
            copyto.m_fields = new List<string>();
            copyto.m_alias = new Dictionary<string, string>();
            foreach (string field in m_fields)
            {
                copyto.m_fields.Add(field);
            }

            foreach (string alias in m_alias.Keys)
            {
                copyto.m_alias.Add(alias, m_alias[alias]);
            }

            copyto.m_whereClause = m_whereClause;
            copyto.m_whereClause2 = m_whereClause2;
            copyto.m_beginrecord = m_beginrecord;
            copyto.m_featureCount = m_featureCount;
            copyto.m_hasmore = m_hasmore;
            copyto.m_nolock = m_nolock;
            copyto.m_fieldPrefix = m_fieldPrefix;
            copyto.m_fieldPostfix = m_fieldPostfix;
            copyto._featureSRef = _featureSRef;
            copyto.CancelTracker = CancelTracker;
            copyto.DatasetCachingContext = this.DatasetCachingContext;

            this.CopyUserDataTo(copyto);
        }

        #region IQueryFilter Member

        public int BeginRecord
        {
            get { return m_beginrecord; }
            set { m_beginrecord = value; }
        }

        public int Limit { get; set; }

        public virtual void AddField(string fieldname, bool caseSensitive = true)
        {
            if (String.IsNullOrEmpty(fieldname))
            {
                return;
            }

            if (HasField("*") && !(fieldname.Contains("(") && fieldname.Contains(")")))
            {
                return;
            }

            if (caseSensitive)
            {
                if (m_fields.IndexOf(fieldname) != -1)
                {
                    return;
                }
            }
            else
            {
                string lowerFieldname = fieldname.ToLower();
                foreach (var f in m_fields)
                {
                    if (f.ToLower() == lowerFieldname)
                    {
                        return;
                    }
                }
            }

            m_fields.Add(fieldname);
        }

        public ICancelTracker CancelTracker { get; set; }

        public virtual void AddField(string fieldname, string alias)
        {
            AddField(fieldname);
            if (fieldname == "*")
            {
                return;
            }

            string a;
            if (m_alias.TryGetValue(fieldname, out a))
            {
                m_alias[fieldname] = alias;
            }
            else
            {
                m_alias.Add(fieldname, alias);
            }
        }
        public string Alias(string fieldname)
        {
            string alias = "";
            m_alias.TryGetValue(fieldname, out alias);
            return alias;
        }

        public bool HasField(string fieldname)
        {
            return m_fields.IndexOf(fieldname) != -1;
        }

        public IEnumerable<string> QuerySubFields => m_fields?.Select(f => f.ToSubFieldName(m_fieldPrefix, m_fieldPostfix)).ToArray() ?? new string[0];

        public string SubFields
        {
            get
            {
                var subfields = new StringBuilder();
                foreach (string field in m_fields)
                {
                    if (subfields.Length > 0)
                    {
                        subfields.Append(" ");
                    }

                    subfields.Append(field.ToSubFieldName(m_fieldPrefix, m_fieldPostfix));
                }
                return subfields.ToString();
            }
            set
            {
                m_fields = new List<string>();

                if (value != null)
                {
                    if (!value.ToLower().Contains(" as "))
                    {
                        value = value.Replace(" ", ",");
                    }

                    foreach (string field in value.Replace(";", ",").Split(','))
                    {
                        string f = field.Trim();
                        if (f == "")
                        {
                            continue;
                        }

                        AddField(f);
                    }
                }
            }
        }
        public virtual string SubFieldsAndAlias
        {
            get
            {
                string subfields = "";
                foreach (string field in m_fields)
                {
                    if (subfields != "")
                    {
                        subfields += ",";
                    }

                    if (field == "*")
                    {
                        subfields += field;
                    }
                    else
                    {
                        if (m_fieldPrefix != null && field.IndexOf(m_fieldPrefix) != 0 && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                        {
                            subfields += m_fieldPrefix;
                        }

                        subfields += field;

                        if (m_fieldPrefix != null && field.IndexOf(m_fieldPostfix, m_fieldPrefix.Length) != field.Length - m_fieldPostfix.Length && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                        {
                            subfields += m_fieldPostfix;
                        }

                        string alias = Alias(field);
                        if (alias != "" && alias != null)
                        {
                            subfields += " as " + alias;
                        }
                    }
                }
                return subfields;
            }
        }

        public string WhereClause
        {
            get
            {
                if (m_whereClause.StartsWith("[") || m_whereClause.StartsWith("{"))
                {
                    if (!String.IsNullOrEmpty(m_whereClause2))
                    {
                        return m_whereClause2;
                    }

                    DisplayFilterCollection dfc = DisplayFilterCollection.FromJSON(m_whereClause);
                    if (dfc != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (DisplayFilter df in dfc)
                        {
                            if (String.IsNullOrEmpty(df.Filter))
                            {
                                continue;
                            }

                            if (sb.Length > 0)
                            {
                                sb.Append(" OR ");
                            }

                            sb.Append(df.Filter);
                        }
                        return m_whereClause2 = sb.ToString();
                    }
                }
                return m_whereClause;
            }
            set
            {
                m_whereClause = (value != null ? value.Trim() : String.Empty);
                m_whereClause2 = String.Empty;
            }
        }
        public string JsonWhereClause
        {
            get
            {
                if ((m_whereClause.Trim().StartsWith("[") && m_whereClause.Trim().EndsWith("]")) ||
                    (m_whereClause.Trim().StartsWith("{") && m_whereClause.Trim().EndsWith("}")))
                {
                    return m_whereClause;
                }

                return String.Empty;
            }
        }
        public string OrderBy
        {
            get
            {
                if (String.IsNullOrEmpty(m_orderBy))
                {
                    return String.Empty;
                }

                if (String.IsNullOrEmpty(m_fieldPostfix) && String.IsNullOrEmpty(m_fieldPrefix))
                {
                    return m_orderBy;
                }

                StringBuilder sb = new StringBuilder();

                foreach (string field in m_orderBy.Replace(",", " ").Split(' '))
                {
                    if (sb.Length > 0 && (field.ToLower() == "asc" || field.ToLower() == "desc"))
                    {
                        sb.Append(" " + field);
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(",");
                        }

                        if (field.IndexOf(m_fieldPrefix) != 0 && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                        {
                            sb.Append(m_fieldPrefix);
                        }

                        sb.Append(field);
                        if (field.IndexOf(m_fieldPostfix, Math.Min(m_fieldPrefix.Length, field.Length)) != field.Length - m_fieldPostfix.Length && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                        {
                            sb.Append(m_fieldPostfix);
                        }
                    }
                }

                return sb.ToString();
            }
            set
            {
                m_orderBy = value;
            }
        }
        public bool NoLock
        {
            get
            {
                return m_nolock;
            }
            set
            {
                m_nolock = value;
            }
        }
        public int LastQueryFeatureCount
        {
            get { return m_featureCount; }
            set { m_featureCount = value; }
        }

        public bool HasMore
        {
            get { return m_hasmore; }
            set { m_hasmore = value; }
        }
        virtual public object Clone()
        {
            QueryFilter filter = new QueryFilter(this);

            return filter;
        }

        public ISpatialReference FeatureSpatialReference
        {
            get
            {
                return _featureSRef;
            }
            set
            {
                _featureSRef = value;
            }
        }

        public ISpatialReference ContextLayerDefaultSpatialReference { get; set; }

        public bool IgnoreUndefinedFields
        {
            get;
            set;
        }

        public double MapScale { get; set; }
        public IDatasetCachingContext DatasetCachingContext { get; set; }

        #endregion

        #region IClone4 Member

        virtual public object Clone(Type type)
        {
            if (type == typeof(ISpatialFilter))
            {
                return new SpatialFilter(this);
            }
            if (type == typeof(IQueryFilter))
            {
                return new QueryFilter(this);
            }
            return null;
        }

        #endregion
    }
}
