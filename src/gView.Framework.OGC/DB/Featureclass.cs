using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using gView.Framework.Data;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace gView.Framework.OGC.DB
{
    public class OgcSpatialFeatureclass : IFeatureClass2, IDebugging
    {
        protected string _name, _shapefield, _idfield;
        protected GeometryType _geomType;
        protected bool _hasZ = false;
        protected OgcSpatialDataset _dataset;
        protected IEnvelope _envelope;
        protected FieldCollection _fields = new FieldCollection();
        protected ISpatialReference _sRef = null;
        internal string _geometry_columns_type = String.Empty;

        protected OgcSpatialFeatureclass() { }
        private OgcSpatialFeatureclass(OgcSpatialDataset dataset, DataRow geometry_columns_row)
        {

        }

        async public static Task<IFeatureClass> Create(OgcSpatialDataset dataset, DataRow geometry_columns_row)
        {
            var featureClass = new OgcSpatialFeatureclass(dataset, geometry_columns_row);

            featureClass._dataset = dataset;

            if (featureClass._dataset == null || geometry_columns_row == null)
            {
                return featureClass;
            }

            try
            {
                featureClass._lastException = null;

                string schema = String.Empty;
                try
                {
                    if (!String.IsNullOrEmpty(featureClass._dataset.OgcDictionary("geometry_columns.f_table_schema")))
                    {
                        schema = geometry_columns_row[featureClass._dataset.OgcDictionary("geometry_columns.f_table_schema")].ToString();
                    }

                    if (!String.IsNullOrEmpty(schema))
                    {
                        schema += ".";
                    }
                }
                catch { schema = ""; }
                featureClass._name = schema + geometry_columns_row[featureClass._dataset.OgcDictionary("geometry_columns.f_table_name")].ToString();
                featureClass._shapefield = geometry_columns_row[featureClass._dataset.OgcDictionary("geometry_columns.f_geometry_column")].ToString();
                featureClass._idfield = featureClass._dataset.OgcDictionary("gid");

                // Read Primary Key -> PostGIS id is not always "gid";
                string pKey = featureClass.GetPKey();
                if (!String.IsNullOrWhiteSpace(pKey) && !pKey.Equals(featureClass._idfield))
                {
                    featureClass._idfield = pKey;
                }

                featureClass._geometry_columns_type = geometry_columns_row[featureClass._dataset.OgcDictionary("geometry_columns.type")].ToString().ToUpper();
                switch (featureClass._geometry_columns_type)
                {
                    case "MULTIPOLYGON":
                    case "POLYGON":
                    case "MULTIPOLYGONM":
                    case "POLYGONM":
                        featureClass._geomType = GeometryType.Polygon;
                        break;
                    case "MULTILINESTRING":
                    case "LINESTRING":
                    case "MULTILINESTRINGM":
                    case "LINESTRINGM":
                        featureClass._geomType = GeometryType.Polyline;
                        break;
                    case "POINT":
                    case "POINTM":
                    case "MULTIPOINT":
                    case "MULTIPOINTM":
                        featureClass._geomType = GeometryType.Point;
                        break;
                    default:
                        featureClass._geomType = GeometryType.Unknown;
                        break;
                }

                featureClass._hasZ = (int)geometry_columns_row[featureClass._dataset.OgcDictionary("geometry_columns.coord_dimension")] == 3;

                try
                {
                    int srid = int.Parse(geometry_columns_row[featureClass._dataset.OgcDictionary("geometry_columns.srid")].ToString());
                    if (srid > 0)
                    {
                        featureClass._sRef = gView.Framework.Geometry.SpatialReference.FromID("epsg:" + srid.ToString());
                    }
                    else
                    {
                        featureClass._sRef = await TrySelectSpatialReference(dataset, featureClass);
                    }
                }
                catch { }
                await featureClass.ReadSchema();
            }
            catch (Exception ex)
            {
                featureClass._lastException = ex;
                string msg = ex.Message;
            }

            return featureClass;
        }

        private Exception _lastException = null;

        async protected Task ReadSchema()
        {
            if (_dataset == null)
            {
                return;
            }

            try
            {
                // Fields
                using (DbConnection dbConnection = _dataset.ProviderFactory.CreateConnection())
                {
                    dbConnection.ConnectionString = _dataset.ConnectionString;
                    await dbConnection.OpenAsync();

                    DbCommand command = _dataset.ProviderFactory.CreateCommand();
                    command.CommandText = _dataset.SelectReadSchema(this.Name); // "select * from " + this.Name;
                    command.Connection = dbConnection;

                    //NpgsqlCommand command = new NpgsqlCommand("select * from " + this.Name, connection);
                    using (DbDataReader schemareader = await command.ExecuteReaderAsync(CommandBehavior.SchemaOnly))
                    {
                        DataTable schema = schemareader.GetSchemaTable();

                        bool foundId = false, foundShape = false;
                        foreach (DataRow row in schema.Rows)
                        {
                            if (row["ColumnName"].ToString() == _idfield && foundId == false)
                            {
                                foundId = true;
                                _fields.Add(new Field(_idfield, FieldType.ID,
                                    Convert.ToInt32(row["ColumnSize"]),
                                    Convert.ToInt32(row["NumericPrecision"])));
                                continue;
                            }
                            else if (row["ColumnName"].ToString() == _shapefield && foundShape == false)
                            {
                                foundShape = true;
                                _fields.Add(new Field(_shapefield, FieldType.Shape));
                                continue;
                            }

                            if (schema.Columns["IsIdentity"] != null && row["IsIdentity"] != null && (bool)row["IsIdentity"] == true)
                            {
                                if (foundId == false)
                                {
                                    _idfield = row["ColumnName"].ToString();
                                }

                                foundId = true;

                                _fields.Add(new Field(_idfield, FieldType.ID,
                                    Convert.ToInt32(row["ColumnSize"]),
                                    Convert.ToInt32(row["NumericPrecision"])));
                                continue;
                            }

                            Field field = new Field(row["ColumnName"].ToString());
                            if (row["DataType"] is Type)
                            {
                                if ((Type)row["DataType"] == typeof(System.Int32))
                                {
                                    field.type = FieldType.integer;
                                }
                                else if ((Type)row["DataType"] == typeof(System.Int16))
                                {
                                    field.type = FieldType.smallinteger;
                                }
                                else if ((Type)row["DataType"] == typeof(System.Int64))
                                {
                                    field.type = FieldType.biginteger;
                                }
                                else if ((Type)row["DataType"] == typeof(System.DateTime))
                                {
                                    field.type = FieldType.Date;
                                }
                                else if ((Type)row["DataType"] == typeof(System.Double))
                                {
                                    field.type = FieldType.Double;
                                }
                                else if ((Type)row["DataType"] == typeof(System.Decimal))
                                {
                                    field.type = FieldType.Float;
                                }
                                else if ((Type)row["DataType"] == typeof(System.Boolean))
                                {
                                    field.type = FieldType.boolean;
                                }
                                else if ((Type)row["DataType"] == typeof(System.Char))
                                {
                                    field.type = FieldType.character;
                                }
                                else if ((Type)row["DataType"] == typeof(System.String))
                                {
                                    field.type = FieldType.String;
                                }
                                else if (row["DataType"].ToString() == "Microsoft.SqlServer.Types.SqlGeometry" ||
                                         row["DataType"].ToString() == "Microsoft.SqlServer.Types.SqlGeography")
                                {
                                    if (foundShape == false)
                                    {
                                        _shapefield = row["ColumnName"].ToString();
                                    }

                                    foundShape = true;
                                    field.type = FieldType.String;
                                }
                            }


                            field.size = Convert.ToInt32(row["ColumnSize"]);
                            int precision;
                            if (int.TryParse(row["NumericPrecision"]?.ToString(), out precision))
                            {
                                field.precision = precision;
                            }

                            _fields.Add(field);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }

        public string GeometryTypeString
        {
            get { return _geometry_columns_type; }
        }

        private string GetPKey()
        {
            string pKeySelect = _dataset.IntegerPrimaryKeyField(_name);
            try
            {
                if (!String.IsNullOrWhiteSpace(pKeySelect))
                {
                    using (DbConnection connection = _dataset.ProviderFactory.CreateConnection())
                    {
                        connection.ConnectionString = _dataset.ConnectionString; ;
                        connection.Open();

                        DbCommand command = _dataset.ProviderFactory.CreateCommand();
                        command.CommandText = pKeySelect;
                        command.Connection = connection;

                        return command.ExecuteScalar()?.ToString();
                    }
                }
            }
            catch { }
            return String.Empty;
        }

        #region IFeatureClass Member

        public string ShapeFieldName
        {
            get { return _shapefield; }
        }

        public IEnvelope Envelope
        {
            get
            {
                if (_envelope == null)
                {
                    //var task = Task.Run(() => _dataset.FeatureClassEnvelope(this));
                    //task.Wait();
                    //_envelope = task.Result;

                    _envelope = Task.Run(() => _dataset.FeatureClassEnvelope(this)).Result;
                }
                return _envelope;
            }
        }

        async public Task<int> CountFeatures()
        {

            try
            {
                _lastException = null;
                using (DbConnection connection = _dataset.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = _dataset.ConnectionString;
                    await connection.OpenAsync();

                    DbCommand command = _dataset.ProviderFactory.CreateCommand();
                    command.CommandText = "select count(" + _idfield + ") from " + this.Name;
                    command.Connection = connection;

                    //NpgsqlCommand command = new NpgsqlCommand("SELECT count(" + _idfield + ") from " + this.Name, connection);
                    return Convert.ToInt32(await command.ExecuteScalarAsync());
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
                return 0;
            }
        }

        async public Task<IFeatureCursor> GetFeatures(IQueryFilter filter)
        {
            _lastException = null;
            if (filter is IBufferQueryFilter)
            {
                ISpatialFilter sFilter = await BufferQueryFilter.ConvertToSpatialFilter(filter as IBufferQueryFilter);
                if (sFilter == null)
                {
                    return null;
                }

                return await GetFeatures(sFilter);
            }

            return await OgcSpatialFeatureCursor.Create(this, filter);
        }

        #endregion

        #region ITableClass Member

        async public Task<ICursor> Search(IQueryFilter filter)
        {
            _lastException = null;
            return await GetFeatures(filter);
        }

        async public Task<ISelectionSet> Select(IQueryFilter filter)
        {
            filter.SubFields = this.IDFieldName;
            //if (filter is ISpatialFilter)
            {
                filter.AddField(this.ShapeFieldName);
            }

            using (IFeatureCursor cursor = await OgcSpatialFeatureCursor.Create(this, filter))
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
                //List<IField> fields = new List<IField>();
                //foreach (IField field in _fields)
                //{
                //    fields.Add(field);
                //}
                //return fields;
                return _fields;
            }
        }

        public IField FindField(string name)
        {
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
            get { return _idfield; }
        }

        #endregion

        #region ITableClass2

        async public Task<int> ExecuteCount(IQueryFilter filter)
        {
            filter.GeometryToSpatialReference(this.SpatialReference, filter?.DatumTransformations);

            using (DbConnection connection = _dataset.ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = _dataset.ConnectionString;
                await connection.OpenAsync();

                DbCommand command = _dataset.SelectCommand(this, filter, out string shapeFieldname, "count", this.IDFieldName);
                command.Connection = connection;

                object result = await command.ExecuteScalarAsync();

                return Convert.ToInt32(result);
            }
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get { return _name; }
        }

        public string Aliasname
        {
            get { return _name; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion

        #region IGeometryDef Member

        public bool HasZ
        {
            get { return _hasZ; }
        }

        public bool HasM
        {
            get { return false; }
        }

        virtual public ISpatialReference SpatialReference
        {
            get { return _sRef; }
            set { _sRef = value; }
        }

        public GeometryType GeometryType
        {
            get { return _geomType; }
        }

        public GeometryFieldType GeometryFieldType
        {
            get
            {
                return GeometryFieldType.Default;
            }
        }

        #endregion

        #region IDebugging

        public Exception LastException
        {
            get { return _lastException; }
            set { _lastException = value; }
        }

        #endregion

        async public static Task<ISpatialReference> TrySelectSpatialReference(OgcSpatialDataset dataset, OgcSpatialFeatureclass fc)
        {
            try
            {
                DbCommand sridCommand = dataset.SelectSpatialReferenceIds(fc);
                if (sridCommand != null)
                {
                    using (DbConnection connection = dataset.ProviderFactory.CreateConnection())
                    {
                        connection.ConnectionString = dataset.ConnectionString;
                        sridCommand.Connection = connection;
                        await connection.OpenAsync();

                        using (DbDataReader reader = await sridCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                object sridObj = reader["srid"];
                                if (sridObj != null && sridObj != DBNull.Value)
                                {
                                    int srid = Convert.ToInt32(sridObj);
                                    if (srid > 0)
                                    {
                                        var sRef = gView.Framework.Geometry.SpatialReference.FromID("epsg:" + srid.ToString());
                                        if (sRef != null)
                                        {
                                            return sRef;
                                        }
                                    }
                                }
                            }
                        }

                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return null;
        }
    }
}
