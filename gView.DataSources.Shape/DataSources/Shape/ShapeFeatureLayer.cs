using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.DataSources.Shape.Lib;

namespace gView.DataSources.Shape
{
    /// <summary>
    /// 
    /// </summary>
    internal class ShapeDatasetElement : DatasetElement, IFeatureSelection
    {
        SHPFile _file = null;
        private ISelectionSet _selectionset;

        internal ShapeDatasetElement(SHPFile file, IDataset dataset, IIndexTree tree)
        {
            _file = file;
            this.Title = _file.Title;

            _class = new ShapeFeatureClass(_file, dataset, tree);
        }

        #region IFeatureSelection Members

        public event FeatureSelectionChangedEvent FeatureSelectionChanged;
        public event BeforeClearSelectionEvent BeforeClearSelection;

        public ISelectionSet SelectionSet
        {
            get
            {
                return (ISelectionSet)_selectionset;
            }
            set
            {
                if (_selectionset != null && _selectionset != value) _selectionset.Clear();

                _selectionset = value;
            }
        }

        public bool Select(IQueryFilter filter, CombinationMethod method)
        {
            if (!(this.Class is ITableClass)) return false;

            ISelectionSet selSet = ((ITableClass)this.Class).Select(filter);

            if (method == CombinationMethod.New || SelectionSet == null)
            {
                SelectionSet = selSet;
            }
            else
            {
                SelectionSet.Combine(selSet, method);
            }

            FireSelectionChangedEvent();

            return true;
        }

        public void ClearSelection()
        {
            if (_selectionset != null)
            {
                _selectionset.Clear();
                _selectionset = null;
                FireSelectionChangedEvent();
            }
        }

        public void FireSelectionChangedEvent()
        {
            if (FeatureSelectionChanged != null)
                FeatureSelectionChanged(this);
        }

        #endregion

        public bool Delete()
        {
            if (_file != null) return _file.Delete();
            return false;
        }

        public bool Rename(string newName)
        {
            if (_file != null) return _file.Rename(newName);
            return false;
        }
    }

    internal class ShapeFeatureClass : gView.Framework.Data.IFeatureClass, IDisposable
    {
        private GeometryDef _geomDef = new GeometryDef();
        private SHPFile _file = null;
        private IIndexTree _tree;
        internal IEnvelope _envelope = null;
        private IDataset _dataset;

        public ShapeFeatureClass(SHPFile file, IDataset dataset, IIndexTree tree)
        {
            if (file == null) return;
            _file = file;
            _dataset = dataset;
            _tree = tree;

            _envelope = new Envelope(_file.Header.Xmin, _file.Header.Ymin, _file.Header.Xmax, _file.Header.Ymax);

            ISpatialReference sRef = null;
            if (file.PRJ_Exists)
            {
                StreamReader sr = new StreamReader(file.PRJ_Filename);
                string esriWKT = sr.ReadToEnd();
                sRef = gView.Framework.Geometry.SpatialReference.FromWKT(esriWKT);
                sr.Close();
            }

            switch (_file.Header.ShapeType)
            {
                case ShapeType.Point:
                case ShapeType.PointM:
                case ShapeType.PointZ:
                case ShapeType.MultiPoint:
                case ShapeType.MultiPointM:
                case ShapeType.MultiPointZ:
                    _geomDef = new GeometryDef(geometryType.Point, sRef);
                    break;
                case ShapeType.PolyLine:
                case ShapeType.PolyLineM:
                case ShapeType.PolyLineZ:
                    _geomDef = new GeometryDef(geometryType.Polyline, sRef);
                    break;
                case ShapeType.Polygon:
                case ShapeType.PolygonM:
                case ShapeType.PolygonZ:
                    _geomDef = new GeometryDef(geometryType.Polygon, sRef);
                    break;
                case ShapeType.MultiPatch:
                    _geomDef = new GeometryDef(geometryType.Aggregate, sRef);
                    break;
                default:
                    _geomDef = new GeometryDef(geometryType.Unknown, sRef);
                    break;
            }
        }

