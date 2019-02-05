using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using gView.Framework.Data;
using gView.Framework.Geometry;
using System.Data.Common;

namespace gView.Framework.OGC.DB
{
    public class OgcSpatialFeatureclass : IFeatureClass
    {
        protected string _name, _shapefield, _idfield;
        protected geometryType _geomType;
        protected bool _hasZ = false;
        protected OgcSpatialDataset _dataset;
        protected IEnvelope _envelope;
        protected Fields _fields = new Fields();
        protected ISpatialReference _sRef = null;
        internal string _geometry_columns_type = String.Empty;

        protected OgcSpatialFeatureclass() { }
        public OgcSpatialFeatureclass(OgcSpatialDataset dataset, DataRow geometry_columns_row)
        {
            _dataset = dataset;

            if (_dataset == null || geometry_columns_row == null)
                return;

            try
            {
                _lastException = null;

                string schema = String.Empty;
                try
                {
                    if (!String.IsNullOrEmpty(_dataset.OgcDictionary("geometry_columns.f_table_schema")))
                        schema = geometry_columns_row[_dataset.OgcDictionary("geometry_columns.f_table_schema")].ToString();
                    if (!String.IsNullOrEmpty(schema))
                        schema += ".";
                }
                catch { schema = ""; }
                _name = schema + geometry_columns_row[_dataset.OgcDictionary("geometry_columns.f_table_name")].ToString();
                _shapefield = geometry_columns_row[_dataset.OgcDictionary("geometry_columns.f_geometry_column")].ToString();
                _idfield = _dataset.OgcDictionary("gid");

                // Read Primary Key -> PostGIS id is not always "gid";
                string pKey = GetPKey();
                if (!String.IsNullOrWhiteSpace(pKey) && !pKey.Equals(_idfield))
                    _idfield = pKey;

                _geometry_columns_type = geometry_columns_row[_dataset.OgcDictionary("geometry_columns.type")].ToString().ToUpper();
                switch (_geometry_columns_type)
                {
                    case "MULTIPOLYGON":
                    case "POLYGON":
                    case "MULTIPOLYGONM":
                    case "POLYGONM":
                        _geomType = geometryType.Polygon;
                        break;
                    case "MULTILINESTRING":
                    case "LINESTRING":
                    case "MULTILINESTRINGM":
                    case "LINESTRINGM":
                        _geomType = geometryType.Polyline;
                        break;
                    case "POINT":
                    case "POINTM":
                    case "MULTIPOINT":
                    case "MULTIPOINTM":
                        _geomType = geometryType.Point;
                        break;
                    default:
                        _geomType = geometryType.Unknown;
                        break;
                }

                _hasZ = (int)geometry_columns_row[_dataset.OgcDictionary("geometry_columns.coord_dimension")] == 3;

                try
                {
                    int srid = int.Parse(geometry_columns_row[_dataset.OgcDictionary("geometry_columns.srid")].ToString());
                    if (srid > 0)
                    {
                        _sRef = gView.Framework.Geometry.SpatialReference.FromID("epsg:" + srid.ToString());
                    }
                    else
                    {
                        _sRef = TrySelectSpatialReference(dataset, this);
                    }
                }
                catch { }
                ReadSchema();
            }
            catch (Exception ex)
            {
                _lastException = ex;
                string msg = ex.Message;
            }
        }

        private Exception _lastException = null;

