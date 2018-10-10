using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.Network;
using System.Data;
using System.Data.Common;
using gView.Framework.Network.Build;
using gView.Framework.Network.Algorthm;
using gView.Framework.FDB;

namespace gView.DataSources.Fdb.PostgreSql
{
    class pgNetworkFeatureClass : IFeatureClass, INetworkFeatureClass
    {
        private pgFDB _fdb;
        private IDataset _dataset;
        private string _name = String.Empty, _aliasname = String.Empty;
        private Fields _fields;
        private GeometryDef _geomDef;
        private IFeatureClass _nodeFc = null;
        private Dictionary<int, IFeatureClass> _edgeFcs = new Dictionary<int, IFeatureClass>();
        private int _fcid = -1;
        private GraphWeights _weights = null;
        private NetworkObjectSerializer.PageManager _pageManager = null;

        public pgNetworkFeatureClass(pgFDB fdb, IDataset dataset, string name, GeometryDef geomDef)
        {
            _fdb = fdb;
            _fcid = _fdb.FeatureClassID(_fdb.DatasetID(dataset.DatasetName), name);

            _dataset = dataset;
            _geomDef = (geomDef != null) ? geomDef : new GeometryDef();

            if (_geomDef != null && _geomDef.SpatialReference == null && dataset is IFeatureDataset)
                _geomDef.SpatialReference = ((IFeatureDataset)dataset).SpatialReference;

            _fields = new Fields();

            _name = _aliasname = name;

            IDatasetElement element = _dataset[_name + "_Nodes"];
            if (element != null)
                _nodeFc = element.Class as IFeatureClass;

            element = _dataset[_name + "_ComplexEdges"];
            if (element != null && element.Class is IFeatureClass)
                _edgeFcs.Add(-1, (IFeatureClass)element.Class);

            DataTable tab = _fdb.Select("\"FCID\"", "\"FDB_NetworkClasses\"", "\"NetworkId\"=" + _fcid);
            if (tab != null && tab.Rows.Count > 0)
            {
                StringBuilder where = new StringBuilder();
                where.Append("\"GeometryType\"=" + ((int)geometryType.Polyline).ToString() + " AND \"ID\" in(");
                for (int i = 0; i < tab.Rows.Count; i++)
                {
                    if (i > 0) where.Append(",");
                    where.Append(tab.Rows[i]["fcid"].ToString());
                }
                where.Append(")");

                tab = _fdb.Select("\"ID\",\"Name\"", "\"FDB_FeatureClasses\"", where.ToString());
                if (tab != null)
                {
                    foreach (DataRow row in tab.Rows)
                    {
                        element = _dataset[row["name"].ToString()];
                        if (element != null && element.Class is IFeatureClass)
                            _edgeFcs.Add((int)row["id"], element.Class as IFeatureClass);
                    }
                }
            }

            _weights = _fdb.GraphWeights(name);
            Dictionary<Guid, string> weightTableNames = null;
            Dictionary<Guid, GraphWeightDataType> weightDataTypes = null;
            if (_weights != null && _weights.Count > 0)
            {
                weightTableNames = new Dictionary<Guid, string>();
                weightDataTypes = new Dictionary<Guid, GraphWeightDataType>();
                foreach (IGraphWeight weight in _weights)
                {
                    if (weight == null)
                        continue;

                    weightTableNames.Add(weight.Guid, _fdb.TableName(_name + "_Weights_" + weight.Guid.ToString("N").ToLower()));
                    weightDataTypes.Add(weight.Guid, weight.DataType);
                }
            }
            _pageManager = new NetworkObjectSerializer.PageManager(
                gView.Framework.Db.DataProvider.PostgresProvider,
                _fdb.ConnectionString, _name,
                _fdb.TableName("FC_" + _name),
                _fdb.TableName(_name + "_Edges"),
                _fdb.TableName("FC_" + name + "_Nodes"),
                weightTableNames, weightDataTypes, _fdb
                );
        }

        #region IFeatureClass Member