        #region IFeatureClass Member

        public gView.Framework.Data.IFeatureCursor GetFeatures(gView.Framework.Data.IQueryFilter filter/*, gView.Framework.Data.getFeatureQueryType type*/)
        {
            if (filter != null)
            {
                bool idField = false;
                if (filter.SubFields != "*")
                {
                    foreach (string field in filter.SubFields.Replace(",", " ").Split(' '))
                    {
                        if (field == this.IDFieldName)
                        {
                            idField = true;
                            break;
                        }
                    }
                }
                if (!idField)
                    filter.AddField(this.IDFieldName);
            }
            if (filter is IBufferQueryFilter)
            {
                ISpatialFilter sFilter = BufferQueryFilter.ConvertToSpatialFilter(filter as IBufferQueryFilter);
                if (sFilter == null) return null;
                return GetFeatures(sFilter);
            }

            if (filter is IRowIDFilter)
            {
                return new ShapeFeatureCursor(this, _file, filter, ((IRowIDFilter)filter).IDs);
            }
            else
            {
                return new ShapeFeatureCursor(this, _file, filter, _tree);
            }
        }

        /*
		public gView.Framework.Data.IFeatureCursor GetFeatures(List<int> ids, gView.Framework.Data.getFeatureQueryType type)
		{
            QueryFilter filter = new QueryFilter();
            switch (type)
            {
                case getFeatureQueryType.All:
                    filter.SubFields = "*";
                    break;
                case getFeatureQueryType.Geometry:
                    filter.SubFields = this.ShapeFieldName;
                    break;
                case getFeatureQueryType.Attributes:
                    foreach (IField field in Fields)
                    {
                        filter.AddField(field.name);
                    }
                    break;
            }

            return new ShapeFeatureCursor(_file, filter, ids);
		}
        */

        public int CountFeatures
        {
            get
            {
                if (_file == null) return -1;
                return (int)_file.Entities;
            }
        }

        public ArrayList SpatialIndexNodes { get { return null; } }

        public string ShapeFieldName
        {
            get
            {
                return "SHAPE";
            }
        }

        /*
		public gView.Framework.Data.IFeature GetFeature(int id, gView.Framework.Data.getFeatureQueryType type)
		{
            List<int> IDs = new List<int>();
            IDs.Add(id);

            IFeatureCursor cursor = GetFeatures(IDs, type);
            if (cursor == null) return null;

            IFeature feat = cursor.NextFeature;
            cursor.Dispose();

            return feat;
		}
        */

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get
            {
                return _envelope;
            }
        }

        #endregion

        #region ITableClass Member

        public string IDFieldName
        {
            get
            {
                return "FID";
            }
        }

        public string Aliasname
        {
            get
            {
                return _file.Title;
            }
        }

        public gView.Framework.Data.IField FindField(string name)
        {
            foreach (IField field in Fields.ToEnumerable())
            {
                if (field.name == name) return field;
            }
            return null;
        }

        public gView.Framework.Data.ICursor Search(gView.Framework.Data.IQueryFilter filter)
        {
            return GetFeatures(filter);
        }

        public gView.Framework.Data.ISelectionSet Select(gView.Framework.Data.IQueryFilter filter)
        {
            if (filter is IBufferQueryFilter)
            {
                ISpatialFilter sFilter = BufferQueryFilter.ConvertToSpatialFilter(filter as IBufferQueryFilter);
                if (sFilter == null) return null;
                return Select(sFilter);
            }

            filter.SubFields = this.IDFieldName;

            IFeatureCursor cursor = (IFeatureCursor)(new ShapeFeatureCursor(this, _file, filter, _tree));
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
            get
            {
                return _file.Fields;
            }
        }

        public string Name
        {
            get
            {
                return _file.Title.Replace(".shp", "");
            }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }
        #endregion

