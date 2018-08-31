using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.system;
using gView.Framework.Db;
using System.Data;
using gView.Framework.Carto;

namespace gView.Framework.Data
{
    internal class WrappedFeatureClassWithJoins : IFeatureClass
    {
        private IFeatureClass _fc;
        private FeatureLayerJoins _joins;
        private Fields _fields;

        public WrappedFeatureClassWithJoins(IFeatureClass fc, FeatureLayerJoins joins)
        {
            _fc = fc;
            this.Joins = joins;
        }

        public void Refresh()
        {
            _fields = new Fields();

            foreach (IField field in _fc.Fields.ToEnumerable())
                _fields.Add(field);

            if (_joins == null)
                return;

            foreach (IFeatureLayerJoin join in _joins)
            {
                if (join.JoinFields == null)
                    continue;

                foreach (IField field in join.JoinFields.ToEnumerable())
                {
                    Field f = new Field(field);
                    f.name = join.JoinName + ":" + field.name;
                    f.aliasname = join.JoinName + ":" + (String.IsNullOrEmpty(f.aliasname) ? f.name : f.aliasname);

                    if (f.type == FieldType.ID)
                        f.type = FieldType.integer;
                    if (f.type == FieldType.Shape)
                        f.type = FieldType.binary;

                    _fields.Add(f);
                }
            }
        }

        internal FeatureLayerJoins Joins
        {
            get { return _joins; }
            set
            {
                _joins = value;
                this.Refresh();
            }
        }

        public IFeatureClass WrappedFeatureclass
        {
            get { return _fc; }
        }

        #region IFeatureClass Member

        public string ShapeFieldName
        {
            get { return _fc.ShapeFieldName; }
        }

        public Geometry.IEnvelope Envelope
        {
            get { return _fc.Envelope; }
        }

        public int CountFeatures
        {
            get { return _fc.CountFeatures; }
        }