        protected void ReadSchema()
        {
            if (_dataset == null) return;
            try
            {
                // Fields
                using (DbConnection connection = _dataset.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = _dataset.ConnectionString;
                    connection.Open();

                    DbCommand command = _dataset.ProviderFactory.CreateCommand();
                    command.CommandText = _dataset.SelectReadSchema(this.Name); // "select * from " + this.Name;
                    command.Connection = connection;

                    //NpgsqlCommand command = new NpgsqlCommand("select * from " + this.Name, connection);
                    using (DbDataReader schemareader = command.ExecuteReader(CommandBehavior.SchemaOnly))
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
                                    _idfield = row["ColumnName"].ToString();
                                foundId = true;

                                _fields.Add(new Field(_idfield, FieldType.ID,
                                    Convert.ToInt32(row["ColumnSize"]),
                                    Convert.ToInt32(row["NumericPrecision"])));
                                continue;
                            }

                            Field field = new Field(row["ColumnName"].ToString());
                            if (row["DataType"] == typeof(System.Int32))
                                field.type = FieldType.integer;
                            else if (row["DataType"] == typeof(System.Int16))
                                field.type = FieldType.smallinteger;
                            else if (row["DataType"] == typeof(System.Int64))
                                field.type = FieldType.biginteger;
                            else if (row["DataType"] == typeof(System.DateTime))
                                field.type = FieldType.Date;
                            else if (row["DataType"] == typeof(System.Double))
                                field.type = FieldType.Double;
                            else if (row["DataType"] == typeof(System.Decimal))
                                field.type = FieldType.Float;
                            else if (row["DataType"] == typeof(System.Boolean))
                                field.type = FieldType.boolean;
                            else if (row["DataType"] == typeof(System.Char))
                                field.type = FieldType.character;
                            else if (row["DataType"] == typeof(System.String))
                                field.type = FieldType.String;
                            else if (row["DataType"].ToString() == "Microsoft.SqlServer.Types.SqlGeometry" ||
                                     row["DataType"].ToString() == "Microsoft.SqlServer.Types.SqlGeography")
                            {
                                if (foundShape == false)
                                    _shapefield = row["ColumnName"].ToString();
                                foundShape = true;
                                field.type = FieldType.String;
                            }


                            field.size = Convert.ToInt32(row["ColumnSize"]);
                            int precision;
                            if (int.TryParse(row["NumericPrecision"]?.ToString(), out precision))
                                field.precision = precision;

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

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get
            {
                if (_envelope == null)
                {
                    _envelope = _dataset.FeatureClassEnvelope(this);
                }
                return _envelope;
            }
        }

        public int CountFeatures
        {
            get
            {
                try
                {
                    _lastException = null;
                    using (DbConnection connection = _dataset.ProviderFactory.CreateConnection())
                    {
                        connection.ConnectionString = _dataset.ConnectionString;
                        connection.Open();

                        DbCommand command = _dataset.ProviderFactory.CreateCommand();
                        command.CommandText = "select count(" + _idfield + ") from " + this.Name;
                        command.Connection = connection;

                        //NpgsqlCommand command = new NpgsqlCommand("SELECT count(" + _idfield + ") from " + this.Name, connection);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    _lastException = ex;
                    return 0;
                }
            }
        }

        public IFeatureCursor GetFeatures(IQueryFilter filter)
        {
            _lastException = null;
            if (filter is IBufferQueryFilter)
            {
                ISpatialFilter sFilter = BufferQueryFilter.ConvertToSpatialFilter(filter as IBufferQueryFilter);
                if (sFilter == null) return null;

                return GetFeatures(sFilter);
            }

            return new OgcSpatialFeatureCursor(this, filter);
        }

        #endregion

        #region ITableClass Member

        public ICursor Search(IQueryFilter filter)
        {
            _lastException = null;
            return GetFeatures(filter);
        }

        public ISelectionSet Select(IQueryFilter filter)
        {
            filter.SubFields = this.IDFieldName;
            if (filter is ISpatialFilter)
                filter.AddField(this.ShapeFieldName);

            using (IFeatureCursor cursor = (IFeatureCursor)new OgcSpatialFeatureCursor(this, filter))
            {
                IFeature feat;

                SpatialIndexedIDSelectionSet selSet = new SpatialIndexedIDSelectionSet(this.Envelope);
                while ((feat = cursor.NextFeature) != null)
                {
                    selSet.AddID(feat.OID, feat.Shape);
                }
                return selSet;
            }
        }

        public IFields Fields
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
                if (field.name == name) return field;
            }
            return null;
        }

        public string IDFieldName
        {
            get { return _idfield; }
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

        virtual public gView.Framework.Geometry.ISpatialReference SpatialReference
        {
            get { return _sRef; }
        }

        public gView.Framework.Geometry.geometryType GeometryType
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

        public static ISpatialReference TrySelectSpatialReference(OgcSpatialDataset dataset, OgcSpatialFeatureclass fc)
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
                        connection.Open();

                        using (DbDataReader reader = sridCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                object sridObj = reader["srid"];
                                if (sridObj != null && sridObj != DBNull.Value)
                                {
                                    int srid = Convert.ToInt32(sridObj);
                                    if (srid > 0)
                                    {
                                        var sRef = gView.Framework.Geometry.SpatialReference.FromID("epsg:" + srid.ToString());
                                        if (sRef != null)
                                            return sRef;
                                    }
                                }
                            }
                        }

                        connection.Close();
                    }
                }
            }
            catch(Exception ex) 
            {
                string msg = ex.Message;
            }
            return null;
        }
    }
}