        public static IGeometry GetGeometry2D(ShapeLib.SHPObject obj)
        {
            if (obj == null) return null;
            int parts = obj.nParts;
            int verts = obj.nVertices;
            int part = 0, nextp = 0;

            double[] X = new double[verts];
            double[] Y = new double[verts];
            int[] pStart = new int[parts];

            unsafe
            {
                int* pstart = (int*)obj.paPartStart.ToPointer();
                //ShapeLib.PartType * pType =(ShapeLib.PartType *)obj.paPartType.ToPointer();
                double* x = (double*)obj.padfX;
                double* y = (double*)obj.padfY;

                for (int i = 0; i < verts; i++)
                {
                    X[i] = x[i];
                    Y[i] = y[i];
                }
                for (int i = 0; i < parts; i++) pStart[i] = pstart[i];
            }

            switch (obj.shpType)
            {
                case ShapeLib.ShapeType.Point:
                case ShapeLib.ShapeType.PointM:
                case ShapeLib.ShapeType.PointZ:
                    return new Point(X[0], Y[0]);
                case ShapeLib.ShapeType.PolyLine:
                case ShapeLib.ShapeType.PolyLineM:
                case ShapeLib.ShapeType.PolyLineZ:
                    IPolyline polyline = new Polyline();
                    IPath path = null;

                    for (int v = 0; v < verts; v++)
                    {
                        if (path != null && nextp == v)
                        {
                            polyline.AddPath(path);
                            path = null;
                        }
                        if (path == null)
                        {
                            if (parts <= part + 1)
                            {
                                nextp = verts;
                            }
                            else
                            {
                                nextp = pStart[part + 1];
                                part++;
                            }
                            path = new gView.Framework.Geometry.Path();
                        }

                        path.AddPoint(new Point(X[v], Y[v]));
                    }
                    polyline.AddPath(path);

                    X = Y = null;
                    return polyline;
                case ShapeLib.ShapeType.Polygon:
                case ShapeLib.ShapeType.PolygonM:
                case ShapeLib.ShapeType.PolygonZ:
                    IPolygon polygon = new Polygon();
                    IRing ring = null;

                    for (int v = 0; v < verts; v++)
                    {
                        if (ring != null && nextp == v)
                        {
                            polygon.AddRing(ring);
                            ring = null;
                        }
                        if (ring == null)
                        {
                            if (parts <= part + 1)
                            {
                                nextp = verts;
                            }
                            else
                            {
                                nextp = pStart[part + 1];
                                part++;
                            }
                            ring = new Ring();
                        }

                        ring.AddPoint(new Point(X[v], Y[v]));
                    }
                    polygon.AddRing(ring);

                    X = Y = null;
                    return polygon;
            }
            return null;
        }

        #region IGeometryDef Member

        public bool HasZ
        {
            get { return _geomDef.HasZ; }
        }

        public bool HasM
        {
            get { return _geomDef.HasM; }
        }

        public geometryType GeometryType
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

        public GeometryFieldType GeometryFieldType
        {
            get { return GeometryFieldType.Default; }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            if (_file != null)
            {
                _file.Close();
                _file = null;
            }
        }