        public IFeatureCursor GetFeatures(IQueryFilter filter)
        {
            if (filter == null)
                return null;

            if (filter is IBufferQueryFilter)
            {
                ISpatialFilter sFilter = BufferQueryFilter.ConvertToSpatialFilter(filter as IBufferQueryFilter);
                if (sFilter == null) return null;
                return GetFeatures(sFilter);
            }

            #region IDistrictFilter ?
            if (filter is IDistinctFilter)
            {
                if (filter.SubFields.Contains(":"))
                {
                    string[] fn = filter.SubFields.Split(':');
                    
                    IFeatureLayerJoin join = _joins[fn[0]];
                    if (join != null)
                    {
                        using (join = (IFeatureLayerJoin)join.Clone())
                        {
                            join.Init(String.Empty);
                            filter = new DistinctFilter(fn[1]);
                            filter.OrderBy = fn[1];
                            return new FeatureCursorWrapper(join.PerformQuery(filter));
                        }
                    }
                    return null;
                }
                else
                {
                    return _fc.GetFeatures(filter);
                }
            }
            #endregion

            #region IFunctionFilter ?
            if (filter is IFunctionFilter)
            {
                if (filter.SubFields.Contains(":"))
                {
                    string[] fn = filter.SubFields.Split(':');

                    IFeatureLayerJoin join = _joins[fn[0]];
                    if (join != null)
                    {
                        using (join = (IFeatureLayerJoin)join.Clone())
                        {
                            join.Init(String.Empty);
                            filter = new FunctionFilter(((IFunctionFilter)filter).Function, fn[1], ((IFunctionFilter)filter).Alias);
                            return new FeatureCursorWrapper(join.PerformQuery(filter));
                        }
                    }
                    return null;
                }
                else
                {
                    return _fc.GetFeatures(filter);
                }
            }
            #endregion

            bool hasInnerJoin = false;
            if (_joins != null)
            {
                foreach (IFeatureLayerJoin join in _joins)
                {
                    if (join.JoinType == joinType.LeftInnerJoin)
                    {
                        hasInnerJoin = true;
                        break;
                    }
                }
            }
            if ((!filter.SubFields.Contains(":") && !filter.SubFields.Contains("*") && hasInnerJoin == false && !filter.WhereClause.Contains(":")) || _joins == null || _joins.Count == 0)
                return _fc.GetFeatures(filter);

            Dictionary<string, UniqueList<string>> fieldNames = new Dictionary<string, UniqueList<string>>();
            fieldNames.Add(String.Empty, new UniqueList<string>());
            fieldNames[String.Empty].Add(this.IDFieldName);

            string[] names = filter.SubFields.Replace(" ", ",").Split(',');

            foreach (string fieldname in filter.SubFields.Replace(" ", ",").Split(','))
            {
                if (fieldname == "*")
                {
                    fieldNames[String.Empty] = new UniqueList<string>();
                    fieldNames[String.Empty].Add("*");
                    foreach (IFeatureLayerJoin join in _joins)
                    {
                        fieldNames[join.JoinName] = new UniqueList<string>();
                        fieldNames[join.JoinName].Add("*");
                    }
                    break;
                }
                if (fieldname.Contains(":"))
                {
                    string[] fn = fieldname.Split(':');

                    IFeatureLayerJoin join = _joins[fn[0]];
                    if (join != null)
                    {
                        fieldNames[String.Empty].Add(join.Field);

                        if (!fieldNames.ContainsKey(fn[0]))
                            fieldNames.Add(fn[0], new UniqueList<string>());
                        fieldNames[fn[0]].Add(fn[1].Trim());
                    }
                }
                else
                {
                    fieldNames[String.Empty].Add(fieldname.Trim());
                }
            }

            foreach (IFeatureLayerJoin join in _joins)
            {
                if (join.JoinType == joinType.LeftInnerJoin)
                {
                    if (!fieldNames.Keys.Contains(join.JoinName))
                    {
                        fieldNames.Add(join.JoinName, new UniqueList<string>() { join.JoinFields[0].name });
                        fieldNames[String.Empty].Add(join.Field);
                    }
                }
            }

            filter = (IQueryFilter)filter.Clone();
            filter.SubFields = fieldNames[String.Empty].ToString(',');

            #region CrossTable Where Clause ?
            if (!String.IsNullOrEmpty(filter.WhereClause) && filter.WhereClause.Contains(":"))
            {
                string where = filter.WhereClause.ToLower();
                bool isCrossTableQuery = false;
                foreach (IField field in this.Fields.ToEnumerable())
                {
                    if (field.name.Contains(":") && where.Contains("[" + field.name.ToLower() + "]"))
                    {
                        IFeatureLayerJoin join = _joins[field.name.Split(':')[0]];
                        if (join != null)
                        {
                            isCrossTableQuery = true;
                            if (!fieldNames.ContainsKey(join.JoinName))
                                fieldNames.Add(join.JoinName, new UniqueList<string>());
                            fieldNames[join.JoinName].Add(field.name.Split(':')[1].Trim());
                            filter.AddField(join.Field);
                            //filter.AddField(field.name);
                        }
                    }
                    else if (!field.name.Contains(":") && where.Contains(field.name.ToLower()))  // select all fields in the where clause (because you need them in table.Select(...)
                    {
                        filter.AddField(field.name);
                    }
                }
                if (isCrossTableQuery)
                {
                    where = filter.WhereClause;
                    filter.WhereClause = String.Empty;
                    IFeatureCursor cursor = new FeatureCursor(_fc.GetFeatures(filter), (FeatureLayerJoins)_joins.Clone(), fieldNames);

                    DataTable tab = gView.Framework.Data.FeatureCursor.ToDataTable(cursor);
                    DataRow[] rows = null;
                    try
                    {
                        rows = tab.Select(where, filter.OrderBy);
                    }
                    catch
                    {
                    }
                    return new gView.Framework.Data.FeatureCursor.DataRowCursor(rows);
                }
            }
            #endregion

            if (fieldNames.Keys.Count <= 1)
                fieldNames = null;

            try
            {
                return new FeatureCursor(_fc.GetFeatures(filter), (FeatureLayerJoins)_joins.Clone(), fieldNames);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region ITableClass Member

        public ICursor Search(IQueryFilter filter)
        {
            return GetFeatures(filter);
        }

        public ISelectionSet Select(IQueryFilter filter)
        {
            filter.SubFields = this.IDFieldName;

            IFeatureCursor cursor = this.GetFeatures(filter);
            if (cursor == null)
                return null;
            IFeature feat;

            IDSelectionSet selSet = new IDSelectionSet();
            while ((feat = cursor.NextFeature) != null)
            {
                selSet.AddID((int)((uint)feat.OID));
            }
            cursor.Dispose();

            return selSet;
        }

        public IFields Fields
        {
            get { return _fields; }
        }

        public IField FindField(string name)
        {
            if (_fields == null)
                return null;

            foreach (IField field in _fields.ToEnumerable())
            {
                if (field.name == name)
                    return field;
            }

            return null;
        }

        public string IDFieldName
        {
            get { return _fc.IDFieldName; }
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get { return _fc.Name; }
        }

        public string Aliasname
        {
            get { return _fc.Aliasname; }
        }

        public IDataset Dataset
        {
            get { return _fc.Dataset; }
        }

        #endregion

        #region IGeometryDef Member

        public bool HasZ
        {
            get { return _fc.HasZ; }
        }

        public bool HasM
        {
            get { return _fc.HasM; }
        }

        public Geometry.ISpatialReference SpatialReference
        {
            get { return _fc.SpatialReference; }
        }

        public Geometry.geometryType GeometryType
        {
            get { return _fc.GeometryType; }
        }

        #endregion

        #region Helper

        #endregion

        #region Helper Classes

        private class FeatureCursor : IFeatureCursor
        {
            private IFeatureCursor _cursor;
            private FeatureLayerJoins _joins;
            private Dictionary<string, UniqueList<string>> _fieldNames = null;
            private List<IFeature> _features = new List<IFeature>();
            private int _pos = 0;

            public FeatureCursor(IFeatureCursor cursor, FeatureLayerJoins joins, Dictionary<string, UniqueList<string>> fieldNames)
            {
                _cursor = cursor;
                _joins = joins;
                _fieldNames = fieldNames;

                if (_fieldNames != null)
                {
                    foreach (string joinName in _fieldNames.Keys)
                    {
                        IFeatureLayerJoin join = _joins[joinName];
                        if (join != null)
                            join.Init(_fieldNames[joinName].ToString(','));
                    }
                }
            }

            private void Collect()
            {
                _features.Clear();
                while (true)
                {
                    IFeature feature = _cursor.NextFeature;
                    if (feature != null)
                    {
                        _features.Add(feature);
                    }
                    if (feature == null || _features.Count > 100)
                        break;
                }
            }

            private void Join()
            {
                if (_fieldNames == null || _joins == null)
                    return;

                #region Collect values & perform cache query
                
                foreach (string joinName in _fieldNames.Keys)
                {
                    if (String.IsNullOrEmpty(joinName))
                        continue;

                    IFeatureLayerJoin join = _joins[joinName];
                    if (join == null)
                        continue;

                    List<string> vals = new List<string>();
                    foreach (IFeature feature in _features)
                    {
                        object joinVal = feature[join.Field];
                        if (joinVal == null)
                            continue;

                        if (!vals.Contains(joinVal.ToString()))
                            vals.Add(joinVal.ToString());
                    }
                    join.PerformCacheQuery(vals.ToArray());
                }

                #endregion

                List<IFeature> removeFeatures = new List<IFeature>();
                foreach (IFeature feature in _features)
                {
                    if (_fieldNames != null && _joins != null)
                    {
                        foreach (string joinName in _fieldNames.Keys)
                        {
                            if (String.IsNullOrEmpty(joinName))
                                continue;
                            IFeatureLayerJoin join = _joins[joinName];
                            if (join == null)
                                continue;

                            object joinVal = feature[join.Field];
                            if (joinVal == null)
                                continue;

                            IRow row = join.GetJoinedRow(joinVal.ToString());
                            if (row == null)
                            {
                                if (join.JoinType == joinType.LeftInnerJoin)
                                {
                                    removeFeatures.Add(feature);
                                    break;
                                }

                                continue;
                            }

                            foreach (string fieldName in _fieldNames[joinName])
                            {
                                if (fieldName == "*" && join.JoinFields != null)
                                {
                                    foreach (IField f in join.JoinFields.ToEnumerable())
                                    {
                                        object v = row[f.name];
                                        feature.Fields.Add(new FieldValue(joinName + ":" + f.name, v));
                                    }
                                    break;
                                }
                                object val = row[fieldName];
                                feature.Fields.Add(new FieldValue(joinName + ":" + fieldName, val));
                            }
                        }
                    }
                }

                foreach (IFeature feature in removeFeatures)
                    _features.Remove(feature);
            }

            #region IFeatureCursor Member

            public IFeature NextFeature
            {
                get
                {
                    if (_pos >= _features.Count)
                    {
                        _pos = 0;

                        Collect();
                        if (_features.Count == 0)
                            return null;
                        Join();
                        if (_features.Count == 0)
                            return NextFeature;
                    }

                    IFeature feature = _features[_pos++];

                    return feature;
                }
            }

            #endregion

            #region IDisposable Member

            public void Dispose()
            {
                if (_cursor != null)
                {
                    _cursor.Dispose();
                    _cursor = null;
                }
                foreach (IFeatureLayerJoin join in _joins)
                    join.Dispose();
            }

            #endregion
        }

        private class FeatureCursorWrapper : IFeatureCursor
        {
            private ICursor _cursor;

            public FeatureCursorWrapper(ICursor cursor)
            {
                _cursor = cursor;
            }

            #region IFeatureCursor Member

            public IFeature NextFeature
            {
                get
                {
                    if (_cursor is IFeatureCursor)
                    {
                        return ((IFeatureCursor)_cursor).NextFeature;
                    }
                    if (_cursor is IRowCursor)
                    {
                        IRow row = ((IRowCursor)_cursor).NextRow;
                        if (row == null)
                            return null;

                        Feature feature = new Feature(row);
                        return feature;
                    }
                    return null;
                }
            }

            #endregion

            #region IDisposable Member

            public void Dispose()
            {
                if (_cursor != null)
                {
                    _cursor.Dispose();
                    _cursor = null;
                }
            }

            #endregion
        }

        #endregion
    }
}