        public string ShapeFieldName
        {
            get { return String.Empty; }
        }

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get
            {
                gView.Framework.Geometry.Envelope env = null;
                if (_edgeFcs != null)
                {
                    foreach (IFeatureClass edgeFc in _edgeFcs.Values)
                    {
                        if (edgeFc != null)
                        {
                            if (env == null)
                                env = new Envelope(edgeFc.Envelope);
                            else
                                env.Union(edgeFc.Envelope);
                        }
                    }
                }
                if (_nodeFc != null)
                {
                    if (env == null)
                        env = new Envelope(_nodeFc.Envelope);
                    else
                        env.Union(_nodeFc.Envelope);
                }
                return (env != null) ? env : new Envelope();
            }
        }

        public int CountFeatures
        {
            get
            {
                int c = 0;
                if (_edgeFcs != null)
                {
                    foreach (IFeatureClass edgeFc in _edgeFcs.Values)
                        if (edgeFc != null)
                            c += edgeFc.CountFeatures;
                }
                if (_nodeFc != null)
                    c += _nodeFc.CountFeatures;
                return c;
            }
        }

        public IFeatureCursor GetFeatures(IQueryFilter filter)
        {
            List<IFeatureClass> edgeFcs = new List<IFeatureClass>();
            if (_edgeFcs != null)
            {
                foreach (IFeatureClass fc in _edgeFcs.Values)
                    edgeFcs.Add(fc);
            }

            return new NetworkFeatureCursor(_fdb, _name, edgeFcs, _nodeFc, filter);
        }

        #endregion

        #region ITableClass Member

        public ICursor Search(IQueryFilter filter)
        {
            return GetFeatures(filter);
        }

        public ISelectionSet Select(IQueryFilter filter)
        {
            return null;
        }

        public IFields Fields
        {
            get { return _fields; }
        }

        public IField FindField(string name)
        {
            return _fields.FindField(name);
        }

        public string IDFieldName
        {
            get { return "FDB_OID"; }
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get { return _name; }
        }

