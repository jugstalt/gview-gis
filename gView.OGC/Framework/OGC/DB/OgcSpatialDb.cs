using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.FDB;
using System.Data.Common;
using gView.Framework.Geometry;
using System.Data;
using gView.Framework.Editor.Core;

namespace gView.Framework.OGC.DB
{
    public abstract class OgcSpatialDataset : DatasetMetadata, IFeatureDataset2, IPersistable, IFeatureDatabase, IEditableDatabase
    {
        protected string _connectionString = "", _errMsg = "";
        protected List<IDatasetElement> _layers;
        private DatasetState _state = DatasetState.unknown;

        protected delegate void ConnectionStringChangedEventHandler(OgcSpatialDataset dataset, string provider);
        protected event ConnectionStringChangedEventHandler ConnectionStringChanged = null;

        #region IFeatureDataset Member

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get
            {
                IEnvelope env = null;
                foreach (IDatasetElement element in this.Elements)
                {
                    if (element != null && element.Class is IFeatureClass)
                    {
                        IEnvelope fcEnv = ((IFeatureClass)element.Class).Envelope;
                        if (fcEnv != null)
                        {
                            if (env == null)
                                env = new Envelope(fcEnv);
                            else
                                env.Union(fcEnv);
                        }
                    }
                }

                if (env != null)
                    return env;

                return new Envelope();
            }
        }

        public gView.Framework.Geometry.ISpatialReference SpatialReference
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        #endregion

        #region IDataset Member

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                string provider = String.Empty;
                if (value.ToLower().IndexOf("sql:") == 0 ||
                   value.ToLower().IndexOf("sqlclient:") == 0)
                {
                    _connectionString = value.Substring(value.IndexOf(":") + 1, value.Length - value.IndexOf(":") - 1);
                    provider = "SqlClient";
                }
                else if (value.ToLower().IndexOf("odbc:") == 0)
                {
                    _connectionString = value.Substring(value.IndexOf(":") + 1, value.Length - value.IndexOf(":") - 1);
                    provider = "Odbc";
                }
                else if (value.ToLower().IndexOf("oracle:") == 0 ||
                        value.ToLower().IndexOf("oracleclient:") == 0)
                {
                    _connectionString = value.Substring(value.IndexOf(":") + 1, value.Length - value.IndexOf(":") - 1);
                    provider = "OracleClient";
                }
                else if (value.ToLower().IndexOf("npgsql:") == 0)
                {
                    _connectionString = value.Substring(value.IndexOf(":") + 1, value.Length - value.IndexOf(":") - 1);
                    provider = "Npgsql";
                }
                else if (value.ToLower().IndexOf("oledb:") == 0)
                {
                    _connectionString = value.Substring(value.IndexOf(":") + 1, value.Length - value.IndexOf(":") - 1);
                    provider = "OleDb";
                }
                else
                {
                    _connectionString = value;
                }

