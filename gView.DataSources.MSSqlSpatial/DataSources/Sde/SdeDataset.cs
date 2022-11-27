using gView.DataSources.MSSqlSpatial.DataSources.Sde.Repo;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.OGC;
using gView.Framework.OGC.DB;
using gView.Framework.SpatialAlgorithms;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde
{
    [gView.Framework.system.RegisterPlugIn("F7394B37-1397-4914-B1D0-5A03B11D2949")]
    public class SdeDataset : gView.Framework.OGC.DB.OgcSpatialDataset
    {
        protected DbProviderFactory _factory = null;
        protected IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        private static string[] SupportedFunctions = new string[] { "STArea()", "STLength()" };

        internal RepoProvider RepoProvider = null;

        public SdeDataset()
            : base()
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

        protected SdeDataset(DbProviderFactory factory)
            : base()
        {
            _factory = factory;
        }

        protected override gView.Framework.OGC.DB.OgcSpatialDataset CreateInstance()
        {
            return new SdeDataset(_factory);
        }

        public override DbProviderFactory ProviderFactory
        {
            get { return _factory; }
        }

        async public override Task<bool> Open()
        {
            var repo = new RepoProvider();
            await repo.Init(_connectionString);
            RepoProvider = repo;

            return await base.Open();
        }

        public override string OgcDictionary(string ogcExpression)
        {
            switch (ogcExpression.ToLower())
            {
                case "gid":
                    return "OBJECTID";
                case "the_geom":
                    return "SHAPE";
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
                (shape is IPolygon || shape is IPolyline) ?
                "geometry::STGeomFromText('" + gView.Framework.OGC.WKT.ToWKT(shape) + "'," + srid + ").MakeValid()" :
                "geometry::STGeomFromText('" + gView.Framework.OGC.WKT.ToWKT(shape) + "'," + srid + ")";
            return geometryString;
            //return "geometry::STGeomFromText('" + geometryString + "',0)";
        }

        protected override IGeometry ValidateGeometry(IFeatureClass fc, IGeometry geometry)
        {
            return geometry.MakeValid(fc.SpatialReference?.MakeValidTolerance ?? 1e-8,
                                      geometryType: fc.GeometryType);
        }

        public override DbCommand SelectCommand(gView.Framework.OGC.DB.OgcSpatialFeatureclass fc, IQueryFilter filter, out string shapeFieldName, string functionName = "", string functionField = "", string functionAlias = "")
        {
            shapeFieldName = String.Empty;

            DbCommand command = this.ProviderFactory.CreateCommand();
            var sqlCommand = new StringBuilder();

            filter.fieldPrefix = "[";
            filter.fieldPostfix = "]";

            if (filter.QuerySubFields.Contains("*"))
            {
                // var subFieldFunctions = filter.QuerySubFields.Where(f => f.IsSubFieldFunction());
                filter.SubFields = "";

                foreach (IField field in fc.Fields.ToEnumerable())
                {
                    filter.AddField(field.name);
                }
                filter.AddField(fc.IDFieldName);
                filter.AddField(fc.ShapeFieldName);

                //foreach(var subFieldFunction in subFieldFunctions)
                //{
                //    filter.AddField(subFieldFunction);
                //}
            }
            else
            {
                filter.AddField(fc.IDFieldName);
            }

            var where = new StringBuilder();
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
                    where.Append($"{fc.ShapeFieldName}.STIntersects(");
                    where.Append($"geometry::STGeomFromText('{WKT.ToWKT(sFilter.Geometry.Envelope)}',{srid}))=1");
                }
                else if (sFilter.Geometry != null)
                {
                    // 
                    // let sqlserver do the hole intersection => make geometry to WKT not only the envelope
                    // otherwise if there is an limit set (1000) not all features will returned
                    //
                    where.Append($"{fc.ShapeFieldName}.STIntersects(");
                    where.Append($"geometry::STGeomFromText('{WKT.ToWKT(sFilter.Geometry)}',{srid}))=1");
                }
                filter.AddField(fc.ShapeFieldName);
            }

            if (!String.IsNullOrWhiteSpace(functionName) && !String.IsNullOrWhiteSpace(functionField))
            {
                filter.SubFields = "";
                filter.AddField(functionName + "(" + filter.fieldPrefix + functionField + filter.fieldPostfix + ")");
            }

            string filterWhereClause = (filter is IRowIDFilter) ? ((IRowIDFilter)filter).RowIDWhereClause : filter.WhereClause;

            var fieldNames = new StringBuilder();
            foreach (string fieldName in filter.QuerySubFields)
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

            StringBuilder limit = new StringBuilder(),
                          top = new StringBuilder(),
                          orderBy = new StringBuilder();

            if (!String.IsNullOrWhiteSpace(filter.OrderBy))
            {
                orderBy.Append($" order by {filter.OrderBy}");
            }

            if (filter.Limit > 0)
            {
                if (String.IsNullOrEmpty(fc.IDFieldName) && orderBy.Length == 0)
                {
                    top.Append($"top({filter.Limit}) ");
                }
                else
                {
                    if (orderBy.Length == 0)
                    {
                        orderBy.Append($" order by {filter.fieldPrefix}{fc.IDFieldName}{filter.fieldPostfix}");
                    }

                    limit.Append($" offset {Math.Max(0, filter.BeginRecord - 1)} rows fetch next {filter.Limit} rows only");
                }
            }

            string fcName = fc.Name;
            if (fc is SdeFeatureClass && !String.IsNullOrWhiteSpace(((SdeFeatureClass)fc).MultiVersionedViewName))
            {
                fcName = ((SdeFeatureClass)fc).MultiVersionedViewName;

                // ToDo: filterWhereClause? SDE_STATE_ID=0?? 
            }

            sqlCommand.Append($"SELECT {top}{fieldNames} FROM {fcName}");

            if (where.Length > 0)
            {
                sqlCommand.Append($" WHERE {where.ToString()}");
                if (!String.IsNullOrEmpty(filterWhereClause))
                {
                    sqlCommand.Append($" AND ({filterWhereClause})");
                }
            }
            else if (!String.IsNullOrEmpty(filterWhereClause))
            {
                sqlCommand.Append($" WHERE {filterWhereClause}");
            }
            sqlCommand.Append(orderBy.ToString());
            sqlCommand.Append(limit.ToString());

            command.CommandText = sqlCommand.ToString();

            return command;
        }

        public override IEnumerable<string> SupportedSubFieldFunctions()
        {
            return SupportedFunctions;
        }

        async public override Task<IEnvelope> FeatureClassEnvelope(IFeatureClass fc)
        {
            if (RepoProvider == null)
            {
                throw new Exception("Repository not initialized");
            }

            return await RepoProvider.FeatureClassEnveolpe(fc);
        }

        async public override Task<List<IDatasetElement>> Elements()
        {
            if (RepoProvider == null)
            {
                throw new Exception("Repository not initialized");
            }

            if (_layers == null || _layers.Count == 0)
            {
                List<IDatasetElement> layers = new List<IDatasetElement>();

                foreach (var sdeLayer in RepoProvider.Layers)
                {
                    layers.Add(new DatasetElement(
                       await SdeFeatureClass.Create(this, sdeLayer.Owner + "." + sdeLayer.TableName,
                          String.IsNullOrWhiteSpace(sdeLayer.MultiVersionedViewName) ? null : sdeLayer.Owner + "." + sdeLayer.MultiVersionedViewName)));
                }

                _layers = layers;
            }
            return _layers;
        }

        async public override Task<IDatasetElement> Element(string title)
        {
            if (RepoProvider == null)
            {
                throw new Exception("Repository not initialized");
            }

            title = title.ToLower();
            var sdeLayer = RepoProvider.Layers.Where(l => (l.Owner + "." + l.TableName).ToLower() == title).FirstOrDefault();

            if (sdeLayer != null)
            {
                return new DatasetElement(await SdeFeatureClass.Create(this,
                    sdeLayer.Owner + "." + sdeLayer.TableName,
                    String.IsNullOrWhiteSpace(sdeLayer.MultiVersionedViewName) ? null : sdeLayer.Owner + "." + sdeLayer.MultiVersionedViewName));
            }

            return null;
        }

        public override bool HasManagedRowIds(ITableClass table)
        {
            return false;
        }
        async public override Task<int?> GetNextInsertRowId(ITableClass table)
        {
            if (RepoProvider == null)
            {
                throw new Exception("Repository not initialized");
            }

            var sdeLayer = RepoProvider.LayerFromTableClass(table);
            if (sdeLayer == null)
            {
                throw new Exception("Sde layer not found: " + table?.Name);
            }

            return await RepoProvider.GetInsertRowId(sdeLayer);
        }
    }
}