        public string Aliasname
        {
            get { return _aliasname; }
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

        //public GeometryFieldType GeometryFieldType
        //{
        //    get
        //    {
        //        return _geomDef.GeometryFieldType;
        //    }
        //}
        #endregion

        #region HelperClasses
        private class NetworkFeatureCursor : IFeatureCursor
        {
            private pgFDB _fdb;
            private string _networkName;
            private List<IFeatureClass> _edgeFcs = new List<IFeatureClass>();
            private IFeatureClass _edgeFc = null, _nodeFc = null;
            private int _edgeFcIndex = 0, _fcid = -1;
            private IFeatureCursor _edgeCursor = null, _nodeCursor = null;
            private IQueryFilter _filter;

            public NetworkFeatureCursor(pgFDB fdb, string networkName, List<IFeatureClass> edgeFcs, IFeatureClass nodeFc, IQueryFilter filter)
            {
                _fdb = fdb;
                _networkName = networkName;

                _edgeFcs = edgeFcs;
                _nodeFc = nodeFc;
                _filter = filter;
            }

            #region IFeatureCursor Member

            public IFeature NextFeature
            {
                get
                {
                    if (_edgeCursor == null && _edgeFcs != null && _edgeFcIndex < _edgeFcs.Count)
                    {
                        IFeatureClass fc = _edgeFcs[_edgeFcIndex++];
                        _fcid = _fdb.FeatureClassID(_fdb.DatasetID(fc.Dataset.DatasetName), fc.Name);
                        if (_fcid < 0)
                            return NextFeature;
                        if (fc.Name == _networkName + "_ComplexEdges")
                            _fcid = -1;

                        IQueryFilter f = (IQueryFilter)_filter.Clone();
                        if (f.SubFields != "*")
                        {
                            f.AddField(fc.IDFieldName);
                            f.AddField(fc.ShapeFieldName);
                        }

                        _edgeCursor = fc.GetFeatures(f);
                        if (_edgeCursor == null)
                            return NextFeature;
                    }
                    if (_edgeCursor != null)
                    {
                        IFeature feature = _edgeCursor.NextFeature;
                        if (feature != null)
                        {
                            feature.Fields.Add(new FieldValue("NETWORK#FCID", _fcid));
                            return feature;
                        }

                        _edgeCursor.Dispose();
                        _edgeCursor = null;
                        return NextFeature;
                    }
                    if (_nodeCursor == null && _nodeFc != null)
                    {
                        _nodeCursor = _nodeFc.GetFeatures(_filter);
                    }
                    if (_nodeCursor != null)
                        return _nodeCursor.NextFeature;

                    return null;
                }
            }

            #endregion

            #region IDisposable Member

            public void Dispose()
            {
                if (_edgeCursor != null)
                {
                    _edgeCursor.Dispose();
                    _edgeCursor = null;
                }
                if (_nodeCursor != null)
                {
                    _nodeCursor.Dispose();
                    _nodeCursor = null;
                }
            }

            #endregion
        }

        private class pgFDBGraphTableAdapter : IGraphTableAdapter
        {
            private string _name, _connString;
            private pgNetworkFeatureClass _nfc;
            private Dictionary<int, NetworkObjectSerializer.GraphPage> _pages = new Dictionary<int, NetworkObjectSerializer.GraphPage>();

            public pgFDBGraphTableAdapter(pgFDB fdb, pgNetworkFeatureClass nfc)
            {
                _connString = fdb.ConnectionString;
                _nfc = nfc;
                _name = _nfc.Name; //name;
            }

            #region IGraphTableAdapter Member

            public GraphTableRows QueryN1(int n1)
            {
                if (_nfc != null && _nfc._pageManager != null)
                {
                    return _nfc._pageManager.QueryN1(n1);
                }
                return new GraphTableRows();
            }

            public IGraphTableRow QueryN1ToN2(int n1, int n2)
            {
                foreach (IGraphTableRow row in QueryN1(n1))
                    if (row.N2 == n2)
                        return row;
                return null;
            }

            public IGraphEdge QueryEdge(int eid)
            {
                if (_nfc != null && _nfc._pageManager != null)
                    return _nfc._pageManager.GetEdge(eid);

                return null;
            }

            public double QueryEdgeWeight(Guid weightGuid, int eid)
            {
                if (_nfc != null && _nfc._pageManager != null)
                    return _nfc._pageManager.GetEdgeWeight(weightGuid, eid);

                return 0.0;
            }

            public bool SwitchState(int nid)
            {
                if (_nfc != null && _nfc._pageManager != null)
                    return _nfc._pageManager.GetSwitchState(nid);

                return false;
            }

            public int GetNodeFcid(int nid)
            {
                if (_nfc != null && _nfc._pageManager != null)
                    return _nfc._pageManager.GetNodeFcid(nid);

                return -1;
            }

            public NetworkNodeType GetNodeType(int nid)
            {
                if (_nfc != null && _nfc._pageManager != null)
                    return _nfc._pageManager.GetNodeType(nid);

                return NetworkNodeType.Unknown;
            }

            public Features QueryNodeEdgeFeatures(int n1)
            {
                Features features = new Features();

                GraphTableRows rows = QueryN1(n1);
                if (rows == null)
                    return features;

                foreach (GraphTableRow row in rows)
                {
                    IFeature feature = _nfc.GetEdgeFeature(row.EID);
                    if (feature != null)
                    {
                        feature.Fields.Add(new FieldValue("NETWORK#EID", row.EID));
                        features.Add(feature);
                    }
                }

                return features;
            }

            public Features QueryNodeFeatures(int n1)
            {
                Features features = new Features();

                IFeature feature = _nfc.GetNodeFeature(n1);
                if (feature != null)
                    features.Add(feature);

                return features;
            }

            #endregion
        }
        #endregion

        #region INetworkFeatureClass Member

        public IGraphTableAdapter GraphTableAdapter()
        {
            return new pgFDBGraphTableAdapter(_fdb, this);
        }

        public IFeatureCursor GetNodeFeatures(IQueryFilter filter)
        {
            if (_nodeFc != null)
                return _nodeFc.GetFeatures(filter);
            return null;
        }

        public IFeatureCursor GetEdgeFeatures(IQueryFilter filter)
        {
            if (_edgeFcs.Count == 0)
                return null;

            if (filter is SpatialFilter)
            {
                List<IFeatureClass> edgeFcs = new List<IFeatureClass>();
                if (_edgeFcs != null)
                {
                    foreach (IFeatureClass fc in _edgeFcs.Values)
                        edgeFcs.Add(fc);
                }
                return new NetworkFeatureCursor(_fdb, _name, edgeFcs, null, filter);
            }

            if (filter is RowIDFilter)
            {
                RowIDFilter idFilter = (RowIDFilter)filter;

                Dictionary<int, QueryFilter> rfilters = new Dictionary<int, QueryFilter>();
                Dictionary<int, Dictionary<int, List<FieldValue>>> additionalFields = new Dictionary<int, Dictionary<int, List<FieldValue>>>();
                foreach (int eid in idFilter.IDs)
                {
                    IGraphEdge edge = _pageManager.GetEdge(eid);
                    if (edge == null || _edgeFcs.ContainsKey(edge.FcId) == false)
                        continue;

                    if (!rfilters.ContainsKey(edge.FcId))
                    {
                        string idFieldName = "FDB_OID";
                        string shapeFieldName = "FDB_SHAPE";

                        IFeatureClass fc = edge.FcId >= 0 ? _fdb.GetFeatureclass(edge.FcId) : null;
                        if (fc != null)
                        {
                            idFieldName = fc.IDFieldName;
                            shapeFieldName = fc.ShapeFieldName;
                        }

                        RowIDFilter rfilter=new RowIDFilter(edge.FcId >= 0 ? idFieldName : "EID");
                        rfilter.fieldPostfix = rfilter.fieldPrefix = "\"";
                        rfilters.Add(edge.FcId, rfilter);
                        rfilters[edge.FcId].AddField(shapeFieldName);
                        additionalFields.Add(edge.FcId, new Dictionary<int, List<FieldValue>>());

                        rfilter.IgnoreUndefinedFields = true;
                        rfilters[edge.FcId].AddField(shapeFieldName);
                        if (filter.SubFields.IndexOf("*") != -1)
                        {
                            rfilters[edge.FcId].AddField("*");
                        }
                        else
                        {
                            foreach (string field in filter.SubFields.Split(' '))
                            {
                                if (field == shapeFieldName)
                                    continue;
                                if (fc.Fields.FindField(field) != null)
                                    rfilter.AddField(fc.Fields.FindField(field).name);
                            }
                        }
                    }

                    ((RowIDFilter)rfilters[edge.FcId]).IDs.Add(edge.FcId >= 0 ? edge.Oid : edge.Eid);
                    additionalFields[edge.FcId].Add(edge.FcId >= 0 ? edge.Oid : edge.Eid, new List<FieldValue>()
                    {
                        new FieldValue("_eid",edge.Eid)
                    });
                }
                if (rfilters.ContainsKey(-1))
                {
                    RowIDFilter complexEdgeFilter = (RowIDFilter)rfilters[-1];
                    QueryFilter ceFilter = new QueryFilter(complexEdgeFilter);
                    ceFilter.WhereClause = complexEdgeFilter.RowIDWhereClause;
                    rfilters[-1] = ceFilter;
                }
                return new CursorCollection<int>(_edgeFcs, rfilters, additionalFields);
            }

            return null;
        }

        public IFeature GetNodeFeature(int nid)
        {
            QueryFilter filter = new QueryFilter();
            filter.WhereClause = _fdb.DbColName("FDB_OID") + "=" + nid;
            filter.AddField("*");

            try
            {
                using (IFeatureCursor cursor = GetNodeFeatures(filter))
                {
                    if (cursor == null)
                        return null;
                    return cursor.NextFeature;
                }
            }
            catch { return null; }
        }

        public IFeature GetEdgeFeature(int eid)
        {
            RowIDFilter filter = new RowIDFilter(String.Empty);
            filter.IDs.Add(eid);
            filter.AddField("*");

            IFeatureCursor cursor = GetEdgeFeatures(filter);
            if (cursor == null)
                return null;

            IFeature feature = cursor.NextFeature;
            cursor.Dispose();

            if (feature != null && feature.FindField("fcid") != null && feature.FindField("oid") != null &&
                _edgeFcs.ContainsKey((int)feature["fcid"]))
            {
                IGraphEdge edge = _pageManager.GetEdge(eid);
                try
                {
                    if (edge != null && edge.FcId == -1) // Complex Edge
                    {
                        filter = new RowIDFilter("FDB_OID");
                        filter.IDs.Add((int)feature["oid"]);
                        filter.AddField("*");
                        IFeatureClass fc = _edgeFcs[(int)feature["fcid"]];
                        using (IFeatureCursor c = fc.GetFeatures(filter))
                        {
                            return c.NextFeature;
                        }
                    }
                }
                catch { }
            }
            return feature;
        }

        public IFeature GetNodeFeatureAttributes(int nodeId, string[] attributes)
        {
            try
            {
                QueryFilter filter = new QueryFilter();
                filter.WhereClause = _fdb.DbColName("FDB_OID") + "=" + nodeId;
                filter.AddField("fcid");
                filter.AddField("oid");

                IFeature feature;
                using (IFeatureCursor cursor = GetNodeFeatures(filter))
                    feature = cursor.NextFeature;
                if (feature == null)
                    return null;

                string fcName = _fdb.GetFeatureClassName((int)feature["fcid"]);
                IDatasetElement element = _dataset[fcName];
                if (element == null)
                    return null;
                IFeatureClass fc = element.Class as IFeatureClass;
                if (fc == null)
                    return null;

                filter = new QueryFilter();
                if (fc is IDatabaseNames)
                {
                    filter.WhereClause = ((IDatabaseNames)fc).DbColName(fc.IDFieldName) + "=" + Convert.ToInt32(feature["oid"]);
                }
                else
                {
                    filter.WhereClause = _fdb.DbColName(fc.IDFieldName) + "=" + Convert.ToInt32(feature["oid"]);
                }

                if (attributes == null)
                {
                    filter.AddField("*");
                }
                else
                {
                    foreach (string attribute in attributes)
                    {
                        if (attribute == "*")
                            filter.AddField(attribute);
                        else if (fc.FindField(attribute) != null)
                            filter.AddField(attribute);
                    }
                }

                using (IFeatureCursor cursor = fc.GetFeatures(filter))
                    feature = cursor.NextFeature;

                if (feature != null)
                    feature.Fields.Add(new FieldValue("_classname", fc.Name));

                return feature;
            }
            catch
            {
                return null;
            }
        }

        public IFeature GetEdgeFeatureAttributes(int edgeId, string[] attributes)
        {
            //RowIDFilter filter = new RowIDFilter(String.Empty);
            //filter.IDs.Add(edgeId);

            return GetEdgeFeature(edgeId);
        }

        public int MaxNodeId
        {
            get
            {
                try
                {
                    DbProviderFactory factory = gView.Framework.Db.DataProvider.PostgresProvider;
                    using (DbConnection connection = factory.CreateConnection())
                    using (DbCommand command = factory.CreateCommand())
                    {
                        connection.ConnectionString = _fdb.ConnectionString;
                        command.CommandText = "SELECT MAX(" + _fdb.DbColName("FDB_OID") + ") FROM " + _fdb.TableName("FC_" + _nodeFc.Name);
                        command.Connection = connection;
                        connection.Open();

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                catch { return -1; }
            }
        }

        public GraphWeights GraphWeights
        {
            get { return _weights; }
        }

        public bool HasDisabledSwitches
        {
            get
            {
                try
                {
                    DbProviderFactory factory = gView.Framework.Db.DataProvider.PostgresProvider;
                    using (DbConnection connection = factory.CreateConnection())
                    using (DbCommand command = factory.CreateCommand())
                    {
                        connection.ConnectionString = _fdb.ConnectionString;
                        command.CommandText = "SELECT COUNT(" + _fdb.DbColName("FDB_OID") + ") FROM " + _fdb.TableName("FC_" + _nodeFc.Name) + " WHERE [switch]=1 AND [state]=0";
                        command.Connection = connection;
                        connection.Open();

                        return Convert.ToInt32(command.ExecuteScalar()) > 0;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public IGraphEdge GetGraphEdge(IPoint point, double tolerance)
        {
            if (point == null)
                return null;
            SpatialFilter filter = new SpatialFilter();
            filter.Geometry = new Envelope(point.X - tolerance, point.Y - tolerance,
                                           point.X + tolerance, point.Y + tolerance);
            filter.AddField("FDB_SHAPE");
            filter.AddField("FDB_OID");

            using (IFeatureCursor cursor = GetEdgeFeatures(filter))
            {
                IFeature feature, selected = null;
                double selectedDist = double.MaxValue;
                int selectedFcId = int.MinValue;
                IPoint snappedPoint = null;
                while ((feature = cursor.NextFeature) != null)
                {
                    if (!(feature.Shape is IPolyline) ||
                          feature.FindField("NETWORK#FCID") == null)
                        continue;

                    int fcid = (int)feature["NETWORK#FCID"];

                    double dist, stat;
                    IPoint spoint = gView.Framework.SpatialAlgorithms.Algorithm.Point2PolylineDistance((IPolyline)feature.Shape, point, out dist, out stat);
                    if (spoint == null)
                        continue;

                    if (selected == null || dist <= selectedDist)
                    {
                        if (fcid != -1)
                        {
                            #region Do complex Edge exists
                            IFeatureClass complexEdgeFc = _edgeFcs[-1];
                            if (complexEdgeFc != null)
                            {
                                QueryFilter complexEdgeFilter = new QueryFilter();
                                complexEdgeFilter.WhereClause = "fcid=" + fcid + " AND oid=" + feature.OID;
                                complexEdgeFilter.AddField("FDB_OID");
                                using (IFeatureCursor complexEdgeCursor = complexEdgeFc.GetFeatures(complexEdgeFilter))
                                {
                                    if (complexEdgeCursor.NextFeature != null)
                                        continue;
                                }
                            }
                            #endregion
                        }
                        selected = feature;
                        selectedDist = dist;
                        selectedFcId = fcid;
                        snappedPoint = spoint;
                    }
                }
                if (selected == null)
                    return null;

                int eid = -1;
                if (selectedFcId == -1)
                {
                    #region Complex Edge
                    object eidObj = _fdb._conn.QuerySingleField("SELECT eid FROM " + _fdb.TableName("FC_" + _name + "_ComplexEdges") + " WHERE " + _fdb.DbColName("FDB_OID") + "=" + selected.OID, "eid");
                    if (eidObj != null)
                        eid = (int)eidObj;
                    #endregion
                }
                else
                {
                    object eidObj = _fdb._conn.QuerySingleField("SELECT eid FROM " + _fdb.TableName(_name + "_EdgeIndex") + " WHERE fcid=" + selectedFcId + " AND oid=" + selected.OID, "eid");
                    if (eidObj != null)
                        eid = (int)eidObj;
                }

                if (eid != -1)
                {
                    point.X = snappedPoint.X;
                    point.Y = snappedPoint.Y;

                    IGraphTableAdapter gt = this.GraphTableAdapter();
                    return gt.QueryEdge(eid);
                }
                return null;
            }
        }

        public List<IFeatureClass> NetworkClasses
        {
            get
            {
                if (_fdb == null)
                    return new List<IFeatureClass>();

                return _fdb.NetworkFeatureClasses(this.Name);
            }
        }

        public string NetworkClassName(int fcid)
        {
            if (_fdb == null)
                return String.Empty;

            return _fdb.GetFeatureClassName(fcid);
        }

        public int NetworkClassId(string className)
        {
            if (_fdb == null || _dataset == null)
                return -1;
            return _fdb.FeatureClassID(_fdb.DatasetID(_dataset.DatasetName), className);
        }
        #endregion
    }
}
