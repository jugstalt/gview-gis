using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.system;
using gView.Framework.Db;
using System.Data;
using gView.Framework.UI;
using System.Reflection;

namespace gView.Framework.Data.Joins
{
    [RegisterPlugIn("297E544A-B92C-4B9C-9B58-BCF5AFA2B876")]
    public class FeatureLayerDatabaseJoin : IFeatureLayerJoin, IPropertyPage
    {
        private DataProvider _provider = null;
        private IFields _fields = new Fields();
        private string _selectFieldNames = String.Empty;
        private joinType _joinType = joinType.LeftOuterJoin;
        private Dictionary<string, IRow> _rows = new Dictionary<string, IRow>();

        #region Properties

        public string JoinConnectionString
        {
            get;
            set;
        }

        public string JoinTable
        {
            get;
            set;
        }

        public string JoinField
        {
            get;
            set;
        }

        #endregion

        #region IFeatureLayerJoin Member

        public string Name { get { return "Database Table Join"; } }

        public string JoinName
        {
            get;
            set;
        }

        public string Field
        {
            get;
            set;
        }

        public IFields JoinFields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                if (_fields != null)
                {
                    foreach (IField field in _fields.ToEnumerable())
                    {
                        if (field is Field)
                            ((Field)field).SaveType = true;
                    }
                }
            }
        }

        public IRow GetJoinedRow(string val)
        {
            if (_rows.ContainsKey(val))
                return _rows[val];

            if (_provider == null)
                return null;

            IField field = _fields.FindField(this.JoinField);
            DataTable tab = _provider.ExecuteQuery("select " + _selectFieldNames + " from " + _provider.ToTableName(this.JoinTable) + " where " + _provider.ToFieldName(this.JoinField) + "=" + QueryValue(field, val));
            if (tab == null || tab.Rows.Count == 0)
                return null;

            Row row = new Row();
            foreach (DataColumn col in tab.Columns)
            {
                row.Fields.Add(new FieldValue(col.ColumnName, tab.Rows[0][col.ColumnName]));
            }

            _rows.Add(val, row);
            return row;
        }

        public void PerformCacheQuery(string[] vals)
        {
            IField field = _fields.FindField(this.JoinField);
            StringBuilder where = new StringBuilder();
            foreach (string val in vals)
            {
                if (_rows.ContainsKey(val)) continue;
                _rows.Add(val, null);
                if (where.Length > 0) where.Append(",");
                where.Append(QueryValue(field, val));
            }
            if (where.Length == 0) return;

            string fields = _selectFieldNames;
            if (_selectFieldNames != "*") fields += "," + _provider.ToFieldName(this.JoinField);
            DataTable tab = _provider.ExecuteQuery("select " + fields + " from " + _provider.ToTableName(this.JoinTable) + " where " + _provider.ToFieldName(this.JoinField) + " in (" + where.ToString() + ")");
            if (tab == null || tab.Rows.Count == 0)
                return;

            foreach (DataRow tableRow in tab.Rows)
            {
                if (tableRow[this.JoinField] == null) 
                    continue;
                string key = tableRow[this.JoinField].ToString();
                if (_rows[key] != null)
                    continue;

                Row row = new Row();
                foreach (DataColumn col in tab.Columns)
                {
                    row.Fields.Add(new FieldValue(col.ColumnName, tableRow[col.ColumnName]));
                }
                _rows[key] = row;
            }
        }

        public ICursor PerformQuery(IQueryFilter filter)
        {
            if (_provider == null || filter==null)
                return null;

            filter.fieldPrefix = _provider.FieldPrefix;
            filter.fieldPostfix = _provider.FieldPostfix;

            string sql = "select " + filter.SubFieldsAndAlias + " from " + _provider.ToTableName(this.JoinTable);
            if (!String.IsNullOrEmpty(filter.WhereClause)) sql += " where " + _provider.ToWhereClause(filter.WhereClause);
            if (!String.IsNullOrEmpty(filter.OrderBy)) sql += " order by " + _provider.ToFieldNames(filter.OrderBy);

            return new RowCursor(_provider.ExecuteQuery(sql));
        }

        public void Init(string selectFieldNames)
        {
            _provider = new DataProvider();
            DbConnectionString connString = new DbConnectionString();
            connString.UseProviderInConnectionString = true;
            connString.FromString(this.JoinConnectionString);
            _provider.Open(connString.ConnectionString);

            _selectFieldNames = String.IsNullOrEmpty(selectFieldNames.Trim()) ? "*" : _provider.ToFieldNames(selectFieldNames);
        }

        public joinType JoinType
        {
            get
            {
                return _joinType;
            }
            set
            {
                _joinType = value;
            }
        }

        public void OnCreate(Carto.IMap map)
        {

        }

        #endregion

        #region IPersistable Member

        public void Load(IO.IPersistStream stream)
        {
            this.JoinName = (string)stream.Load("joinname", "Join");
            this.Field = (string)stream.Load("field", String.Empty);

            this.JoinConnectionString = (string)stream.Load("joinconnectionstring", String.Empty);
            this.JoinTable = (string)stream.Load("jointable", String.Empty);
            this.JoinField = (string)stream.Load("joinfield", String.Empty);
            _fields = stream.Load("Fields", null, new Fields()) as Fields;
            _joinType = (joinType)stream.Load("jointype", (int)joinType.LeftOuterJoin);
        }

        public void Save(IO.IPersistStream stream)
        {
            stream.Save("joinname", this.JoinName);
            stream.Save("field", this.Field);

            stream.Save("joinconnectionstring", this.JoinConnectionString);
            stream.Save("jointable", this.JoinTable);
            stream.Save("joinfield", this.JoinField);
            if (_fields != null)
                stream.Save("Fields", _fields);
            stream.Save("jointype", (int)_joinType);
        }

        #endregion

        #region IClone Member

        public object Clone()
        {
            FeatureLayerDatabaseJoin clone = new FeatureLayerDatabaseJoin();

            clone.JoinName = this.JoinName;
            clone.Field = this.Field;

            clone.JoinConnectionString = this.JoinConnectionString;
            clone.JoinTable = this.JoinTable;
            clone.JoinField = this.JoinField;

            clone._fields = this._fields != null ? (Fields)this._fields.Clone() : null;
            clone._selectFieldNames = this._selectFieldNames;
            clone._joinType = this._joinType;

            return clone;
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            if (_provider != null)
            {
                _provider.Dispose();
                _provider = null;
            }
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            if (initObject is IMapDocument)
            {
                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Data.Joins.UI.dll");

                IJoinPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Data.Joins.UI.FeatureLayerDatabaseJoinControl") as IJoinPropertyPanel;
                if (p != null)
                {
                    return p.PropertyPanel(this, (IMapDocument)initObject);
                }

                return p;
            }

            return null;
        }

        public object PropertyPageObject()
        {
            return this;
        }

        #endregion

        #region Helper

        private string QueryValue(IField field, string val)
        {
            if (field == null)
                return val;

            switch (field.type)
            {
                case FieldType.biginteger:
                case FieldType.Double:
                case FieldType.Float:
                case FieldType.ID:
                case FieldType.integer:
                case FieldType.smallinteger:
                case FieldType.boolean:
                    return val;
                default:
                    return "'" + val + "'";
            }
        }

        #endregion

        #region Cursor Class

        private class RowCursor : IRowCursor
        {
            private DataTable _tab;
            private int _pos = 0;

            public RowCursor(DataTable tab)
            {
                _tab = tab;
            }

            #region IRowCursor Member

            public IRow NextRow
            {
                get
                {
                    if (_tab == null || _pos >= _tab.Rows.Count)
                        return null;

                    DataRow tabRow = _tab.Rows[_pos++];

                    Row row = new Row();
                    foreach (DataColumn col in _tab.Columns)
                    {
                        row.Fields.Add(new FieldValue(col.ColumnName, tabRow[col.ColumnName]));
                    }

                    return row;
                }
            }

            #endregion

            #region IDisposable Member

            public void Dispose()
            {
                
            }

            #endregion
        }

        #endregion
    }
}
