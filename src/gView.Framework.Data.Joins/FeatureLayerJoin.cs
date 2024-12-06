using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.IO;
using gView.Framework.Core.UI;
using gView.Framework.Data.Filters;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.Data.Joins
{
    [RegisterPlugIn("b480ce21-7e05-4153-89ee-331ebc4b3167")]
    public class FeatureLayerJoin : IFeatureLayerJoin, IPropertyPage
    {
        private IMap _map;
        private IFeatureLayer _joinLayer;
        private string _selectFieldNames = String.Empty;
        private int _joinLayerId = -1;
        private Dictionary<string, IRow> _rows = new Dictionary<string, IRow>();

        #region IFeatureLayerJoin Member

        public string Name
        {
            get { return "Feature Layer Join"; }
        }

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

        public IFieldCollection JoinFields
        {
            get
            {
                if (_joinLayer != null)
                {
                    return _joinLayer.Fields;
                }

                return new FieldCollection();
            }
            set
            {

            }
        }

        async public Task<IRow> GetJoinedRow(string val)
        {
            if (_rows.ContainsKey(val))
            {
                return _rows[val];
            }

            if (this.FeatureLayer == null || !(this.FeatureLayer.Class is ITableClass))
            {
                return null;
            }

            QueryFilter filter = new QueryFilter();
            filter.SubFields = _selectFieldNames;
            IField field = ((ITableClass)this.FeatureLayer.Class).FindField(this.JoinField);
            filter.WhereClause = this.JoinField + "=" + QueryValue(field, val);

            IRow ret = null;
            using (ICursor cursor = await ((ITableClass)this.FeatureLayer.Class).Search(filter))
            {
                if (cursor is IFeatureCursor)
                {
                    ret = await ((IFeatureCursor)cursor).NextFeature();
                }
                else if (cursor is IRowCursor)
                {
                    ret = await ((IRowCursor)cursor).NextRow();
                }
            }
            _rows.Add(val, ret);
            return ret;
        }

        async public Task PerformCacheQuery(string[] vals)
        {
            if (this.FeatureLayer == null || !(this.FeatureLayer.Class is ITableClass) || vals == null)
            {
                return;
            }

            QueryFilter filter = new QueryFilter();
            filter.SubFields = _selectFieldNames;
            filter.AddField(this.JoinField);
            IField field = ((ITableClass)this.FeatureLayer.Class).FindField(this.JoinField);
            StringBuilder where = new StringBuilder();
            foreach (string val in vals)
            {
                if (_rows.ContainsKey(val))
                {
                    continue;
                }

                _rows.Add(val, null);
                if (where.Length > 0)
                {
                    where.Append(",");
                }

                where.Append(QueryValue(field, val));
            }
            if (where.Length == 0)
            {
                return;
            }

            filter.WhereClause = $"{this.JoinField} in ({where})";

            using (ICursor cursor = await ((ITableClass)this.FeatureLayer.Class).Search(filter))
            {
                while (true)
                {
                    IRow row = null;
                    if (cursor is IFeatureCursor)
                    {
                        row = await ((IFeatureCursor)cursor).NextFeature();
                    }
                    else if (cursor is IRowCursor)
                    {
                        row = await ((IRowCursor)cursor).NextRow();
                    }

                    if (row == null)
                    {
                        break;
                    }

                    string key = row[this.JoinField].ToString();
                    _rows[key] = row;
                }
            }
        }

        async public Task<ICursor> PerformQuery(IQueryFilter filter)
        {
            if (this.FeatureLayer == null || !(this.FeatureLayer.Class is ITableClass) || filter == null)
            {
                return null;
            }

            return await ((ITableClass)this.FeatureLayer.Class).Search(filter);
        }

        public void Init(string selectFieldNames)
        {
            if (selectFieldNames != null)
            {
                _selectFieldNames = selectFieldNames.Replace(",", " ");
            }

            if (String.IsNullOrEmpty(_selectFieldNames.Trim()))
            {
                _selectFieldNames = "*";
            }
        }

        public joinType JoinType
        {
            get;
            set;
        }

        public void OnCreate(IMap map)
        {
            _map = map;

            foreach (IDatasetElement element in map.MapElements)
            {
                if (element.ID == _joinLayerId && element is IFeatureLayer)
                {
                    _joinLayer = (IFeatureLayer)element;
                }
            }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            this.JoinName = (string)stream.Load("joinname", "Join");
            this.Field = (string)stream.Load("field", String.Empty);
            this.JoinType = (joinType)stream.Load("jointype", (int)joinType.LeftOuterJoin);

            _joinLayerId = (int)stream.Load("joinlayerid", -1);
            this.JoinField = (string)stream.Load("joinfield", String.Empty);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("joinname", this.JoinName);
            stream.Save("field", this.Field);
            stream.Save("jointype", (int)this.JoinType);

            if (_joinLayer != null)
            {
                stream.Save("joinlayerid", _joinLayer.ID);
            }

            stream.Save("joinfield", this.JoinField);
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        public IFeatureLayer FeatureLayer
        {
            get { return _joinLayer; }
            set
            {
                _joinLayer = value;
                if (_joinLayer != null)
                {
                    _joinLayerId = _joinLayer.ID;
                }
            }
        }

        public string JoinField
        {
            get;
            set;
        }

        #region IClone Member

        public object Clone()
        {
            FeatureLayerJoin clone = new FeatureLayerJoin();

            clone.JoinName = this.JoinName;
            clone.Field = this.Field;

            //clone._fields = this._fields != null ? (Fields)this._fields.Clone() : null;
            clone._selectFieldNames = this._selectFieldNames;
            clone.JoinType = this.JoinType;

            clone._joinLayer = _joinLayer;
            clone._joinLayerId = _joinLayerId;
            clone.JoinField = this.JoinField;

            return clone;
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            if (initObject is IMapDocument)
            {
                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"/gView.Win.Data.Joins.UI.dll");

                IJoinPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Data.Joins.UI.FeatureLayerJoinControl") as IJoinPropertyPanel;
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
            {
                return val;
            }

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
    }
}
