using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using gView.Framework;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.FDB;
using System.Threading.Tasks;

namespace gView.Framework.Data
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
            if (filter == null) return;

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
                copyto.m_fields.Add(field);
            foreach (string alias in m_alias.Keys)
                copyto.m_alias.Add(alias, m_alias[alias]);

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

        public virtual void AddField(string fieldname, bool caseSensitive=true)
        {
            if (String.IsNullOrEmpty(fieldname)) return;

            if (HasField("*")) return;

            if (caseSensitive)
            {
                if (m_fields.IndexOf(fieldname) != -1) return;
            }
            else
            {
                string lowerFieldname = fieldname.ToLower();
                foreach (var f in m_fields)
                    if (f.ToLower() == lowerFieldname)
                        return;
            }

            m_fields.Add(fieldname);
        }

        public ICancelTracker CancelTracker { get; set; }

        public virtual void AddField(string fieldname, string alias)
        {
            AddField(fieldname);
            if (fieldname == "*") return;

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

        public string SubFields
        {
            get
            {
                string subfields = "";
                foreach (string field in m_fields)
                {
                    if (subfields != "") subfields += " ";
                    if (field == "*")
                    {
                        subfields += field;
                    }
                    else
                    {
                        if (field.IndexOf(m_fieldPrefix) != 0 && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                            subfields += m_fieldPrefix;
                        subfields += field;
                        if (field.IndexOf(m_fieldPostfix, Math.Min(m_fieldPrefix.Length, field.Length)) != field.Length - m_fieldPostfix.Length && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                            subfields += m_fieldPostfix;
                    }
                }
                return subfields;
            }
            set
            {
                m_fields = new List<string>();
                if (value != null)
                {
                    foreach (string field in value.Replace(" ", ",").Replace(";", ",").Split(','))
                    {
                        string f = field.Trim();
                        if (f == "") continue;
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
                    if (subfields != "") subfields += ",";
                    if (field == "*")
                    {
                        subfields += field;
                    }
                    else
                    {
                        if (m_fieldPrefix != null && field.IndexOf(m_fieldPrefix) != 0 && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                            subfields += m_fieldPrefix;
                        subfields += field;
                        if (m_fieldPrefix != null && field.IndexOf(m_fieldPostfix, m_fieldPrefix.Length) != field.Length - m_fieldPostfix.Length && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                            subfields += m_fieldPostfix;

                        string alias = Alias(field);
                        if (alias != "" && alias != null) subfields += " as " + alias;
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
                        return m_whereClause2;

                    DisplayFilterCollection dfc = DisplayFilterCollection.FromJSON(m_whereClause);
                    if (dfc != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (DisplayFilter df in dfc)
                        {
                            if (String.IsNullOrEmpty(df.Filter))
                                continue;

                            if (sb.Length > 0) sb.Append(" OR ");
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
                    return m_whereClause;
                return String.Empty;
            }
        }
        public string OrderBy
        {
            get
            {
                if (String.IsNullOrEmpty(m_orderBy))
                    return String.Empty;

                if (String.IsNullOrEmpty(m_fieldPostfix) && String.IsNullOrEmpty(m_fieldPrefix))
                    return m_orderBy;

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
                            sb.Append(",");

                        if (field.IndexOf(m_fieldPrefix) != 0 && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                            sb.Append(m_fieldPrefix);
                        sb.Append(field);
                        if (field.IndexOf(m_fieldPostfix, Math.Min(m_fieldPrefix.Length, field.Length)) != field.Length - m_fieldPostfix.Length && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                            sb.Append(m_fieldPostfix);
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

    public class DistinctFilter : QueryFilter, IDistinctFilter
    {
        private DistinctFilter()
            :base()
        {
        }
        public DistinctFilter(string field)
            : base()
        {
            m_fields.Add(field);
        }
        public DistinctFilter(string field, string alias)
            : base()
        {
            m_fields.Add(field);
            m_alias.Add(field, alias);
        }

        public override string SubFieldsAndAlias
        {
            get
            {
                string subfields = "";
                foreach (string field in m_fields)
                {
                    if (subfields != "") subfields += ",";
                    if (field == "*")
                    {
                        subfields += field;
                    }
                    else
                    {
                        subfields += "DISTINCT(";
                        if (field.IndexOf(m_fieldPrefix) != 0 && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                            subfields += m_fieldPrefix;
                        subfields += field;
                        if (field.IndexOf(m_fieldPostfix) != field.Length - m_fieldPostfix.Length && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                            subfields += m_fieldPostfix;
                        subfields += ")";

                        string alias = Alias(field);
                        if (alias != "" && alias != null) subfields += " as " + alias;
                    }
                }
                return subfields;
            }
        }
        public override void AddField(string fieldname, bool caseSensitive = true)
        {
            // Nicht möglich
        }
        public override void AddField(string fieldname, string alias)
        {
            // Nicht möglich
        }

        public override object Clone()
        {
            DistinctFilter clone = new DistinctFilter();
            base.CopyTo(clone);

            return clone;
        }
    }

    public class FunctionFilter : QueryFilter, IFunctionFilter
    {
        private string _function = String.Empty;

        private FunctionFilter() : base() { }
        public FunctionFilter(string function, string field)
            : base()
        {
            _function = function;
            m_fields.Add(field);
        }
        public FunctionFilter(string function, string field, string alias)
            : base()
        {
            _function = function;
            m_fields.Add(field);
            m_alias.Add(field, alias);
        }

        public override string SubFieldsAndAlias
        {
            get
            {
                string subfields = "";
                foreach (string field in m_fields)
                {
                    if (subfields != "") subfields += ",";
                    if (field == "*")
                    {
                        subfields += field;
                    }
                    else
                    {
                        subfields += _function + "(";
                        if (field.IndexOf(m_fieldPrefix) != 0 && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                            subfields += m_fieldPrefix;
                        subfields += field;
                        if (field.IndexOf(m_fieldPostfix) != field.Length - m_fieldPostfix.Length && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                            subfields += m_fieldPostfix;
                        subfields += ")";

                        string alias = Alias(field);
                        if (alias != "" && alias != null) subfields += " as " + alias;
                    }
                }
                return subfields;
            }
        }

        public string Function
        {
            get { return _function; }
        }

        public string Alias
        {
            get
            {
                if (m_fields.Count == 1 && m_alias.ContainsKey(m_fields[0]))
                    return m_alias[m_fields[0]];

                return String.Empty;
            }
        }

        public override void AddField(string fieldname, bool caseSensitive = true)
        {
            // Nicht möglich
        }
        public override void AddField(string fieldname, string alias)
        {
            // Nicht möglich
        }

        async public static Task<object> QueryScalar(IFeatureClass fc, FunctionFilter filter, string fieldName)
        {
            if (fc == null || filter == null) return null;

            using (IFeatureCursor cursor = await fc.Search(filter) as IFeatureCursor)
            {
                if (cursor is IFeatureCursorSkills && ((IFeatureCursorSkills)cursor).KnowsFunctions == false)
                {
                    double ret = 0D;
                    #region Init 

                    switch (filter._function.ToLower())
                    {
                        case "min":
                            ret=double.MaxValue;
                            break;
                        case "max":
                            ret=double.MinValue;
                            break;
                        default:
                            ret=0D;
                            break;
                    }
                    
                    #endregion

                    IFeature feature = null;
                    string subField = filter.SubFields.Split(' ')[0];
                    bool hasFeature=false;
                    while ((feature = await cursor.NextFeature()) != null)
                    {
                        hasFeature = true;
                        double val = Convert.ToDouble(feature[subField]);

                        switch (filter._function.ToLower())
                        {
                            case "min":
                                ret = Math.Min(ret, val);
                                break;
                            case "max":
                                ret = Math.Max(ret, val);
                                break;
                            case "sum":
                                ret += val;
                                break;
                        }
                    }

                    if (hasFeature == false)
                        return null;
                    return ret;
                }
                else
                {
                    if (cursor == null) return null;

                    IFeature feature = await cursor.NextFeature();
                    if (feature == null) return null;

                    return feature[fieldName];
                }
            }
        }

        public override object Clone()
        {
            FunctionFilter clone = new FunctionFilter();
            clone._function = _function;
            base.CopyTo(clone);
            return clone;
        }
    }

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
                    if (sb.Length > 0) sb.Append(",");
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
                    if (sb.Length > 0) sb.Append(",");
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

    public class SpatialFilter : QueryFilter, ISpatialFilter, IClone
    {
        private IGeometry m_geom = null;
        //private double m_bufferDist;
        //private bool m_fuzzy=false;
        private ISpatialReference _sRef = null;
        private spatialRelation _spatialRelation = spatialRelation.SpatialRelationIntersects;

        public SpatialFilter()
            : base()
        {
            //m_bufferDist = 0.0;
            //_spatialRelation = spatialRelation.SpatialRelationIntersects;
        }

        public SpatialFilter(IQueryFilter filter) :
            base(filter)
        {
            ISpatialFilter spatialFilter = filter as ISpatialFilter;
            if (spatialFilter == null) return;

            this.Geometry = spatialFilter.Geometry;
            this.FilterSpatialReference = spatialFilter.FilterSpatialReference;
            this.SpatialRelation = spatialFilter.SpatialRelation;
        }

        #region ISpatialFilter Member

        public IGeometry Geometry
        {
            get
            {
                return m_geom;
            }
            set
            {
                m_geom = value;
            }
        }

        //public double BufferDistance 
        //{
        //    get { return m_bufferDist; }
        //    set { m_bufferDist=value; }
        //}

        //public bool FuzzyQuery 
        //{
        //    get { return m_fuzzy; }
        //    set { m_fuzzy=value; }
        //}

        public override object Clone()
        {
            SpatialFilter filter = new SpatialFilter(this);
            return filter;
        }

        public ISpatialReference FilterSpatialReference
        {
            get
            {
                return _sRef;
            }
            set
            {
                _sRef = value;
            }
        }

        //public IGeometry GeometryEx
        //{
        //    get { return null; }
        //}

        public spatialRelation SpatialRelation
        {
            get
            {
                return _spatialRelation;
            }
            set
            {
                _spatialRelation = value;
            }
        }

        #endregion

        public static ISpatialFilter Project(ISpatialFilter filter, ISpatialReference to)
        {
            if (filter == null ||
                to == null || filter.FilterSpatialReference == null ||
                to.Equals(filter.FilterSpatialReference)) return filter;

            SpatialFilter pFilter = new SpatialFilter(filter);
            pFilter.FilterSpatialReference = to;

            pFilter.Geometry = GeometricTransformerFactory.Transform2D(
                filter.Geometry,
                filter.FilterSpatialReference,
                to);

            return pFilter;
        }
    }

    public class BufferQueryFilter : QueryFilter, IBufferQueryFilter
    {
        private IQueryFilter _rootFilter = null;
        private IFeatureClass _rootFC = null;
        private double _bufferDistance = 0.0;
        private gView.Framework.Carto.GeoUnits _units = gView.Framework.Carto.GeoUnits.Meters;

        public BufferQueryFilter() { }
        public BufferQueryFilter(IQueryFilter proto)
            : base(proto)
        {
            if (proto is IBufferQueryFilter)
            {
                IBufferQueryFilter bProto = (IBufferQueryFilter)proto;
                if (bProto.RootFilter != null)
                    _rootFilter = bProto.RootFilter.Clone() as IQueryFilter;
                _rootFC = bProto.RootFeatureClass;
                _bufferDistance = bProto.BufferDistance;
                _units = bProto.BufferUnits;
            }
        }

        #region IBufferQuery Member

        public IQueryFilter RootFilter
        {
            get { return _rootFilter; }
            set { _rootFilter = value; }
        }

        public IFeatureClass RootFeatureClass
        {
            get { return _rootFC; }
            set { _rootFC = value; }
        }

        public double BufferDistance
        {
            get { return _bufferDistance; }
            set { _bufferDistance = value; }
        }

        public gView.Framework.Carto.GeoUnits BufferUnits
        {
            get { return _units; }
            set { _units = value; }
        }

        #endregion

        public override object Clone()
        {
            BufferQueryFilter filter = new BufferQueryFilter(this);
            return filter;
        }
        async public static Task<ISpatialFilter> ConvertToSpatialFilter(IBufferQueryFilter bufferQuery)
        {
            try
            {
                if (bufferQuery == null ||
                    bufferQuery.RootFilter == null ||
                    bufferQuery.RootFeatureClass == null) return null;

                IQueryFilter rootFilter = bufferQuery.RootFilter.Clone() as IQueryFilter;
                if (bufferQuery.RootFeatureClass.Dataset != null && bufferQuery.RootFeatureClass.Dataset.Database is IDatabaseNames)
                {
                    rootFilter.SubFields = ((IDatabaseNames)bufferQuery.RootFeatureClass.Dataset.Database).DbColName(bufferQuery.RootFeatureClass.IDFieldName) + "," +
                                           ((IDatabaseNames)bufferQuery.RootFeatureClass.Dataset.Database).DbColName(bufferQuery.RootFeatureClass.ShapeFieldName);
                }
                else
                {
                    rootFilter.SubFields = bufferQuery.RootFeatureClass.IDFieldName + "," + bufferQuery.RootFeatureClass.ShapeFieldName;
                }
                // Wenn SpatialFilter, dann gleich hier projezieren, damit
                // FEATURECOORDSYS und FILTERCOORDSYS für IMS Requests immer gleich sind,
                // da unten die SpatialReference für den konvertierten Filter die SpatialRef 
                // der RootfeatureClass übernonnem wird...
                //
                // Seit es den FilterSpatialReference und FeatureSpatialReference implementiert 
                // sind, ist das nicht mehr notwendig bzw. erwünscht...
                //
                //if (rootFilter is ISpatialFilter)
                //    rootFilter = SpatialFilter.Project(rootFilter as ISpatialFilter,
                //        bufferQuery.RootFeatureClass.SpatialReference);

                IPolygon buffer = null;
                using (IFeatureCursor cursor = (IFeatureCursor)await bufferQuery.RootFeatureClass.Search(rootFilter))
                {
                    IFeature feature;
                    while ((feature = await cursor.NextFeature()) != null)
                    {
                        if (feature.Shape == null) continue;

                        if (!(feature.Shape is ITopologicalOperation))
                        {
                            throw new Exception("Buffer is not implemented for selected geometry type...");
                        }

                        ITopologicalOperation topoOp = feature.Shape as ITopologicalOperation;
                        IPolygon poly = topoOp.Buffer(bufferQuery.BufferDistance);
                        if (poly == null)
                        {
                            throw new Exception("Buffer fails for geometry...");
                        }
                        if (buffer == null)
                        {
                            buffer = poly;
                        }
                        else
                        {
                            ((ITopologicalOperation)buffer).Union(poly);
                        }
                    }
                }

                if (buffer == null)
                {
                    throw new Exception("No geometry found to buffer! (SELECT " + rootFilter.SubFields + " FROM " + bufferQuery.RootFeatureClass.Name + " WHERE " + rootFilter.WhereClause + ")");
                }

                ISpatialFilter spatialFilter = new SpatialFilter();
                spatialFilter.SubFields = bufferQuery.SubFields;
                spatialFilter.Geometry = buffer;
                spatialFilter.WhereClause = bufferQuery.WhereClause;
                // Die abgefragen Features liegen im FeatureSpatialReference System
                // des RootFilter!!
                spatialFilter.FilterSpatialReference = bufferQuery.RootFilter.FeatureSpatialReference;
                //spatialFilter.FilterSpatialReference = ((bufferQuery.RootFilter is ISpatialFilter) ?
                //    ((ISpatialFilter)bufferQuery.RootFilter).FilterSpatialReference :
                //    bufferQuery.RootFeatureClass.SpatialReference);

                spatialFilter.FeatureSpatialReference = bufferQuery.FeatureSpatialReference;

                // UserData übernehen
                bufferQuery.CopyUserDataTo(spatialFilter);

                return spatialFilter;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class RowTable : ITable
    {
        private DataTable _table;
        private string _idFieldName;
        private IRowCursor _cursor;
        private bool _hasMore = true;

        public event RowsAddedToTableEvent RowsAddedToTable;

        public RowTable(IRowCursor cursor, IFields fields)
        {
            _table = new DataTable(System.Guid.NewGuid().ToString("N"));
            _cursor = cursor;
            CreateColumns(fields);
        }

        public void Dispose()
        {
            _table.Dispose(); _table = null;
        }

        public Task<int> FillAtLeast(List<int> IDs)
        {
            return Task.FromResult<int>(0);
        }

        public Task<int> Fill()
        {
            return Fill(-1);
        }

        async public Task<int> Fill(int next_N_Rows)
        {
            if (_cursor == null) return 0;
            int counter = 0;

            IRow feat = await _cursor.NextRow();
            while (feat != null)
            {
                if (_table.Columns.Count > 0)
                {
                    DataRow row = _table.NewRow();
                    foreach (IFieldValue fv in feat.Fields)
                    {
                        if (_table.Columns[fv.Name] == null) continue;
                        try
                        {
                            row[fv.Name] = fv.Value;
                        }
                        catch { }
                    }
                    _table.Rows.Add(row);
                }
                counter++;
                if (counter >= next_N_Rows && next_N_Rows > 0)
                {
                    _hasMore = true;
                    return counter;
                }
                feat = await _cursor.NextRow();
            }
            _hasMore = false;
            return counter;
        }
        public bool hasMore
        {
            get { return _hasMore; }
        }
        private bool CreateColumns(IFields fields)
        {
            try
            {
                if (fields == null) return false;

                foreach (IField pField in fields.ToEnumerable())
                {
                    if (pField.type == FieldType.Shape) continue;
                    switch (pField.type)
                    {
                        case FieldType.ID:
                            _idFieldName = pField.name;
                            _table.Columns.Add(pField.name, typeof(int));
                            break;
                        case FieldType.biginteger:
                        case FieldType.integer:
                        case FieldType.smallinteger:
                            _table.Columns.Add(pField.name, typeof(int));
                            break;
                        case FieldType.boolean:
                            _table.Columns.Add(pField.name, typeof(bool));
                            break;
                        case FieldType.Double:
                            _table.Columns.Add(pField.name, typeof(double));
                            break;
                        case FieldType.Float:
                            _table.Columns.Add(pField.name, typeof(float));
                            break;
                        default:
                            _table.Columns.Add(pField.name, typeof(string));
                            break;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region IRowTable Member

        public DataTable Table
        {
            get
            {
                return _table;
            }
        }

        public string IDFieldName
        {
            get
            {
                return _idFieldName;
            }
        }

        #endregion
    }

    public class FeatureTable : IFeatureTable
    {
        private DataTable _table;
        private string _idFieldName;
        private Hashtable _geometry;
        private IFeatureCursor _cursor;
        private bool _hasMore = true;
        private ITableClass _tableClass;

        public event RowsAddedToTableEvent RowsAddedToTable;

        public FeatureTable(IFeatureCursor cursor, IFields fields, ITableClass tableClass)
        {
            _table = new DataTable(System.Guid.NewGuid().ToString("N"));
            _cursor = cursor;
            _tableClass = tableClass;
            _geometry = new Hashtable();
            CreateColumns(fields);
        }

        public void Dispose()
        {
            _table.Dispose(); _table = null;
            _geometry.Clear();
        }

        public void ReleaseCursor()
        {
            if (_cursor != null)
            {
                _cursor.Dispose();
                _cursor = null;
            }
        }

        // Lebt nur noch, weil es im FromDataTableOld vorkommt
        // Bitte nicht mehr verwenden...
        public Task<int> FillAtLeast(List<int> IDs)
        {
            return Fill(IDs, null);
        }
        public Task<int> Fill(List<int> IDs)
        {
            return Fill(IDs, null);
        }
        async public Task<int> Fill(List<int> IDs, ICancelTracker cancelTracker)
        {
            int counter = 0;
            if (_tableClass is IFeatureClass)
            {
                ReleaseCursor();

                IFeatureClass fc = (IFeatureClass)_tableClass;

                RowIDFilter filter = new RowIDFilter(fc.IDFieldName);
                filter.IDs = IDs;
                filter.SubFields = "*";
                _cursor = await fc.GetFeatures(filter);
                if (_cursor == null) return 0;

                return await Fill(cancelTracker);
            }
            return counter;
        }

        public Task<int> Fill()
        {
            return Fill(-1, null);
        }
        public Task<int> Fill(ICancelTracker cancelTracker)
        {
            return Fill(-1, cancelTracker);
        }
        public Task<int> Fill(int next_N_Rows)
        {
            return Fill(next_N_Rows, null);
        }
        async public Task<int> Fill(int next_N_Rows, ICancelTracker cancelTracker)
        {
            if (_cursor == null) return 0;
            int counter = 0;

            IFeature feat = await _cursor.NextFeature();
            while (feat != null)
            {
                if (_table.Columns.Count > 0)
                {
                    DataRow row = _table.NewRow();
                    foreach (IFieldValue fv in feat.Fields)
                    {
                        if (_table.Columns[fv.Name] == null) continue;
                        try
                        {
                            row[fv.Name] = fv.Value;
                        }
                        catch { }
                    }
                    _table.Rows.Add(row);
                }

                if (feat.Shape != null)
                {
                    _geometry.Remove(feat.OID);
                    _geometry.Add(feat.OID, feat.Shape);
                }

                counter++;

                if (RowsAddedToTable != null)
                {
                    if ((counter % 50) == 0)
                    {
                        RowsAddedToTable(50);
                    }
                }

                if ((counter >= next_N_Rows && next_N_Rows > 0) || (cancelTracker != null && !cancelTracker.Continue))
                {
                    _hasMore = true;
                    return counter;
                }
                feat = (_cursor != null) ? await _cursor.NextFeature() : null;
            }
            _hasMore = false;
            return counter;
        }
        public bool hasMore
        {
            get { return _hasMore; }
        }
        private bool CreateColumns(IFields fields)
        {
            try
            {
                if (fields == null) return false;

                foreach (IField pField in fields.ToEnumerable())
                {
                    if (pField.type == FieldType.Shape) continue;
                    switch (pField.type)
                    {
                        case FieldType.ID:
                            _idFieldName = pField.name;
                            DataColumn col = new DataColumn(pField.name, typeof(int));
                            _table.Columns.Add(col);
                            _table.PrimaryKey = new DataColumn[] { col };
                            break;
                        case FieldType.biginteger:
                        case FieldType.integer:
                        case FieldType.smallinteger:
                            _table.Columns.Add(pField.name, typeof(int));
                            break;
                        case FieldType.boolean:
                            _table.Columns.Add(pField.name, typeof(bool));
                            break;
                        case FieldType.Double:
                            _table.Columns.Add(pField.name, typeof(double));
                            break;
                        case FieldType.Float:
                            _table.Columns.Add(pField.name, typeof(float));
                            break;
                        default:
                            _table.Columns.Add(pField.name, typeof(string));
                            break;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #region IFeatureTable Member

        public DataTable Table
        {
            get
            {
                return _table;
            }
        }

        public IGeometry Shape(object ObjectID)
        {
            return (IGeometry)_geometry[ObjectID];
        }

        public string IDFieldName
        {
            get
            {
                return _idFieldName;
            }
        }

        #endregion
    }

    abstract public class IDSetTemplate<T>
    {
        abstract public List<T> IDs
        {
            get;
        }
    }
    public class IDSelectionSetTemplate<T> : IDSetTemplate<T>
    {
        protected List<T> _IDs;

        public IDSelectionSetTemplate()
            : base()
        {
            _IDs = new List<T>();
        }
        public void Dispose()
        {
            Clear();
        }

        #region IIDSelectionSet Member

        public void Clear()
        {
            _IDs.Clear();
        }

        public void AddID(T ID)
        {
            if (_IDs.IndexOf(ID) != -1) return;
            _IDs.Add(ID);
        }

        public void AddIDs(List<T> IDs)
        {
            if (IDs == null) return;
            foreach (T id in IDs)
            {
                this.AddID(id);
            }
        }

        public void RemoveID(T ID)
        {
            if (_IDs.IndexOf(ID) == -1) return;
            _IDs.Remove(ID);
        }

        public void RemoveIDs(List<T> IDs)
        {
            if (IDs == null) return;
            foreach (T id in IDs)
            {
                this.RemoveID(id);
            }
        }

        override public List<T> IDs
        {
            get
            {
                return ListOperations<T>.Clone(_IDs); // _IDs.Clone();
            }
        }

        public void Combine(ISelectionSet selSet, CombinationMethod method)
        {
            if (selSet == null)
                return;

            if (!(selSet is IIDSelectionSet))
            {
                throw new Exception("Can't combine selectionset that are not implement the IIDSectionSet type..");
            }
            if (!(selSet is IDSetTemplate<T>))
            {
                throw new Exception("Can't combine selectionset with different templates...");
            }

            IIDSelectionSet idSelSet = selSet as IIDSelectionSet;

            switch (method)
            {
                case CombinationMethod.Union:
                    foreach (T id in ((IDSetTemplate<T>)idSelSet).IDs)
                    {
                        if (_IDs.IndexOf(id) == -1) _IDs.Add(id);
                    }
                    break;
                case CombinationMethod.Difference:  // Remove from Current Selection
                    foreach (T id in ((IDSetTemplate<T>)idSelSet).IDs)
                    {
                        if (_IDs.IndexOf(id) != -1) _IDs.Remove(id);
                    }
                    break;
                case CombinationMethod.Intersection:  // Select from Current Selection (nur die gleichen)
                    List<T> ids = ((IDSetTemplate<T>)idSelSet).IDs;
                    foreach (T id in ListOperations<T>.Clone(_IDs))
                    {
                        if (ids.IndexOf(id) == -1) _IDs.Remove(id);
                    }
                    break;
                case CombinationMethod.SymDifference:
                    foreach (T id in ((IDSetTemplate<T>)idSelSet).IDs)
                    {
                        if (_IDs.IndexOf(id) != -1)
                            _IDs.Remove(id);
                        else
                            _IDs.Add(id);
                    }
                    break;
            }
        }
        #endregion

        #region ISelectionSet Member


        public int Count
        {
            get
            {
                if (_IDs == null) return 0;
                return _IDs.Count;
            }
        }

        #endregion
    }

    public class SpatialIndexedIDSelectionSetTemplate<T> : IDSetTemplate<T>
    {
        private Dictionary<long, IndexList<T>> _NIDs;
        private BinarySearchTree2 _tree;

        public SpatialIndexedIDSelectionSetTemplate(IEnvelope bounds)
        {
            _tree = new BinarySearchTree2(bounds, 50, 200, 0.55, null);
            _NIDs = new Dictionary<long, IndexList<T>>();
            _NIDs.Add((long)0, new IndexList<T>());
        }

        #region ISpatialIndexedSelectionSet Members

        public void Clear()
        {
            foreach (List<T> list in _NIDs.Values)
            {
                list.Clear();
            }
            _NIDs.Clear();
        }
        public void AddID(T ID, IGeometry geometry)
        {
            IndexList<T> ids;

            long NID = (geometry != null) ? _tree.InsertSINode(geometry.Envelope) : 0;
            _tree.AddNodeNumber(NID);

            if (!_NIDs.TryGetValue(NID, out ids))
            {
                ids = new IndexList<T>();
                _NIDs.Add(NID, ids);
            }
            if (ids.IndexOf(ID) == -1) ids.Add(ID);
        }

        public long NID(T id)
        {
            foreach (long nid in _NIDs.Keys)
            {
                List<T> ids = (List<T>)_NIDs[nid];
                if (ids == null) continue;

                if (ids.IndexOf(id) != -1) return nid;
            }
            return 0;
        }

        public List<T> IDsInEnvelope(IEnvelope envelope)
        {
            IndexList<T> ret = new IndexList<T>();
            if (_tree != null && envelope != null)
            {
                foreach (long NID in _tree.CollectNIDs(envelope))
                {
                    IndexList<T> list;

                    if (_NIDs.TryGetValue(NID, out list))
                    {
                        ret.AddRange(list);
                    }
                }
            }
            else
            {
                foreach (long NID in _NIDs.Keys)
                {
                    ret.AddRange((List<T>)_NIDs[NID]);
                }
            }
            return ret;
        }

        #endregion

        public IEnvelope Bounds
        {
            get { return _tree.Bounds; }
        }
        private IEnvelope NodeEnvelope(T id)
        {
            foreach (long NID in _NIDs.Keys)
            {
                if (((List<T>)_NIDs[NID]).IndexOf(id) >= 0)
                {
                    return _tree[NID];
                }
            }
            return null;
        }
        #region ISelectionSet Members

        public void AddID(T ID)
        {
            this.AddID(ID, null);
        }

        public void AddIDs(List<T> IDs)
        {
            foreach (T ID in IDs)
            {
                this.AddID(ID, null);
            }
        }

        public void RemoveID(T ID)
        {
            foreach (List<T> list in _NIDs.Values)
            {
                if (list.IndexOf(ID) != -1) list.Remove(ID);
            }
        }

        public void RemoveIDs(List<T> IDs)
        {
            foreach (T ID in IDs)
            {
                RemoveID(ID);
            }
        }

        override public List<T> IDs
        {
            get { return this.IDsInEnvelope(null); }
        }

        public void Combine(ISelectionSet selSet, CombinationMethod method)
        {
            if (!(selSet is IIDSelectionSet))
            {
                throw new Exception("Can't combine selectionset that are not implement the IIDSectionSet type..");
            }
            if (!(selSet is IDSetTemplate<T>))
            {
                throw new Exception("Can't combine selectionset with different templates...");
            }

            IIDSelectionSet idSelSet = selSet as IIDSelectionSet;
            SpatialIndexedIDSelectionSetTemplate<T> spSelSet = null;
            if (selSet is SpatialIndexedIDSelectionSetTemplate<T>) spSelSet = (SpatialIndexedIDSelectionSetTemplate<T>)selSet;

            List<T> _IDs = this.IDs;
            switch (method)
            {
                case CombinationMethod.Union:
                    foreach (T id in ((IDSetTemplate<T>)idSelSet).IDs)
                    {
                        if (_IDs.IndexOf(id) == -1)
                        {
                            if (spSelSet != null)
                                this.AddID(id, spSelSet.NodeEnvelope(id) /*spSelSet.NID(id)*/);
                            else
                                this.AddID(id);  // to NodeID = 0
                        }
                    }
                    break;
                case CombinationMethod.Difference:  // Remove from Current Selection
                    foreach (T id in ((IDSetTemplate<T>)idSelSet).IDs)
                    {
                        if (_IDs.IndexOf(id) != -1) this.RemoveID(id);
                    }
                    break;
                case CombinationMethod.Intersection:  // Select from Current Selection (nur die gleichen)
                    List<T> ids = ((IDSetTemplate<T>)idSelSet).IDs;
                    foreach (T id in _IDs)
                    {
                        if (ids.IndexOf(id) == -1) this.RemoveID(id);
                    }
                    break;
                case CombinationMethod.SymDifference:
                    foreach (T id in ((IDSetTemplate<T>)idSelSet).IDs)
                    {
                        if (_IDs.IndexOf(id) != -1)
                        {
                            this.RemoveID(id);
                        }
                        else
                        {
                            if (spSelSet != null)
                                this.AddID(id, spSelSet.NodeEnvelope(id));
                            else
                                this.AddID(id);
                        }
                    }
                    break;
            }
        }


        #endregion

        #region ISelectionSet Member


        public int Count
        {
            get
            {
                if (_NIDs == null) return 0;

                int count = 0;
                foreach (long nid in _NIDs.Keys)
                {
                    if (_NIDs[nid] == null) continue;
                    count += _NIDs[nid].Count;
                }
                return count;
            }
        }

        #endregion
    }

    public class IDSelectionSet : IDSelectionSetTemplate<int>, IIDSelectionSet
    {
        public IDSelectionSet()
            : base()
        {
        }
    }

    public class SpatialIndexedIDSelectionSet : SpatialIndexedIDSelectionSetTemplate<int>, ISpatialIndexedIDSelectionSet
    {
        public SpatialIndexedIDSelectionSet(IEnvelope bounds)
            : base(bounds)
        {
        }
    }

    public class GlobalIDSelectionSet : IDSelectionSetTemplate<long>, IGlobalIDSelectionSet
    {
        public GlobalIDSelectionSet()
            : base()
        {
        }
    }

    public class SpatialIndexedGlobalIDSelectionSet : SpatialIndexedIDSelectionSetTemplate<long>, ISpatialIndexedGlobalIDSelectionSet
    {
        public SpatialIndexedGlobalIDSelectionSet(IEnvelope bounds)
            : base(bounds)
        {
        }
    }

    public class QueryFilteredSelectionSet : IQueryFilteredSelectionSet
    {
        private IQueryFilter _filter = null;
        private int _count = 0;

        private QueryFilteredSelectionSet() { }

        async static public Task<QueryFilteredSelectionSet> CreateAsync(IFeatureClass fClass, IQueryFilter filter)
        {
            var set = new QueryFilteredSelectionSet();

            if (fClass == null)
                return set;

            set._filter = filter.Clone() as IQueryFilter;
            if (set._filter == null)
                return set;

            try
            {
                using (IFeatureCursor cursor = await fClass.GetFeatures(filter))
                {
                    if (cursor == null)
                        return set;

                    IFeature feature;
                    while ((feature = await cursor.NextFeature()) != null)
                        set._count++;
                }
            }
            catch
            {
                set.Clear();
            }

            return set;
        }

        public QueryFilteredSelectionSet(IQueryFilter filter, int count)
        {
            _filter = filter;
            _count = count;
        }
        #region IQueryFilteredSelectionSet Member

        public IQueryFilter QueryFilter
        {
            get { return _filter; }
        }

        #endregion

        #region ISelectionSet Member

        public void Clear()
        {
            _filter = null;
            _count = 0;
        }

        public int Count
        {
            get { return _count; }
        }

        public void Combine(ISelectionSet selSet, CombinationMethod method)
        {
            throw new Exception("A combination for featureselection is not allowed for this featuretype (no ID Field is defined...)");
        }

        #endregion
    }

    public class TextCursor : ITextCursor
    {
        private string _text;
        public TextCursor(string text)
        {
            _text = text;
        }
        #region ITextCursor Member

        public string Text
        {
            get { return _text; }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }

    public class UrlCursor : IUrlCursor
    {
        private string _url;

        public UrlCursor(string url)
        {
            _url = url;
        }

        #region IUrlCursor Member

        public string Url
        {
            get { return _url; }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }

    public class XmlCursor : IXmlCursor
    {
        private string _xml;

        public XmlCursor(string xml)
        {
            _xml = xml;
        }

        #region IXmlCursor Member

        public string Xml
        {
            get { return _xml; }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }

    public class DisplayFilter
    {
        public string Filter = String.Empty;
        public System.Drawing.Color Color = System.Drawing.Color.Transparent;
        public float PenWidth = -1f;
    }

    public class DisplayFilterCollection : List<DisplayFilter>
    {
        public static DisplayFilterCollection FromJSON(string json)
        {
            try
            {
                if (json.StartsWith("["))
                {
                    json = "{ \"filters\":" + json + "}";
                }

                json = json.Replace("\r", "").Replace("\n", "").Replace("\t", "");
                Hashtable f = (Hashtable)JSON.JsonDecode(json);

                DisplayFilterCollection filterCollection = new DisplayFilterCollection();

                ArrayList filters = (ArrayList)f["filters"];
                for (int i = 0; i < filters.Count; i++)
                {
                    Hashtable filter = (Hashtable)filters[i];
                    DisplayFilter displayFilter = new DisplayFilter();
                    if (filter["sql"] != null)
                        displayFilter.Filter = (string)filter["sql"];
                    if (filter["color"] != null)
                        displayFilter.Color = ColorConverter2.ConvertFrom((string)filter["color"]);
                    if (filter["penwidth"] is double)
                        displayFilter.PenWidth = (float)((double)filter["penwidth"]);

                    filterCollection.Add(displayFilter);
                }
                return filterCollection;
            }
            catch
            {
            }
            return null;
        }
    }
}