                if (ConnectionStringChanged != null)
                    ConnectionStringChanged(this, provider);
            }
        }

        virtual public string DatasetGroupName
        {
            get { return "OgcSpatialDatabase"; }
        }

        public string DatasetName
        {
            get { return "OgcSpatialDatabase"; }
        }

        public string ProviderName
        {
            get { return "gViewGIS"; }
        }

        public DatasetState State
        {
            get { return _state; }
        }

        virtual public bool Open()
        {
            _state = DatasetState.opened;
            return true;
        }

        public string lastErrorMsg
        {
            get { return _errMsg; }
        }

        public Exception lastException { get { return null; } }

        public int order
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        public IDatasetEnum DatasetEnum
        {
            get { return null; }
        }

        virtual public List<IDatasetElement> Elements
        {
            get
            {
                if (_layers == null || _layers.Count == 0)
                {
                    List<IDatasetElement> layers = new List<IDatasetElement>();
                    DataTable tab = new DataTable();
                    try
                    {
                        using (DbConnection conn = this.ProviderFactory.CreateConnection())
                        {
                            conn.ConnectionString = _connectionString;
                            conn.Open();

                            DbDataAdapter adapter = this.ProviderFactory.CreateDataAdapter();
                            adapter.SelectCommand = this.ProviderFactory.CreateCommand();
                            adapter.SelectCommand.CommandText = "SELECT * FROM " + DbSchemaPrefix + OgcDictionary("geometry_columns");
                            adapter.SelectCommand.Connection = conn;
                            adapter.Fill(tab);
                            conn.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        _errMsg = ex.Message;
                        return layers;
                    }

                    foreach (DataRow row in tab.Rows)
                    {
                        OgcSpatialFeatureclass fc = new OgcSpatialFeatureclass(this, row);
                        layers.Add(new DatasetElement(fc));
                    }

                    _layers = layers;
                }
                return _layers;
            }
        }

        public string Query_FieldPrefix
        {
            get { return ""; }
        }

        public string Query_FieldPostfix
        {
            get { return ""; }
        }

        public gView.Framework.FDB.IDatabase Database
        {
            get { return this; }
        }

        virtual public IDatasetElement this[string title]
        {
            get
            {
                try
                {
                    DataTable tab = new DataTable();
                    using (DbConnection conn = this.ProviderFactory.CreateConnection())
                    {
                        conn.ConnectionString = _connectionString;
                        conn.Open();

                        DbDataAdapter adapter = this.ProviderFactory.CreateDataAdapter();
                        adapter.SelectCommand = this.ProviderFactory.CreateCommand();

                        if (title.Contains("."))
                        {
                            string schema = title.Split('.')[0];
                            string table = title.Substring(schema.Length + 1);
                            adapter.SelectCommand.CommandText = "SELECT * FROM " + DbSchemaPrefix + OgcDictionary("geometry_columns") + " WHERE " + OgcDictionary("geometry_columns.f_table_name") + "='" + table + "' AND " + OgcDictionary("geometry_columns.f_table_schema") + "='" + schema + "'";
                        }
                        else
                        {
                            adapter.SelectCommand.CommandText = "SELECT * FROM " + DbSchemaPrefix + OgcDictionary("geometry_columns") + " WHERE " + OgcDictionary("geometry_columns.f_table_name") + "='" + title + "'";
                        }
                        adapter.SelectCommand.Connection = conn;

                        adapter.Fill(tab);
                        conn.Close();
                    }
                    if (tab.Rows.Count != 1)
                    {
                        _errMsg = "Layer not found: " + title;
                        return null;
                    }

                    return new DatasetElement(new OgcSpatialFeatureclass(this, tab.Rows[0]));
                }
                catch (Exception ex)
                {
                    _errMsg = ex.Message;
                    return null;
                }
            }
        }

        public void RefreshClasses()
        {
        }
        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region IDataset2 Member

        public IDataset2 EmptyCopy()
        {
            OgcSpatialDataset dataset = CreateInstance();
            dataset.ConnectionString = ConnectionString;
            dataset.Open();

            return dataset;
        }

        public void AppendElement(string elementName)
        {
            if (_layers == null) _layers = new List<IDatasetElement>();

            foreach (IDatasetElement e in _layers)
            {
                if (e.Title == elementName) return;
            }

            //IDatasetElement element = _fdb.DatasetElement(this, elementName);
            IDatasetElement element = this[elementName];
            if (element != null) _layers.Add(element);
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_layers != null) _layers.Clear();
            this.ConnectionString = (string)stream.Load("connectionstring", "");
            this.Open();
        }

        public void Save(IPersistStream stream)
        {
            stream.SaveEncrypted("connectionstring", this.ConnectionString);
        }

        #endregion

        #region IDatabase Member

        virtual public bool Create(string name)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public bool Open(string name)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IFeatureDatabase Member

        virtual public int CreateDataset(string name, ISpatialReference sRef)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int CreateFeatureClass(string dsname, string fcname, IGeometryDef geomDef, IFields Fields)
        {
            DatasetNameCase nameCase = DatasetNameCase.ignore;
            foreach (System.Attribute attribute in System.Attribute.GetCustomAttributes(this.GetType()))
            {
                if (attribute is UseDatasetNameCase)
                    nameCase = ((UseDatasetNameCase)attribute).Value;
            }
            switch (nameCase)
            {
                case DatasetNameCase.lower:
                case DatasetNameCase.classNameLower:
                    fcname = fcname.ToLower();
                    break;
                case DatasetNameCase.upper:
                case DatasetNameCase.classNameUpper:
                    fcname = fcname.ToUpper();
                    break;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE " + fcname + " \n(\n");

            Field idField = new Field(OgcDictionary("gid"), FieldType.ID);
            sb.Append(OgcDictionary("gid") + " ");
            sb.Append(DbDictionary(idField));

            Field shapeField = new Field(OgcDictionary("the_geom"), FieldType.Shape);
            if (!String.IsNullOrEmpty(DbDictionary(shapeField)))
            {
                sb.Append(",\n");
                sb.Append(OgcDictionary("the_geom") + " ");
                sb.Append(DbDictionary(shapeField));
            }

            foreach (IField field in Fields.ToEnumerable())
            {
                if (field.type == FieldType.ID ||
                    field.type == FieldType.Shape) continue;

                string fieldName = field.name;
                switch (nameCase)
                {
                    case DatasetNameCase.lower:
                    case DatasetNameCase.classNameLower:
                        fieldName = fieldName.ToLower();
                        break;
                    case DatasetNameCase.upper:
                    case DatasetNameCase.classNameUpper:
                        fieldName = fieldName.ToUpper();
                        break;
                }

                sb.Append(",\n");
                sb.Append(DbColumnName(fieldName) + " ");

                sb.Append(DbDictionary(field));
            }

            //sb.Append(" the_geom  geometry,\n");
            string geomTypeString = "";
            switch (geomDef.GeometryType)
            {
                case geometryType.Point:
                    geomTypeString = "POINT";
                    break;
                case geometryType.Polyline:
                    geomTypeString = "MULTILINESTRING";
                    break;
                case geometryType.Polygon:
                    geomTypeString = "MULTIPOLYGON";
                    break;
                default:
                    _errMsg = "Geometrytype not implemented...";
                    return -1;
            }

            sb.Append(")\n");
            try
            {
                using (DbConnection connection = this.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = _connectionString;
                    connection.Open();

                    DbCommand command = this.ProviderFactory.CreateCommand();
                    command.CommandText = sb.ToString();
                    command.Connection = connection;
                    command.ExecuteNonQuery();

                    command.CommandText = CreateGidSequence(fcname);
                    if (!String.IsNullOrEmpty(command.CommandText))
                        command.ExecuteNonQuery();
                    command.CommandText = CreateGidTrigger(fcname, OgcDictionary("gid"));
                    if (!String.IsNullOrEmpty(command.CommandText))
                        command.ExecuteNonQuery();

                    command.CommandText = AddGeometryColumn("", fcname, OgcDictionary("the_geom"), "-1", geomTypeString);
                    if (!String.IsNullOrEmpty(command.CommandText))
                        command.ExecuteNonQuery();
                }
                return 0;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return -1;
            }
        }

        virtual protected string DbColumnName(string colName)
        {
            return "\"" + colName + "\"";
        }

        virtual protected string DbParameterName(string name)
        {
            return "@" + name;
        }

        IFeatureDataset IFeatureDatabase.this[string name]
        {
            get
            {
                if (this.DatasetName == name)
                    return this;
                else
                    return null;
            }
        }

        virtual public bool DeleteFeatureClass(string name)
        {
            try
            {
                using (DbConnection connection = this.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = this.ConnectionString;
                    connection.Open();

                    //NpgsqlCommand command = new NpgsqlCommand("DROP TABLE " + name, connection);
                    //NpgsqlCommand command = new NpgsqlCommand("SELECT DropGeometryTable ('','" + name + "')", connection);
                    DbCommand command = this.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    foreach (var commandText in DropGeometryTable("", name).Split(';'))
                    {
                        command.CommandText = commandText;
                        command.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return false;
            }
        }

        virtual public bool DeleteDataset(string dsName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public bool RenameDataset(string name, string newName)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        virtual public bool RenameFeatureClass(string name, string newName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public string[] DatasetNames
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public IFeatureCursor Query(IFeatureClass fc, IQueryFilter filter)
        {
            if (fc == null) return null;
            return fc.GetFeatures(filter);
        }

        #endregion

        #region IFeatureUpdater Member

        Encoding _encoder = new UTF7Encoding();
        virtual public bool Insert(IFeatureClass fClass, IFeature feature)
        {
            List<IFeature> features = new List<IFeature>();
            features.Add(feature);
            return Insert(fClass, features);
        }

        virtual public bool Insert(IFeatureClass fClass, List<IFeature> features)
        {
            DatasetNameCase nameCase = DatasetNameCase.ignore;
            foreach (System.Attribute attribute in System.Attribute.GetCustomAttributes(this.GetType()))
            {
                if (attribute is UseDatasetNameCase)
                    nameCase = ((UseDatasetNameCase)attribute).Value;
            }

            if (fClass == null) return false;

            if (!CanEditFeatureClass(fClass, EditCommands.Insert))
                return false;

            try
            {
                int srid = 0;
                if (fClass.SpatialReference != null)
                {
                    string sridName = fClass.SpatialReference.Name;
                    if (sridName.ToLower().StartsWith("epsg:"))
                    {
                        srid = int.Parse(sridName.Split(':')[1]);
                    }
                }

                using (DbConnection connection = this.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = _connectionString;
                    connection.Open();

                    DbCommand command = this.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    foreach (IFeature feature in features)
                    {
                        StringBuilder fields = new StringBuilder(), parameters = new StringBuilder();
                        command.Parameters.Clear();
                        if (feature.Shape != null)
                        {
                            bool asParameter;
                            object shapeObject = this.ShapeParameterValue((OgcSpatialFeatureclass)fClass, feature.Shape,
                                feature.Shape.Srs != null && feature.Shape.Srs > 0 ? (int)feature.Shape.Srs : srid,
                                out asParameter);
                            if (asParameter == true)
                            {
                                DbParameter parameter = this.ProviderFactory.CreateParameter();
                                parameter.ParameterName = this.DbParameterName(fClass.ShapeFieldName);
                                parameter.Value = shapeObject != null ? shapeObject : DBNull.Value;
                                fields.Append(this.DbColumnName(fClass.ShapeFieldName));

                                string paramExpresssion = InsertShapeParameterExpression((OgcSpatialFeatureclass)fClass, feature.Shape);
                                if (!String.IsNullOrWhiteSpace(paramExpresssion))
                                    paramExpresssion = String.Format(paramExpresssion, this.DbParameterName(fClass.ShapeFieldName));
                                else
                                    paramExpresssion = this.DbParameterName(fClass.ShapeFieldName);
                                parameters.Append(paramExpresssion);
                                command.Parameters.Add(parameter);
                            }
                            else
                            {
                                fields.Append(this.DbColumnName(fClass.ShapeFieldName));
                                parameters.Append(shapeObject.ToString());
                            }
                        }

                        foreach (IFieldValue fv in feature.Fields)
                        {
                            string fvName = fv.Name;
                            switch (nameCase)
                            {
                                case DatasetNameCase.lower:
                                case DatasetNameCase.classNameLower:
                                    fvName = fvName.ToLower();
                                    break;
                                case DatasetNameCase.upper:
                                case DatasetNameCase.classNameUpper:
                                    fvName = fvName.ToUpper();
                                    break;
                            }

                            if (fvName == fClass.IDFieldName || fvName == fClass.ShapeFieldName) continue;

                            IField field = fClass.FindField(fvName);
                            if (field == null) continue;

                            if (fields.Length != 0) fields.Append(",");
                            if (parameters.Length != 0) parameters.Append(",");

                            object val = fv.Value;

                            DbParameter parameter = this.ProviderFactory.CreateParameter();
                            parameter.ParameterName = DbParameterName(fvName);
                            try
                            {
                                parameter.Value = val;
                            }
                            catch
                            {
                                if (val != null)
                                    parameter.Value = val.ToString();
                            }
                            //NpgsqlParameter parameter = new NpgsqlParameter("@" + fv.Name, val);

                            fields.Append(this.DbColumnName(fvName));
                            parameters.Append(DbParameterName(fvName));
                            command.Parameters.Add(parameter);
                        }

                        command.CommandText = "INSERT INTO " + fClass.Name + " (" + fields.ToString() + ") VALUES (" + parameters + ")";
                        try
                        {
                            command.CommandTimeout = 600;
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            _errMsg = ex.Message;
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return false;
            }
        }

        public bool Update(IFeatureClass fClass, IFeature feature)
        {
            if (feature == null || fClass == null) return false;
            List<IFeature> features = new List<IFeature>();
            features.Add(feature);

            return Update(fClass, features);
        }

        public bool Update(IFeatureClass fClass, List<IFeature> features)
        {
            if (fClass == null) return false;

            if (!CanEditFeatureClass(fClass, EditCommands.Update))
                return false;

            try
            {
                using (DbConnection connection = this.ProviderFactory.CreateConnection())
                {
                    int srid = 0;
                    if (fClass.SpatialReference != null)
                    {
                        string sridName = fClass.SpatialReference.Name;
                        if (sridName.ToLower().StartsWith("epsg:"))
                        {
                            srid = int.Parse(sridName.Split(':')[1]);
                        }
                    }

                    connection.ConnectionString = _connectionString;
                    connection.Open();


                    DbCommand command = this.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    foreach (IFeature feature in features)
                    {
                        StringBuilder fields = new StringBuilder();
                        command.Parameters.Clear();
                        if (feature.Shape != null)
                        {
                            bool asParameter;
                            object shapeObject = this.ShapeParameterValue((OgcSpatialFeatureclass)fClass, feature.Shape,
                                feature.Shape.Srs != null && feature.Shape.Srs > 0 ? (int)feature.Shape.Srs : srid, 
                                out asParameter);
                            if (asParameter == true)
                            {
                                DbParameter parameter = this.ProviderFactory.CreateParameter();
                                parameter.ParameterName = "@" + fClass.ShapeFieldName;
                                parameter.Value = shapeObject != null ? shapeObject : DBNull.Value;
                                fields.Append(DbColumnName(fClass.ShapeFieldName) + "=" + DbParameterName(fClass.ShapeFieldName));
                                command.Parameters.Add(parameter);
                            }
                            else
                            {
                                fields.Append(DbColumnName(fClass.ShapeFieldName) + "=" + shapeObject.ToString());
                            }
                        }

                        foreach (IFieldValue fv in feature.Fields)
                        {
                            if (fv.Name == fClass.IDFieldName || fv.Name == fClass.ShapeFieldName) continue;

                            IField field = fClass.FindField(fv.Name);
                            if (field == null) continue;

                            if (fields.Length != 0) fields.Append(",");

                            object val = fv.Value;

                            DbParameter parameter = this.ProviderFactory.CreateParameter();
                            parameter.ParameterName = DbParameterName( fv.Name);
                            parameter.Value = val;
                            //NpgsqlParameter parameter = new NpgsqlParameter("@" + fv.Name, val);
                            fields.Append(DbColumnName(fv.Name) + "=" + DbParameterName(fv.Name));
                            command.Parameters.Add(parameter);
                        }

                        command.CommandText = "UPDATE " + fClass.Name + " SET " + fields.ToString() + " WHERE " + fClass.IDFieldName + "=" + feature.OID;
                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return false;
            }
        }

        public bool Delete(IFeatureClass fClass, int oid)
        {
            if (fClass == null) return false;
            return Delete(fClass, fClass.IDFieldName + "=" + oid.ToString());
        }

        public bool Delete(IFeatureClass fClass, string where)
        {
            if (fClass == null) return false;

            if (!CanEditFeatureClass(fClass, EditCommands.Delete))
                return false;

            try
            {
                using (DbConnection connection = this.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = _connectionString;
                    connection.Open();


                    DbCommand command = this.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "DELETE FROM " + fClass.Name + ((where != String.Empty) ? " WHERE " + where : "");

                    command.ExecuteNonQuery();
                    connection.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return false;
            }
        }

        public int SuggestedInsertFeatureCountPerTransaction
        {
            get { return 50; }
        }

        #endregion

        public abstract DbProviderFactory ProviderFactory
        {
            get;
        }
        protected abstract OgcSpatialDataset CreateInstance();

        protected string DbSchemaPrefix
        {
            get
            {
                return ((DbSchema != String.Empty) ? DbSchema + "." : String.Empty);
            }
        }
        virtual public string DbSchema
        {
            get { return String.Empty; }
        }

        virtual public string OgcDictionary(string ogcExpression)
        {
            return Field.shortName(ogcExpression);
        }
        virtual public string DbDictionary(IField field)
        {
            switch (field.type)
            {
                case FieldType.Shape:
                    return String.Empty;
                case FieldType.ID:
                    return "serial PRIMARY KEY";
                case FieldType.smallinteger:
                    return "int2";
                case FieldType.integer:
                    return "int4";
                case FieldType.biginteger:
                    return "int8";
                case FieldType.Float:
                    return "float4";
                case FieldType.Double:
                    return "float8";
                case FieldType.boolean:
                    return "bool";
                case FieldType.character:
                    return "nvarchar(1)";
                case FieldType.Date:
                    return "time";
                case FieldType.String:
                    return "nvarchar(" + ((field.size > 0) ? field.size : 256).ToString() + ")";
                default:
                    return "nvarchar(256)";
            }
        }
        virtual protected string AddGeometryColumn(string schemaName, string tableName, string colunName, string srid, string geomTypeString)
        {
            return "SELECT " + DbSchemaPrefix + "AddGeometryColumn ('" + schemaName + "','" + tableName + "','" + colunName + "','" + srid + "','" + geomTypeString + "','2')";
        }

        virtual public string IntegerPrimaryKeyField(string tableName)
        {
            return String.Empty;
        }

        virtual protected string CreateGidSequence(string tableName)
        {
            return String.Empty;
        }
        virtual protected string CreateGidTrigger(String tableName, string gid)
        {
            return String.Empty;
        }
        virtual protected string DropGeometryTable(string schemaName, string tableName)
        {
            return "SELECT DropGeometryTable ('" + schemaName + "','" + tableName + "')";
        }
        virtual protected object ShapeParameterValue(OgcSpatialFeatureclass featureClass, IGeometry shape, int srid, out bool AsSqlParameter)
        {
            AsSqlParameter = true;

            byte[] geometry = gView.Framework.OGC.OGC.GeometryToWKB(shape, 0, gView.Framework.OGC.OGC.WkbByteOrder.Ndr);
            string geometryString = gView.Framework.OGC.OGC.BytesToHexString(geometry);
            return geometryString;
        }

        virtual protected string InsertShapeParameterExpression(OgcSpatialFeatureclass featureClass, IGeometry shape)
        {
            return "{0}";
        }

        virtual public IEnvelope FeatureClassEnvelope(IFeatureClass fc)
        {
            if (fc == null) return null;

            IEnvelope envelope = null;
            DataTable tab = new DataTable();
            using (DbConnection conn = this.ProviderFactory.CreateConnection())
            {
                conn.ConnectionString = this.ConnectionString;
                // Extent
                using (DbDataAdapter adapter = this.ProviderFactory.CreateDataAdapter()) //new NpgsqlDataAdapter("select extent(" + this.ShapeFieldName + ") from " + this.Name, conn))
                {
                    adapter.SelectCommand = this.ProviderFactory.CreateCommand();
                    adapter.SelectCommand.CommandText = "select st_extent(" + fc.ShapeFieldName + ") from " + DbTableName(fc.Name);
                    adapter.SelectCommand.Connection = conn;

                    adapter.Fill(tab);
                }
                if (tab.Rows.Count == 1)
                {
                    try
                    {
                        string box = tab.Rows[0][0].ToString();
                        box = box.ToLower().Replace("box(", "").Replace(")", "");
                        string[] xy = box.Split(',');

                        string[] c1 = xy[0].Split(' ');
                        string[] c2 = xy[1].Split(' ');
                        envelope = new Envelope(
                             gView.Framework.OGC.OGC.ToDouble(c1[0]),
                             gView.Framework.OGC.OGC.ToDouble(c1[1]),
                             gView.Framework.OGC.OGC.ToDouble(c2[0]),
                             gView.Framework.OGC.OGC.ToDouble(c2[1]));
                    }
                    catch { }
                }
            }

            return envelope;
        }

        protected object ExecuteFunction(string function)
        {
            using (DbConnection conn = this.ProviderFactory.CreateConnection())
            using (DbCommand command = this.ProviderFactory.CreateCommand())
            {
                conn.ConnectionString = this.ConnectionString;
                command.CommandText = function;
                command.Connection = conn;
                conn.Open();

                return command.ExecuteScalar();
            }
        }

        virtual public DbCommand SelectCommand(OgcSpatialFeatureclass fc, IQueryFilter filter, out string shapeFieldName)
        {
            shapeFieldName = String.Empty;

            filter.fieldPostfix = filter.fieldPrefix = "\"";
            if (filter.SubFields == "*")
            {
                filter.SubFields = "";

                foreach (IField field in fc.Fields.ToEnumerable())
                {
                    filter.AddField(field.name);
                }
                filter.AddField(fc.IDFieldName);
                filter.AddField(fc.ShapeFieldName);
            }
            else
            {
                filter.AddField(fc.IDFieldName);
            }

            string where = "";
            if (filter is ISpatialFilter)
            {
                ISpatialFilter sFilter = filter as ISpatialFilter;
                if (sFilter.Geometry is IEnvelope)
                {
                    where = fc.ShapeFieldName + " && " + gView.Framework.OGC.OGC.Envelope2box2(sFilter.Geometry as IEnvelope, fc.SpatialReference);
                }
                else if (sFilter.Geometry != null)
                {
                    where = fc.ShapeFieldName + " && " + gView.Framework.OGC.OGC.Envelope2box2(sFilter.Geometry.Envelope, fc.SpatialReference);
                }
                filter.AddField(fc.ShapeFieldName);
            }
            string filterWhereClause = (filter is IRowIDFilter) ? ((IRowIDFilter)filter).RowIDWhereClause : filter.WhereClause;

            if (where != "" && filterWhereClause != "") where += " AND ";
            where += filterWhereClause;

            StringBuilder fieldNames = new StringBuilder();
            foreach (string fieldName in filter.SubFields.Split(' '))
            {
                if (fieldNames.Length > 0) fieldNames.Append(",");
                if (fieldName == "\"" + fc.ShapeFieldName + "\"")
                {
                    fieldNames.Append("ST_AsBinary(\"" + fc.ShapeFieldName + "\") as temp_geometry");
                    shapeFieldName = "temp_geometry";
                }
                else
                {
                    fieldNames.Append(fieldName);
                }
            }

            DbCommand command = ((OgcSpatialDataset)fc.Dataset).ProviderFactory.CreateCommand();
            command.CommandText = "SELECT " + fieldNames + " FROM " + DbTableName(fc.Name) + ((where != "") ? " WHERE " + where : "");

            return command;
        }
        virtual public string SelectReadSchema(string tableName)
        {
            return "select * from " + DbTableName(tableName);
        }
        virtual public DbCommand SelectSpatialReferenceIds(OgcSpatialFeatureclass fc)
        {
            return null;
        }
        virtual public string DbTableName(string tableName)
        {
            return tableName;
        }

        public enum EditCommands { Insert=0, Update=1, Delete=2 }
        virtual public bool CanEditFeatureClass(IFeatureClass fc, EditCommands command)
        {
            return true;
        }
    }
}
