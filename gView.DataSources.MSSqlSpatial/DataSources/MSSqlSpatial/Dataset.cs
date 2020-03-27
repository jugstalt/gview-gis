using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using gView.Framework.Data;
using gView.Framework.Geometry;
using System.Data;
using gView.Framework.FDB;
using gView.Framework.OGC.DB;
using gView.Framework.OGC;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial
{
    [gView.Framework.system.RegisterPlugIn("69A965B6-E7F6-4C67-A8F4-57AEDF9541C3")]
    public class GeometryDataset : gView.Framework.OGC.DB.OgcSpatialDataset, IFeatureImportEvents
    {
        protected DbProviderFactory _factory = null;
        protected IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        public GeometryDataset()
        {
            try
            {
                _factory = System.Data.SqlClient.SqlClientFactory.Instance;
            }
            catch
            {
                _factory = null;
            }
        }

        protected GeometryDataset(DbProviderFactory factory)
        {
            _factory = factory;
        }

        public override DbProviderFactory ProviderFactory
        {
            get { return _factory; }
        }

        protected override gView.Framework.OGC.DB.OgcSpatialDataset CreateInstance()
        {
            return new GeometryDataset(_factory);
        }

        public override string OgcDictionary(string ogcExpression)
        {
            switch (ogcExpression.ToLower())
            {
                case "gid":
                    return "GID";
                case "the_geom":
                    return "THE_GEOM";
                case "geometry_columns":
                case "geometry_columns.f_table_name":
                case "geometry_columns.f_geometry_column":
                case "geometry_columns.f_table_catalog":
                case "geometry_columns.f_table_schema":
                case "geometry_columns.coord_dimension":
                case "geometry_columns.srid":
                    return Field.shortName(ogcExpression).ToUpper();
                case "geometry_columns.type":
                    return "GEOMETRY_TYPE";
                case "gview_id":
                    return "gview_id";
            }
            return Field.shortName(ogcExpression);
        }

        public override string DbDictionary(IField field)
        {
            switch (field.type)
            {
                case FieldType.Shape:
                    return "[GEOMETRY]";
                case FieldType.ID:
                    return "[int] IDENTITY(1,1) NOT NULL CONSTRAINT KEY_" + System.Guid.NewGuid().ToString("N") + "_" + field.name + " PRIMARY KEY CLUSTERED";
                case FieldType.smallinteger:
                    return "[int] NULL";
                case FieldType.integer:
                    return "[int] NULL";
                case FieldType.biginteger:
                    return "[bigint] NULL";
                case FieldType.Float:
                    return "[float] NULL";
                case FieldType.Double:
                    return "[float] NULL";
                case FieldType.boolean:
                    return "[bit] NULL";
                case FieldType.character:
                    return "[nvarchar] (1) NULL";
                case FieldType.Date:
                    return "[datetime] NULL";
                case FieldType.String:
                    return "[nvarchar](" + field.size + ")";
                default:
                    return "[nvarchar] (255) NULL";
            }
        }

        protected override string DropGeometryTable(string schemaName, string tableName)
        {
            return "DROP TABLE " + tableName;
        }

        protected override object ShapeParameterValue(OgcSpatialFeatureclass fClass, gView.Framework.Geometry.IGeometry shape, int srid, out bool AsSqlParameter)
        {
            if (shape is IPolygon)
            {
                #region Check Polygon Rings

                IPolygon p = new Polygon();
                for (int i = 0; i < ((IPolygon)shape).RingCount; i++)
                {
                    IRing ring = ((IPolygon)shape)[i];
                    if (ring != null && ring.Area > 0D)
                    {
                        p.AddRing(ring);
                    }
                }

                if (p.RingCount == 0)
                {
                    AsSqlParameter = true;
                    return null;
                }
                shape = p;

                #endregion
            }
            else if (shape is IPolyline)
            {
                #region Check Polyline Paths

                IPolyline l = new Polyline();
                for (int i = 0; i < ((IPolyline)shape).PathCount; i++)
                {
                    IPath path = ((IPolyline)shape)[i];
                    if (path != null && path.Length > 0D)
                    {
                        l.AddPath(path);
                    }
                }

                if (l.PathCount == 0)
                {
                    AsSqlParameter = true;
                    return null;
                }
                shape = l;

                #endregion
            }

            AsSqlParameter = false;

            //return gView.Framework.OGC.OGC.GeometryToWKB(shape, gView.Framework.OGC.OGC.WkbByteOrder.Ndr);
            string geometryString =
                (shape is IPolygon) ?
                "geometry::STGeomFromText('" + gView.Framework.OGC.WKT.ToWKT(shape) + "'," + srid + ").MakeValid()" :
                "geometry::STGeomFromText('" + gView.Framework.OGC.WKT.ToWKT(shape) + "'," + srid + ")";
            return geometryString;
            //return "geometry::STGeomFromText('" + geometryString + "',0)";
        }

        async public override Task<IEnvelope> FeatureClassEnvelope(IFeatureClass fc)
        {
            IEnvelope env = null;
            try
            {
                if (fc != null && !String.IsNullOrEmpty(fc.ShapeFieldName))
                {
                    using (DbConnection connection = _factory.CreateConnection())
                    {
                        connection.ConnectionString = _connectionString;
                        DbCommand command = connection.CreateCommand();
                        command.CommandText = "select top(1000) " + ToDbName(fc.ShapeFieldName) + ".MakeValid().STEnvelope().STAsBinary() as envelope from " + ToDbName(fc.Name) + " where " + ToDbName(fc.ShapeFieldName) + " is not null";
                        await connection.OpenAsync();

                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                byte[] envelope = (byte[])reader["envelope"];
                                IGeometry geometry = gView.Framework.OGC.OGC.WKBToGeometry(envelope);
                                if (geometry == null) continue;

                                if (env == null)
                                    env = new Envelope(geometry.Envelope);
                                else
                                    env.Union(geometry.Envelope);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            if (env == null)
                return new Envelope(-1000, -1000, 1000, 1000);

            return env;
        }

        public override DbCommand SelectCommand(gView.Framework.OGC.DB.OgcSpatialFeatureclass fc, IQueryFilter filter, out string shapeFieldName, string functionName="", string functionField="", string functionAlias="")
        {
            shapeFieldName = String.Empty;

            DbCommand command = this.ProviderFactory.CreateCommand();

            filter.fieldPrefix = "[";
            filter.fieldPostfix = "]";

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

            string where = String.Empty;
            if (filter is ISpatialFilter && ((ISpatialFilter)filter).Geometry != null)
            {
                ISpatialFilter sFilter = filter as ISpatialFilter;

                int srid = 0;
                try
                {
                    if (fc.SpatialReference != null && fc.SpatialReference.Name.ToLower().StartsWith("epsg:"))
                    {
                        srid = Convert.ToInt32(fc.SpatialReference.Name.Split(':')[1]);
                    }
                }
                catch { }

                if (sFilter.SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects /*|| sFilter.Geometry is IEnvelope*/)
                {
                    IEnvelope env = sFilter.Geometry.Envelope;

                    where = fc.ShapeFieldName + ".Filter(";
                    where += "geometry::STGeomFromText('POLYGON((";
                    where += env.minx.ToString(_nhi) + " ";
                    where += env.miny.ToString(_nhi) + ",";

                    where += env.minx.ToString(_nhi) + " ";
                    where += env.maxy.ToString(_nhi) + ",";

                    where += env.maxx.ToString(_nhi) + " ";
                    where += env.maxy.ToString(_nhi) + ",";

                    where += env.maxx.ToString(_nhi) + " ";
                    where += env.miny.ToString(_nhi) + ",";

                    where += env.minx.ToString(_nhi) + " ";
                    where += env.miny.ToString(_nhi) + "))'," + srid + "))=1";
                }
                else if (sFilter.Geometry != null)
                {
                    IEnvelope env = sFilter.Geometry.Envelope;

                    where = fc.ShapeFieldName + ".STIntersects(";
                    where += "geometry::STGeomFromText('POLYGON((";
                    where += env.minx.ToString(_nhi) + " ";
                    where += env.miny.ToString(_nhi) + ",";

                    where += env.minx.ToString(_nhi) + " ";
                    where += env.maxy.ToString(_nhi) + ",";

                    where += env.maxx.ToString(_nhi) + " ";
                    where += env.maxy.ToString(_nhi) + ",";

                    where += env.maxx.ToString(_nhi) + " ";
                    where += env.miny.ToString(_nhi) + ",";

                    where += env.minx.ToString(_nhi) + " ";
                    where += env.miny.ToString(_nhi) + "))'," + srid + "))=1";
                }
                filter.AddField(fc.ShapeFieldName);
            }

            if (!String.IsNullOrWhiteSpace(functionName) && !String.IsNullOrWhiteSpace(functionField))
            {
                filter.SubFields = "";
                filter.AddField(functionName + "(" + filter.fieldPrefix + functionField + filter.fieldPostfix + ")");
            }

            string filterWhereClause = (filter is IRowIDFilter) ? ((IRowIDFilter)filter).RowIDWhereClause : filter.WhereClause;

            StringBuilder fieldNames = new StringBuilder();
            foreach (string fieldName in filter.SubFields.Split(' '))
            {
                if (fieldNames.Length > 0) fieldNames.Append(",");
                if (fieldName == "[" + fc.ShapeFieldName + "]")
                {
                    fieldNames.Append(fc.ShapeFieldName + ".STAsBinary() as temp_geometry");
                    shapeFieldName = "temp_geometry";
                }
                else
                {
                    fieldNames.Append(fieldName);
                }
            }

            string limit = String.Empty, top = String.Empty, orderBy = String.Empty;
            if (!String.IsNullOrWhiteSpace(filter.OrderBy))
            {
                orderBy = " order by " + filter.OrderBy;
            }

            if (filter.Limit > 0)
            {
                if (String.IsNullOrEmpty(fc.IDFieldName) && String.IsNullOrWhiteSpace(orderBy))
                {
                    top = "top(" + filter.Limit + ") ";
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(orderBy))
                    {
                        orderBy = " order by " + filter.fieldPrefix + fc.IDFieldName + filter.fieldPostfix;
                    }

                    limit = " offset " + Math.Max(0, filter.BeginRecord - 1) + " rows fetch next " + filter.Limit + " rows only";
                }
            }

            command.CommandText = "SELECT " + top + fieldNames + " FROM " + fc.Name;

            if (!String.IsNullOrEmpty(where))
            {
                command.CommandText += " WHERE " + where + ((filterWhereClause != "") ? $" AND ({ filterWhereClause })" : "");
            }
            else if (!String.IsNullOrEmpty(filterWhereClause))
            {
                command.CommandText += " WHERE " + filterWhereClause;
            }
            command.CommandText += orderBy + limit;

            return command;
        }

        protected override string AddGeometryColumn(string schemaName, string tableName, string colunName, string srid, string geomTypeString)
        {
            //return "CREATE SPATIAL INDEX SIndx_Postcodes_" + colunName + "_col1 ON " + tableName + "(" + colunName + ")";
            return String.Empty;
        }

        protected string IDFieldName(string tabName)
        {
            try
            {
                string schemaName = String.Empty;
                if (tabName.Contains("."))
                {
                    schemaName = tabName.Split('.')[0];
                    tabName = tabName.Split('.')[1];
                }

                string idFieldName = String.Empty;

                using (DbConnection conn = this.ProviderFactory.CreateConnection())
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    DbCommand command = this.ProviderFactory.CreateCommand();

                    command.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1 AND TABLE_NAME = '" + tabName + "'";
                    if (!String.IsNullOrWhiteSpace(schemaName))
                    {
                        command.CommandText += " AND TABLE_SCHEMA = '" + schemaName + "'";
                    }
                    command.Connection = conn;

                    try
                    {
                        idFieldName = command.ExecuteScalar() as string;
                    }
                    catch { }

                    if (String.IsNullOrWhiteSpace(idFieldName))
                    {
                        command.CommandText = "select c.name from sys.tables t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where t.name='" + tabName + "' and c.is_identity = 1";
                        command.Connection = conn;

                        idFieldName = command.ExecuteScalar() as string;
                    }
                    return idFieldName;
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return String.Empty;
            }
        }

        #region IDataset

        async public override Task<List<IDatasetElement>> Elements()
        {
            if (_layers == null || _layers.Count == 0)
            {
                List<IDatasetElement> layers = new List<IDatasetElement>();
                DataTable tables = new DataTable(), views = new DataTable();
                try
                {
                    using (DbConnection conn = this.ProviderFactory.CreateConnection())
                    {
                        conn.ConnectionString = _connectionString;
                        await conn.OpenAsync();

                        DbDataAdapter adapter = this.ProviderFactory.CreateDataAdapter();
                        adapter.SelectCommand = this.ProviderFactory.CreateCommand();
                        adapter.SelectCommand.CommandText = @"select t.name as tabName, c.name as colName, types.name from sys.tables t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where types.name = 'geometry'";
                        adapter.SelectCommand.Connection = conn;
                        adapter.Fill(tables);

                        adapter.SelectCommand.CommandText = @"select t.name as tabName, c.name as colName, types.name from sys.views t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where types.name = 'geometry'";
                        adapter.Fill(views);

                        conn.Close();
                    }

                }
                catch (Exception ex)
                {
                    _errMsg = ex.Message;
                    return layers;
                }

                foreach (DataRow row in tables.Rows)
                {
                    try
                    {
                        IFeatureClass fc = await Featureclass.Create(this,
                            row["tabName"].ToString(),
                            IDFieldName(row["tabName"].ToString()),
                            row["colName"].ToString(), false);

                        if (fc.Fields.Count > 0)
                            layers.Add(new DatasetElement(fc));
                    }
                    catch { }
                }
                foreach (DataRow row in views.Rows)
                {
                    try
                    {
                        IFeatureClass fc = await Featureclass.Create(this,
                            row["tabName"].ToString(),
                            IDFieldName(row["tabName"].ToString()),
                            row["colName"].ToString(), true);

                        if (fc.Fields.Count > 0)
                            layers.Add(new DatasetElement(fc));
                    }
                    catch { }
                }

                _layers = layers;
            }
            return _layers;
        }

        async public override Task<IDatasetElement> Element(string title)
        {
            DataTable tables = new DataTable(), views = new DataTable();

            try
            {
                using (DbConnection conn = this.ProviderFactory.CreateConnection())
                {
                    conn.ConnectionString = _connectionString;
                    await conn.OpenAsync();

                    DbDataAdapter adapter = this.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = this.ProviderFactory.CreateCommand();
                    adapter.SelectCommand.CommandText = @"select t.name as tabName, c.name as colName, types.name from sys.tables t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where types.name = 'geometry'";
                    adapter.SelectCommand.Connection = conn;
                    adapter.Fill(tables);

                    adapter.SelectCommand.CommandText = @"select t.name as tabName, c.name as colName, types.name from sys.views t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where types.name = 'geometry'";
                    adapter.Fill(views);

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return null;
            }

            foreach (DataRow row in tables.Rows)
            {
                string tableName = row["tabName"].ToString();
                if (await EqualsTableName(tableName, title, false))
                    return new DatasetElement(await Featureclass.Create(this,
                        tableName,
                        IDFieldName(title),
                        row["colName"].ToString(), false));
            }

            foreach (DataRow row in views.Rows)
            {
                string tableName = row["tabName"].ToString();
                if (await EqualsTableName(tableName, title, true))
                    return new DatasetElement(await Featureclass.Create(this,
                        tableName,
                        IDFieldName(title),
                        row["colName"].ToString(), true));
            }

            return null;
        }
        #endregion

        public override DbCommand SelectSpatialReferenceIds(gView.Framework.OGC.DB.OgcSpatialFeatureclass fc)
        {
            //string cmdText = "select distinct [" + fc.ShapeFieldName + "].STSrid as srid from " + fc.Name + " where [" + fc.ShapeFieldName + "] is not null";
            string cmdText = "select top(100) " + ToDbName(fc.ShapeFieldName) + ".STSrid as srid from " + ToDbName(fc.Name) + " where " + ToDbName(fc.ShapeFieldName) + " is not null";
            DbCommand command = this.ProviderFactory.CreateCommand();
            command.CommandText = cmdText;

            return command;
        }

        private Dictionary<string, string> _tableSchemas = new Dictionary<string, string>();
        private Dictionary<string, string> _viewSchemas = new Dictionary<string, string>();

        private string TableNamePlusSchemaFromCache(string tableName, bool isView)
        {
            if (isView)
            {
                if (_viewSchemas.ContainsKey(tableName.ToLower()))
                    return _viewSchemas[tableName.ToLower()] + "." + tableName;
            }
            else
            {
                if (_tableSchemas.ContainsKey(tableName.ToLower()))
                    return _tableSchemas[tableName.ToLower()] + "." + tableName;
            }

            return null;
        }

        async internal Task<string> TableNamePlusSchema(string tableName, bool isView)
        {
            if (tableName.Contains("."))
                return tableName;

            string ret = TableNamePlusSchemaFromCache(tableName, isView);
            if (ret != null)
                return ret;

            try
            {
                using (DbConnection conn = this.ProviderFactory.CreateConnection())
                {
                    conn.ConnectionString = _connectionString;
                    await conn.OpenAsync();

                    var command = this.ProviderFactory.CreateCommand();
                    command.Connection = conn;
                    string sysTable = isView ? "sys.views" : "sys.tables";
                    //command.CommandText = "select SCHEMA_NAME(schema_id) from " + sysTable + " where name='" + tableName + "'";
                    //string schema = command.ExecuteScalar()?.ToString();

                    command.CommandText = "select SCHEMA_NAME(schema_id) as dbschema, name as dbname from " + sysTable;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (isView)
                            {
                                _viewSchemas[reader["dbname"]?.ToString().ToLower()] = reader["dbschema"]?.ToString();
                            }
                            else
                            {
                                _tableSchemas[reader["dbname"]?.ToString().ToLower()] = reader["dbschema"]?.ToString();
                            }
                        }
                    }
                }

                ret = TableNamePlusSchemaFromCache(tableName, isView);
            }
            catch { }

            return ret ?? tableName;
        }

        private string ToDbName(string name)
        {
            if (!name.StartsWith("["))
                return "[" + name.Replace(".", "].[") + "]";

            return name;
        }

        async protected Task<bool> EqualsTableName(string tableName, string title, bool isView)
        {
            if (tableName.ToLower() == title.ToLower())
                return true;

            tableName = await TableNamePlusSchema(tableName, isView);
            if (tableName.ToLower() == title.ToLower())
                return true;

            return false;
        }

        #region IFeatureImportEvents Member

        virtual public void BeforeInsertFeaturesEvent(IFeatureClass sourceFc, IFeatureClass destFc)
        {
            if (sourceFc == null || destFc == null) return;
            try
            {
                Envelope env = new Envelope(sourceFc.Envelope);
                env.Raise(150.0);

                if (env == null) return;

                using (DbConnection conn = this.ProviderFactory.CreateConnection())
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    DbCommand command = this.ProviderFactory.CreateCommand();
                    command.CommandText = "CREATE SPATIAL INDEX SI_" + destFc.Name;
                    command.CommandText += " ON " + destFc.Name + "(" + destFc.ShapeFieldName + ")";
                    command.CommandText += " USING GEOMETRY_GRID WITH (";
                    command.CommandText += "BOUNDING_BOX = (";
                    command.CommandText += "xmin=" + env.minx.ToString(_nhi) + ",";
                    command.CommandText += "ymin=" + env.miny.ToString(_nhi) + ",";
                    command.CommandText += "xmax=" + env.maxx.ToString(_nhi) + ",";
                    command.CommandText += "ymax=" + env.maxy.ToString(_nhi) + ")";
                    command.CommandText += ",GRIDS = (LEVEL_1 = LOW, LEVEL_2 = LOW, LEVEL_3 = LOW, LEVEL_4 = LOW)";
                    command.CommandText += ",CELLS_PER_OBJECT = 256";
                    command.CommandText += ")";
                    command.Connection = conn;

                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
            }
        }

        public void AfterInsertFeaturesEvent(IFeatureClass sourceFc, IFeatureClass destFc)
        {
            try
            {
                using (DbConnection conn = this.ProviderFactory.CreateConnection())
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    DbCommand command = this.ProviderFactory.CreateCommand();
                    command.CommandText = "UPDATE " + destFc.Name + " SET " + destFc.ShapeFieldName + " = " + destFc.ShapeFieldName + ".MakeValid() WHERE " + destFc.ShapeFieldName + ".STIsValid() = 0";
                    command.Connection = conn;

                    command.ExecuteNonQuery();

                    if (sourceFc.SpatialReference != null && sourceFc.SpatialReference.Name.ToLower().StartsWith("epsg:"))
                    {
                        int srid = 0;
                        int.TryParse(sourceFc.SpatialReference.Name.Split(':')[1],out srid);

                        if (srid > 0)
                        {
                            command = this.ProviderFactory.CreateCommand();
                            command.CommandText = "UPDATE " + destFc.Name + " SET " + destFc.ShapeFieldName + ".STSrid=" + srid;
                            command.Connection = conn;

                            command.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
            }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("6EB3070C-377A-4B1B-8479-A0ADA92D8D69")]
    public class GeographyDataset : GeometryDataset
    {
        public GeographyDataset()
            : base()
        {
        }

        protected GeographyDataset(DbProviderFactory factory)
            : base(factory)
        {
        }

        protected override gView.Framework.OGC.DB.OgcSpatialDataset CreateInstance()
        {
            return new GeographyDataset(_factory);
        }

        public override string DbDictionary(IField field)
        {
            switch (field.type)
            {
                case FieldType.Shape:
                    return "[GEOGRAPHY]";
                case FieldType.ID:
                    return "[int] IDENTITY(1,1) NOT NULL CONSTRAINT KEY_" + System.Guid.NewGuid().ToString("N") + "_" + field.name + " PRIMARY KEY CLUSTERED";
                case FieldType.smallinteger:
                    return "[int] NULL";
                case FieldType.integer:
                    return "[int] NULL";
                case FieldType.biginteger:
                    return "[bigint] NULL";
                case FieldType.Float:
                    return "[float] NULL";
                case FieldType.Double:
                    return "[float] NULL";
                case FieldType.boolean:
                    return "[bit] NULL";
                case FieldType.character:
                    return "[nvarchar] (1) NULL";
                case FieldType.Date:
                    return "[datetime] NULL";
                case FieldType.String:
                    return "[nvarchar](" + field.size + ")";
                default:
                    return "[nvarchar] (255) NULL";
            }
        }

        protected override object ShapeParameterValue(OgcSpatialFeatureclass fClass, gView.Framework.Geometry.IGeometry shape, int srid, out bool AsSqlParameter)
        {
            if (shape is IPolygon)
            {
                #region Check Polygon Rings
                IPolygon p = new Polygon();
                for (int i = 0; i < ((IPolygon)shape).RingCount; i++)
                {
                    IRing ring = ((IPolygon)shape)[i];
                    if (ring != null && ring.Area > 0D)
                    {
                        p.AddRing(ring);
                    }
                }

                if (p.RingCount == 0)
                {
                    AsSqlParameter = true;
                    return null;
                }
                shape = p;
                #endregion
            }
            else if (shape is IPolyline)
            {
                #region Check Polyline Paths
                IPolyline l = new Polyline();
                for (int i = 0; i < ((IPolyline)shape).PathCount; i++)
                {
                    IPath path = ((IPolyline)shape)[i];
                    if (path != null && path.Length > 0D)
                    {
                        l.AddPath(path);
                    }
                }

                if (l.PathCount == 0)
                {
                    AsSqlParameter = true;
                    return null;
                }
                shape = l;
                #endregion
            }

            AsSqlParameter = false;

            //return gView.Framework.OGC.OGC.GeometryToWKB(shape, gView.Framework.OGC.OGC.WkbByteOrder.Ndr);
            string geometryString =
                (shape is IPolygon) ?
                "geometry::STGeomFromText('" + gView.Framework.OGC.WKT.ToWKT(shape) + "'," + srid + ").MakeValid()" :
                "geometry::STGeomFromText('" + gView.Framework.OGC.WKT.ToWKT(shape) + "'," + srid + ")";
            return geometryString;
            //return "geometry::STGeomFromText('" + geometryString + "',0)";

            // Old
            //AsSqlParameter = true;

            //string geometryString = gView.Framework.OGC.WKT.ToWKT(shape);
            //return geometryString;
        }

        public override Task<IEnvelope> FeatureClassEnvelope(IFeatureClass fc)
        {
            return Task.FromResult<IEnvelope>(new Envelope(-180, -90, 180, 90));
        }

        public override DbCommand SelectCommand(gView.Framework.OGC.DB.OgcSpatialFeatureclass fc, IQueryFilter filter, out string shapeFieldName, string functionName = "", string functionField = "", string functionAlias = "")
        {
            shapeFieldName = String.Empty;

            DbCommand command = this.ProviderFactory.CreateCommand();

            filter.fieldPrefix = "[";
            filter.fieldPostfix = "]";

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

            string where = String.Empty;
            if (filter is ISpatialFilter && ((ISpatialFilter)filter).Geometry != null)
            {
                ISpatialFilter sFilter = filter as ISpatialFilter;


                if (sFilter.SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects /*|| sFilter.Geometry is IEnvelope*/)
                {
                    IEnvelope env = sFilter.Geometry.Envelope;

                    where = fc.ShapeFieldName + ".Filter(";
                    where += "geography::STGeomFromText('POLYGON((";
                    where += Math.Max(-179.99, env.minx).ToString(_nhi) + " ";
                    where += Math.Max(-89.99, env.miny).ToString(_nhi) + ",";

                    where += Math.Min(179.99, env.maxx).ToString(_nhi) + " ";
                    where += Math.Max(-89.99, env.miny).ToString(_nhi) + ",";

                    where += Math.Min(179.99, env.maxx).ToString(_nhi) + " ";
                    where += Math.Min(89.99, env.maxy).ToString(_nhi) + ",";

                    where += Math.Max(-179.99, env.minx).ToString(_nhi) + " ";
                    where += Math.Min(89.99, env.maxy).ToString(_nhi) + ",";

                    where += Math.Max(-179.99, env.minx).ToString(_nhi) + " ";
                    where += Math.Max(-89.99, env.miny).ToString(_nhi) + "))',4326))=1";
                }
                else if (sFilter.Geometry != null)
                {
                    IEnvelope env = sFilter.Geometry.Envelope;

                    where = fc.ShapeFieldName + ".STIntersects(";
                    where += "geography::STGeomFromText('POLYGON((";
                    where += Math.Max(-180.0, env.minx).ToString(_nhi) + " ";
                    where += Math.Max(-89.99, env.miny).ToString(_nhi) + ",";

                    where += Math.Max(-180.0, env.minx).ToString(_nhi) + " ";
                    where += Math.Min(89.99, env.maxy).ToString(_nhi) + ",";

                    where += Math.Min(180.0, env.maxx).ToString(_nhi) + " ";
                    where += Math.Min(89.99, env.maxy).ToString(_nhi) + ",";

                    where += Math.Min(180.0, env.maxx).ToString(_nhi) + " ";
                    where += Math.Max(-89.99, env.miny).ToString(_nhi) + ",";

                    where += Math.Max(-180.0, env.minx).ToString(_nhi) + " ";
                    where += Math.Max(-89.99, env.miny).ToString(_nhi) + "))',4326))=1";
                }
                filter.AddField(fc.ShapeFieldName);
            }

            if (!String.IsNullOrWhiteSpace(functionName) && !String.IsNullOrWhiteSpace(functionField))
            {
                filter.SubFields = "";
                filter.AddField(functionName + "(" + filter.fieldPrefix + functionField + filter.fieldPostfix + ")");
            }

            string filterWhereClause = (filter is IRowIDFilter) ? ((IRowIDFilter)filter).RowIDWhereClause : filter.WhereClause;

            StringBuilder fieldNames = new StringBuilder();
            foreach (string fieldName in filter.SubFields.Split(' '))
            {
                if (fieldNames.Length > 0) fieldNames.Append(",");
                if (fieldName == "[" + fc.ShapeFieldName + "]")
                {
                    fieldNames.Append(fc.ShapeFieldName + ".STAsBinary() as temp_geometry");
                    shapeFieldName = "temp_geometry";
                }
                else
                {
                    fieldNames.Append(fieldName);
                }
            }

            string limit = String.Empty, top = String.Empty, orderBy = String.Empty;
            if (!String.IsNullOrWhiteSpace(filter.OrderBy))
            {
                orderBy = " order by " + filter.OrderBy;
            }

            if (filter.Limit > 0)
            {
                if (String.IsNullOrEmpty(fc.IDFieldName) && String.IsNullOrWhiteSpace(orderBy))
                {
                    top = "top(" + filter.Limit + ") ";
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(orderBy))
                    {
                        orderBy = " order by " + filter.fieldPrefix + fc.IDFieldName + filter.fieldPostfix;
                    }

                    limit = " offset " + Math.Max(0, filter.BeginRecord - 1) + " rows fetch next " + filter.Limit + " rows only";
                }
            }

            command.CommandText = "SELECT " + limit + fieldNames + " FROM " + fc.Name;

            if (!String.IsNullOrEmpty(where))
            {
                command.CommandText += " WHERE " + where + ((filterWhereClause != "") ? $" AND ({ filterWhereClause })" : "");
            }
            else if (!String.IsNullOrEmpty(filterWhereClause))
            {
                command.CommandText += " WHERE " + filterWhereClause;
            }

            command.CommandText += orderBy + limit;

            return command;
        }

        #region IDataset
        async public override Task<List<IDatasetElement>> Elements()
        {
            if (_layers == null || _layers.Count == 0)
            {
                List<IDatasetElement> layers = new List<IDatasetElement>();
                DataTable tables = new DataTable(), views = new DataTable();
                try
                {
                    using (DbConnection conn = this.ProviderFactory.CreateConnection())
                    {
                        conn.ConnectionString = _connectionString;
                        await conn.OpenAsync();

                        DbDataAdapter adapter = this.ProviderFactory.CreateDataAdapter();
                        adapter.SelectCommand = this.ProviderFactory.CreateCommand();
                        adapter.SelectCommand.CommandText = @"select t.name as tabName, c.name as colName, types.name from sys.tables t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where types.name = 'geography'";
                        adapter.SelectCommand.Connection = conn;
                        adapter.Fill(tables);

                        adapter.SelectCommand.CommandText = @"select t.name as tabName, c.name as colName, types.name from sys.views t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where types.name = 'geography'";
                        adapter.Fill(views);

                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    _errMsg = ex.Message;
                    return layers;
                }

                foreach (DataRow row in tables.Rows)
                {
                    IFeatureClass fc = await Featureclass.Create(this,
                        row["tabName"].ToString(),
                        IDFieldName(row["tabName"].ToString()),
                        row["colName"].ToString(), false);
                    layers.Add(new DatasetElement(fc));
                }
                foreach (DataRow row in views.Rows)
                {
                    IFeatureClass fc = await Featureclass.Create(this,
                        row["tabName"].ToString(),
                        IDFieldName(row["tabName"].ToString()),
                        row["colName"].ToString(), true);
                    layers.Add(new DatasetElement(fc));
                }

                _layers = layers;
            }
            return _layers;
        }

        async public override Task<IDatasetElement> Element(string title)
        {
            DataTable tables = new DataTable(), views = new DataTable();

            try
            {
                using (DbConnection conn = this.ProviderFactory.CreateConnection())
                {
                    conn.ConnectionString = _connectionString;
                    await conn.OpenAsync();

                    DbDataAdapter adapter = this.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = this.ProviderFactory.CreateCommand();
                    adapter.SelectCommand.CommandText = @"select t.name as tabName, c.name as colName, types.name from sys.tables t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where types.name = 'geography'";
                    adapter.SelectCommand.Connection = conn;
                    adapter.Fill(tables);

                    adapter.SelectCommand.CommandText = @"select t.name as tabName, c.name as colName, types.name from sys.views t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where types.name = 'geography'";
                    adapter.Fill(views);

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return null;
            }

            foreach (DataRow row in tables.Rows)
            {
                string tableName = row["tabName"].ToString();
                if (await EqualsTableName(tableName, title, false))
                    return new DatasetElement(await Featureclass.Create(this,
                        tableName,
                        IDFieldName(title),
                        row["colName"].ToString(), false));
            }
            foreach (DataRow row in views.Rows)
            {
                string tableName = row["tabName"].ToString();
                if (await EqualsTableName(tableName, title, true))
                    return new DatasetElement(await Featureclass.Create(this,
                        tableName,
                        IDFieldName(title),
                        row["colName"].ToString(), true));
            }

            return null;
        }

        #endregion

        #region IFeatureImportEvents Member

        override public void BeforeInsertFeaturesEvent(IFeatureClass sourceFc, IFeatureClass destFc)
        {
            if (sourceFc == null || destFc == null) return;
            try
            {
                Envelope env = new Envelope(sourceFc.Envelope);
                env.Raise(1.5);

                if (env == null) return;

                using (DbConnection conn = this.ProviderFactory.CreateConnection())
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    DbCommand command = this.ProviderFactory.CreateCommand();
                    command.CommandText = "CREATE SPATIAL INDEX SI_" + destFc.Name;
                    command.CommandText += " ON " + destFc.Name + "(" + destFc.ShapeFieldName + ")";
                    command.CommandText += " USING GEOGRAPHY_GRID WITH (";
                    command.CommandText += "GRIDS = (LEVEL_1 = LOW, LEVEL_2 = LOW, LEVEL_3 = LOW, LEVEL_4 = LOW)";
                    command.CommandText += ",CELLS_PER_OBJECT = 256";
                    //command.CommandText += ",DROP_EXISTING=ON";
                    command.CommandText += ")";
                    command.Connection = conn;

                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
            }
        }

        #endregion
    }

    public class Featureclass : gView.Framework.OGC.DB.OgcSpatialFeatureclass
    {
        private Featureclass(GeometryDataset dataset, string name, string idFieldName, string shapeFieldName, bool isView)
        {
            
        }

        async static public Task<IFeatureClass> Create(GeometryDataset dataset, string name, string idFieldName, string shapeFieldName, bool isView)
        {
            var featureClass = new Featureclass(dataset, name, idFieldName, shapeFieldName, isView);

            featureClass._name =await dataset.TableNamePlusSchema(name, isView);
            featureClass._idfield = idFieldName;
            featureClass._shapefield = shapeFieldName;
            featureClass._geomType = geometryType.Unknown;

            featureClass._dataset = dataset;
            if (featureClass._dataset is GeographyDataset)
                featureClass._sRef = gView.Framework.Geometry.SpatialReference.FromID("epsg:4326");

            await featureClass.ReadSchema();

            if (String.IsNullOrEmpty(featureClass._idfield) && featureClass._fields.Count > 0 && featureClass._dataset != null)
            {
                Field field = featureClass._fields[0] as Field;
                if (field != null)
                {
                    if ((field.type == FieldType.integer || field.type == FieldType.biginteger || field.type == FieldType.ID)
                        && field.name.ToLower() == featureClass._dataset.OgcDictionary("gview_id").ToLower())
                        featureClass._idfield = field.name;
                    ((Field)field).type = FieldType.ID;
                }
            }

            //base._geomType = geometryType.Polygon;

            if (featureClass._sRef == null)
                featureClass._sRef = await gView.Framework.OGC.DB.OgcSpatialFeatureclass.TrySelectSpatialReference(dataset, featureClass);

            return featureClass;
        }

        public override ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
        }
    }
}
