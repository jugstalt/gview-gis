using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.OGC.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial
{
    [gView.Framework.system.RegisterPlugIn("6EB3070C-377A-4B1B-8479-A0ADA92D8D69")]
    [ImportFeatureClassNameWithSchema]
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
                "geography::STGeomFromText('" + gView.Framework.OGC.WKT.ToWKT(shape) + "'," + srid + ").MakeValid()" :
                "geography::STGeomFromText('" + gView.Framework.OGC.WKT.ToWKT(shape) + "'," + srid + ")";
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
                if (fieldNames.Length > 0)
                {
                    fieldNames.Append(",");
                }

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
                command.CommandText += " WHERE " + where + ((filterWhereClause != "") ? $" AND ({filterWhereClause})" : "");
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
                    using (DbConnection dbConnection = this.ProviderFactory.CreateConnection())
                    {
                        dbConnection.ConnectionString = _connectionString;
                        await dbConnection.OpenAsync();

                        DbDataAdapter adapter = this.ProviderFactory.CreateDataAdapter();
                        adapter.SelectCommand = this.ProviderFactory.CreateCommand();
                        adapter.SelectCommand.CommandText = @"select SCHEMA_NAME(t.schema_id) as dbSchema, t.name as tabName, c.name as colName, types.name from sys.tables t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where types.name = 'geography'";
                        adapter.SelectCommand.Connection = dbConnection;
                        adapter.Fill(tables);

                        adapter.SelectCommand.CommandText = @"select SCHEMA_NAME(t.schema_id) as dbSchema, t.name as tabName, c.name as colName, types.name from sys.views t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where types.name = 'geography'";
                        adapter.Fill(views);

                        foreach (DataRow row in tables.Rows)
                        {
                            var fcShema = row["dbSchema"].ToString();
                            var fcName = String.IsNullOrEmpty(fcShema) ? row["tabName"].ToString() : $"{fcShema}.{row["tabName"].ToString()}";

                            IFeatureClass fc = await Featureclass.Create(dbConnection, this,
                                fcName,
                                IDFieldName(dbConnection, fcName),
                                row["colName"].ToString(), false);
                            layers.Add(new DatasetElement(fc));
                        }
                        foreach (DataRow row in views.Rows)
                        {
                            var fcShema = row["dbSchema"].ToString();
                            var fcName = String.IsNullOrEmpty(fcShema) ? row["tabName"].ToString() : $"{fcShema}.{row["tabName"].ToString()}";

                            IFeatureClass fc = await Featureclass.Create(dbConnection, this,
                                fcName,
                                IDFieldName(dbConnection, fcName),
                                row["colName"].ToString(), true);
                            layers.Add(new DatasetElement(fc));
                        }

                        dbConnection.Close();
                    }

                    _layers = layers;
                }
                catch (Exception ex)
                {
                    _errMsg = ex.Message;
                    return layers;
                }
            }
            return _layers;
        }

        async public override Task<IDatasetElement> Element(string title)
        {
            DataTable tables = new DataTable(), views = new DataTable();

            try
            {
                using (DbConnection dbConnection = this.ProviderFactory.CreateConnection())
                {
                    dbConnection.ConnectionString = _connectionString;
                    await dbConnection.OpenAsync();

                    DbDataAdapter adapter = this.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = this.ProviderFactory.CreateCommand();
                    adapter.SelectCommand.CommandText = @"select SCHEMA_NAME(t.schema_id) as dbSchema, t.name as tabName, c.name as colName, types.name from sys.tables t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where types.name = 'geography'";
                    adapter.SelectCommand.Connection = dbConnection;
                    adapter.Fill(tables);

                    adapter.SelectCommand.CommandText = @"select SCHEMA_NAME(t.schema_id) as dbSchema, t.name as tabName, c.name as colName, types.name from sys.views t join sys.columns c on (t.object_id = c.object_id) join sys.types types on (c.user_type_id = types.user_type_id) where types.name = 'geography'";
                    adapter.Fill(views);

                    foreach (DataRow row in tables.Rows)
                    {
                        var fcShema = row["dbSchema"].ToString();
                        var tableName = row["tabName"].ToString();
                        var fcName = title.Contains(".") ? $"{fcShema}.{tableName}" : tableName;

                        if (await EqualsTableName(fcName, title, false))
                        {
                            return new DatasetElement(await Featureclass.Create(dbConnection, this,
                                fcName,
                                IDFieldName(dbConnection, title),
                                row["colName"].ToString(), false));
                        }
                    }
                    foreach (DataRow row in views.Rows)
                    {
                        var fcShema = row["dbSchema"].ToString();
                        var tableName = row["tabName"].ToString();
                        var fcName = title.Contains(".") ? $"{fcShema}.{tableName}" : tableName;

                        if (await EqualsTableName(fcName, title, true))
                        {
                            return new DatasetElement(await Featureclass.Create(dbConnection, this,
                                fcName,
                                IDFieldName(dbConnection, title),
                                row["colName"].ToString(), true));
                        }
                    }

                    dbConnection.Close();
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return null;
            }
            return null;
        }

        #endregion

        #region IFeatureImportEvents Member

        override public void BeforeInsertFeaturesEvent(IFeatureClass sourceFc, IFeatureClass destFc)
        {
            if (sourceFc == null || destFc == null)
            {
                return;
            }

            try
            {
                Envelope env = new Envelope(sourceFc.Envelope);
                env.Raise(1.5);

                if (env == null)
                {
                    return;
                }

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
}
