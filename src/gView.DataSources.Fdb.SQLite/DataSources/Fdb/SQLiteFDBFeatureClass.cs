using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.SQLite
{
    public class SQLiteFDBFeatureClass : IFeatureClass2, IRefreshable, IFeatureCursorRequiresWrapperForOrdering
    {
        private SQLiteFDB _fdb;
        private IDataset _dataset;
        private string _name = String.Empty, _aliasname = String.Empty;
        private string _idField = String.Empty, _shapeField = String.Empty;
        private FieldCollection _fields;
        private IEnvelope _envelope = null;

        private BinarySearchTree _searchTree = null;
        private GeometryDef _geomDef;
        private string _dbSchema = String.Empty;

        private SQLiteFDBFeatureClass()
        {

        }

        async static public Task<SQLiteFDBFeatureClass> Create(SQLiteFDB fdb, IDataset dataset, GeometryDef geomDef)
        {
            var fc = new SQLiteFDBFeatureClass();

            fc._fdb = fdb;

            fc._dataset = dataset;
            fc._geomDef = (geomDef != null) ? geomDef : new GeometryDef();

            if (fc._geomDef != null && fc._geomDef.SpatialReference == null && dataset is IFeatureDataset)
            {
                fc._geomDef.SpatialReference = await ((IFeatureDataset)dataset).GetSpatialReference();
            }

            fc._fields = new FieldCollection();

            return fc;
        }

        async static public Task<SQLiteFDBFeatureClass> Create(SQLiteFDB fdb, IDataset dataset, GeometryDef geomDef, BinarySearchTree tree)
        {
            var fc = await Create(fdb, dataset, geomDef);
            fc._searchTree = tree;

            return fc;
        }

        #region FeatureClass Member

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                _dbSchema = String.Empty;
            }
        }
        public string Aliasname { get { return _aliasname; } set { _aliasname = value; } }

        async public Task<int> CountFeatures()
        {
            if (_fdb == null)
            {
                return -1;
            }

            return await _fdb.CountFeatures(_name);
        }

        async public Task<List<SpatialIndexNode>> SpatialIndexNodes()
        {
            if (_fdb == null)
            {
                return null;
            }

            return await _fdb.SpatialIndexNodes2(_name);
        }

        async public Task<IFeatureCursor> GetFeatures(IQueryFilter filter/*, gView.Framework.Data.getFeatureQueryType type*/)
        {
            if (_fdb == null)
            {
                return null;
            }

            if (filter != null)
            {
                filter.AddField("FDB_OID");
            }

            if (filter is IRowIDFilter)
            {
                filter.fieldPostfix = filter.fieldPrefix = "\"";
                return await _fdb.QueryIDs(this, filter.SubFieldsAndAlias, ((IRowIDFilter)filter).IDs, filter.FeatureSpatialReference, filter.DatumTransformations);
            }
            else
            {
                return await _fdb.Query(this, filter);
            }
        }

        async public Task<ICursor> Search(IQueryFilter filter)
        {
            return await GetFeatures(filter);
        }

        #region ITableClass2

        /// <summary>
        /// Fast count for <c>returnCountOnly</c> queries (see <see cref="ITableClass2"/>).
        /// <list type="bullet">
        ///   <item><b>Classic</b> storage: no SQL spatial index to push a count into - counts by
        ///     iterating, same as before this interface existed, just without materializing
        ///     unrequested attribute fields.</item>
        ///   <item><b>SpatiaLite</b> storage: <c>SELECT count(...) WHERE ...</c> straight in SQL - the
        ///     precise relation (<c>ST_Intersects</c> etc.) is exact there too, so this is always exact
        ///     and never touches a single row of geometry.</item>
        ///   <item><b>GeoPackage</b> storage: same SQL shortcut when there is no spatial filter or it
        ///     is a plain bbox test (no in-database <c>ST_Intersects</c> without mod_spatialite); a
        ///     precise relation (Intersects/Within/...) falls back to iterating, since only the R-Tree
        ///     bbox candidates are known in SQL and the exact test needs the decoded geometry.</item>
        /// </list>
        /// </summary>
        async public Task<int> ExecuteCount(IQueryFilter filter)
        {
            filter ??= new QueryFilter();

            if (_fdb == null)
            {
                return -1;
            }

            var storage = (_dataset as IFDBDataset)?.SpatialIndexDef?.StorageType ?? GeometryStorageType.Classic;

            if (SQLiteFDB.IsSpatiaLiteStorage(storage))
            {
                var flavor = gView.DataSources.SpatiaLite.SpatiaLiteSchema.FlavorFor(storage);

                if (!Cursors.SQLiteNativeFeatureCursor.NeedsPreciseRowFilter(flavor, filter))
                {
                    return await _fdb.ExecuteNativeCountAsync(this, filter, flavor);
                }
                // GeoPackage + a precise spatial relation: no in-DB ST_Intersects - fall through
                // to the per-row count below (still cheaper than the caller's own full feature scan,
                // no attribute fields other than the id are fetched).
            }

            var countFilter = (IQueryFilter)filter.Clone();
            countFilter.SubFields = !String.IsNullOrEmpty(_idField) ? _idField : "*";

            int count = 0;
            using (IFeatureCursor cursor = await GetFeatures(countFilter))
            {
                if (cursor != null)
                {
                    while (await cursor.NextFeature() != null)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        #endregion

        async public Task<ISelectionSet> Select(IQueryFilter filter)
        {
            filter.SubFields = this.IDFieldName;

            filter.AddField("FDB_SHAPE");
            filter.AddField("FDB_OID");
            using (IFeatureCursor cursor = await _fdb.Query(this, filter))
            {
                IFeature feat;

                SpatialIndexedIDSelectionSet selSet = new SpatialIndexedIDSelectionSet(this.Envelope);
                while ((feat = await cursor.NextFeature()) != null)
                {
                    selSet.AddID(feat.OID, feat.Shape);
                }
                return selSet;
            }
        }

        public IFieldCollection Fields
        {
            get
            {
                return _fields;
            }
        }

        public IField FindField(string name)
        {
            if (_fields == null)
            {
                return null;
            }

            foreach (IField field in _fields.ToEnumerable())
            {
                if (field.name == name)
                {
                    return field;
                }
            }
            return null;
        }

        public string IDFieldName
        {
            get
            {
                return _idField;
            }
            set { _idField = value; }
        }
        public string ShapeFieldName
        {
            get { return _shapeField; }
            set { _shapeField = value; }
        }
        public IEnvelope Envelope
        {
            get { return _envelope; }
            set { _envelope = value; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }
        #endregion

        #region IGeometryDef Member

        public bool HasZ
        {
            get { return _geomDef.HasZ; }
        }

        public bool HasM
        {
            get { return _geomDef.HasM; }
        }

        public GeometryType GeometryType
        {
            get { return _geomDef.GeometryType; }
        }

        public ISpatialReference SpatialReference
        {
            get
            {
                return _geomDef.SpatialReference;
            }
            set
            {
                _geomDef.SpatialReference = value;
            }
        }
        #endregion

        #region IRefreshable Member

        public void RefreshFrom(object obj)
        {
            if (!(obj is SQLiteFDBFeatureClass))
            {
                return;
            }

            SQLiteFDBFeatureClass fc = (SQLiteFDBFeatureClass)obj;
            if (fc.Name != this.Name)
            {
                return;
            }

            this.Envelope = fc.Envelope;
            this.SpatialReference = fc.SpatialReference;
            this.IDFieldName = fc.IDFieldName;
            this.ShapeFieldName = fc.ShapeFieldName;

            _geomDef.GeometryType = fc.GeometryType;
            _geomDef.HasZ = fc.HasZ;
            _geomDef.HasM = fc.HasM;

            FieldCollection fields = new FieldCollection(fc.Fields);
            if (fields != null)
            {
                fields.PrimaryDisplayField = _fields.PrimaryDisplayField;
            }

            _fields = fields;
        }

        #endregion

        public string DbSchema
        {
            get { return _dbSchema; }
        }

        public string DbTableName
        {
            get
            {
                string name = _name;
                if (name.Contains("@"))
                {
                    name = _fdb.SpatialViewNames(name)[1];
                }
                else
                {
                    name = "FC_" + name;
                }

                return (String.IsNullOrEmpty(_dbSchema) ? "\"" + name + "\"" : _dbSchema + ".\"" + name + "\"");
            }
        }

        public string SiDbTableName
        {
            get
            {
                string name = _name;
                if (name.Contains("@"))
                {
                    name = _fdb.SpatialViewNames(name)[0];
                }

                return (String.IsNullOrEmpty(_dbSchema) ? "\"FCSI_" + name + "\"" : "\"" + _dbSchema + "\".\"FCSI_" + name + "\"");
            }
        }
    }
}