        #endregion
    }

    internal class ShapeFeatureCursor : FeatureCursor
    {
        private SHPFile _file = null;
        private long _shape = 0;
        private List<int> _IDs = null;
        private Envelope _bounds = null;
        private IQueryFilter _filter;
        private DBFDataReader _dataReader = null;
        private bool _queryShape = false;
        private ISpatialFilter _spatialFilter = null;
        private IGeometry _queryGeometry;
        private List<object> _unique;
        private string _uniqueField = String.Empty;

        public ShapeFeatureCursor(IFeatureClass fc, SHPFile file, IQueryFilter filter, IIndexTree tree)
            : base((fc != null) ? fc.SpatialReference : null,
                   (filter != null) ? filter.FeatureSpatialReference : null)
        {
            base.KnowsFunctions = false;

            if (file == null) return;
            _file = new SHPFile(file);

            _filter = filter;
            if (filter is ISpatialFilter)
            {
                IEnvelope env = ((ISpatialFilter)filter).Geometry.Envelope;
                if (tree != null)
                {
                    _IDs = tree.FindShapeIds(env);
                    _IDs.Sort();
                }
                //_IDs = tree.FindShapeIds(env);
                _bounds = new Envelope(env);
                _spatialFilter = filter as ISpatialFilter;
                _queryGeometry = ((ISpatialFilter)filter).Geometry;
                _queryShape = true;
            }
            else
            {
                foreach (string fname in filter.SubFields.Split(' '))
                {
                    if (fname == "SHAPE" || fname == "*")
                    {
                        _queryShape = true;
                        break;
                    }
                }
            }

            if (filter.WhereClause != "" && _filter.SubFields != "*" && !(filter is IRowIDFilter))
            {
                //StringBuilder sb = new StringBuilder();
                //sb.Append("FID");

                QueryFilter f = new QueryFilter();
                f.SubFields = _filter.SubFields;
                foreach (IField field in _file.Fields.ToEnumerable())
                {
                    if (field.name == "FID") continue;
                    if (filter.WhereClause.IndexOf(" " + field.name + " ") != -1 ||
                        filter.WhereClause.IndexOf("(" + field.name + " ") != -1 ||
                        filter.WhereClause.IndexOf(" " + field.name + "=") != -1 ||
                        filter.WhereClause.IndexOf("(" + field.name + "=") != -1 ||
                        filter.WhereClause.IndexOf(" " + field.name + "<") != -1 ||
                        filter.WhereClause.IndexOf("(" + field.name + "<") != -1 ||
                        filter.WhereClause.IndexOf(" " + field.name + ">") != -1 ||
                        filter.WhereClause.IndexOf("(" + field.name + ">") != -1 ||
                        filter.WhereClause.IndexOf("[" + field.name + "]") != -1 ||
                        filter.WhereClause.IndexOf(field.name + " ") == 0 ||
                        filter.WhereClause.IndexOf(field.name + "=") == 0 ||
                        filter.WhereClause.IndexOf(field.name + "<") == 0 ||
                        filter.WhereClause.IndexOf(field.name + ">") == 0
                        )
                    {
                        f.AddField(field.name);
                    }
                }

                /*
                _dataReader = _file.DBFDataReader(sb.ToString());
                for (uint i = 1; i <= _file.Entities; i++)
                    _dataReader.AddRecord(i);

                DataRow [] rows=_dataReader.Table.Select(_filter.WhereClause);

                _IDs = new ArrayList();
                foreach (DataRow row in rows)
                {
                    uint id=_file.GetIndexFromRecNumber((uint)row["FID"]);
                    _IDs.Add((int)id);
                }
                _dataReader.Dispose();
                */

                _dataReader = _file.DBFDataReader(f.SubFields.Replace(" ", ",").Replace("DISTINCT(", "").Replace(")", ""));
            }
            else
            {
                _dataReader = _file.DBFDataReader(filter.SubFields.Replace(" ", ",").Replace("DISTINCT(", "").Replace(")", ""));
            }

            if (_filter is IDistinctFilter)
            {
                _unique = new List<object>();
                _uniqueField = _filter.SubFields.Replace("DISTINCT(", "").Replace(")", "");
            }

            if (_filter is FunctionFilter)
            {
            }

            /*
            if (!(filter is ISpatialFilter))
            {
                DataTable tab = _dataReader.AllRecords;
                tab = null;
            }*/
        }
        public ShapeFeatureCursor(IFeatureClass fc, SHPFile file, IQueryFilter filter, List<int> FIDs)
            : this(fc, file, filter, (IIndexTree)null)
        {
            _IDs = new List<int>();
            foreach (int fid in FIDs)
            {
                uint id = _file.GetIndexFromRecNumber((uint)fid);
                if (id > _file.Entities) continue;
                _IDs.Add((int)id);
            }
        }
        private bool AppendAttributes(IFeature feature)
        {
            if (_dataReader == null) return true;
            _dataReader.Clear();
            _dataReader.AddRecord((uint)feature.OID);

            if (_dataReader.Table.Rows.Count == 0) return true;

            if (_filter.WhereClause != "")
            {
                if (_dataReader.Table.Select(_filter.WhereClause).Length == 0)
                    return false;
            }

            foreach (DataColumn col in _dataReader.Table.Columns)
            {
                FieldValue fv = new FieldValue(col.ColumnName, _dataReader.Table.Rows[0][col.ColumnName]);
                feature.Fields.Add(fv);
            }

            return true;
        }

        #region IFeatureCursor Member

        public override IFeature NextFeature
        {
            get
            {
                if (_file == null) return null;

                IFeature feature;
                if (_IDs == null)
                {
                    while (true)
                    {
                        if (_shape >= _file.Entities) return null;
                        if (_queryShape)
                        {
                            if (_bounds != null)
                            {
                                while (true)
                                {
                                    if (_shape >= _file.Entities) return null;
                                    //IEnvelope e = _file.ReadEnvelope((uint)_shape);
                                    feature = _file.ReadShape((uint)_shape++, _bounds);  // wenn nicht in Box -> feature.Shape==null
                                    if (feature == null || feature.Shape == null)
                                    {
                                        //_shape++;
                                        continue;
                                    }

                                    if (_spatialFilter != null)
                                    {
                                        if (!gView.Framework.Geometry.SpatialRelation.Check(_spatialFilter, feature.Shape))
                                        {
                                            continue;
                                        }
                                    }
                                    //if (!_fuzzyQuery)
                                    //{
                                    //    if (!SpatialAlgorithms.Algorithm.IntersectBox(feature.Shape, _bounds))
                                    //    {
                                    //        //_shape++;
                                    //        continue;
                                    //    }
                                    //}
                                    break;
                                }
                            }
                            else
                            {
                                //if (_shape == 3200)
                                //{
                                //    feature = null;
                                //}
                                feature = _file.ReadShape((uint)_shape++);
                            }
                        }
                        else
                        {
                            feature = new Feature();
                            ((Feature)feature).OID = ((int)_shape++) + 1;
                        }

                        if (feature == null || !AppendAttributes(feature)) continue;

                        // DistinctFilter
                        if (_unique != null && !String.IsNullOrEmpty(_uniqueField))
                        {
                            if (_unique.Contains(feature[_uniqueField]))
                                continue;
                            _unique.Add(feature[_uniqueField]);
                        }

                        Transform(feature);
                        return feature;
                    }
                }
                else
                {
                    while (true)
                    {
                        if (_shape >= _IDs.Count) return null;
                        if (_queryShape)
                        {
                            if (_bounds != null)
                            {
                                while (true)
                                {
                                    if (_shape >= _IDs.Count) return null;
                                    //IEnvelope e = _file.ReadEnvelope((uint)_shape);
                                    feature = _file.ReadShape((uint)((int)_IDs[(int)_shape++]), _bounds); // wenn nicht in Box -> feature.Shape==null
                                    if (feature == null || feature.Shape == null)
                                    {
                                        //_shape++;
                                        continue;
                                    }
                                    if (_spatialFilter != null)
                                    {
                                        if (!gView.Framework.Geometry.SpatialRelation.Check(_spatialFilter, feature.Shape))
                                        {
                                            continue;
                                        }
                                    }
                                    //if (!_fuzzyQuery)
                                    //{
                                    //    if (!SpatialAlgorithms.Algorithm.IntersectBox(feature.Shape, _bounds))
                                    //    {
                                    //        //_shape++;
                                    //        continue;
                                    //    }
                                    //}
                                    break;
                                }
                            }
                            else
                            {
                                feature = _file.ReadShape((uint)((int)_IDs[(int)_shape++]));
                            }
                        }
                        else
                        {
                            feature = new Feature();
                            ((Feature)feature).OID = ((int)_IDs[(int)_shape++]) + 1;
                        }

                        if (feature == null || !AppendAttributes(feature)) continue;

                        Transform(feature);
                        return feature;
                    }
                }
            }
        }

        #endregion

        #region ICursor Member

        override public void Dispose()
        {
            base.Dispose();
            if (_dataReader != null) _dataReader.Dispose();
            _dataReader = null;
            if (_file != null) _file.Close();
            _file = null;
        }

        #endregion
    }
}
