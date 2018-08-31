using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using gView.Framework.Data;
using gView.Framework.Network.Algorthm;
using System.Data.Common;
using gView.Framework.Network;
using gView.Framework.IO;
using System.Data;
using gView.Framework.FDB;

namespace gView.Framework.Network.Build
{
    public class NetworkObjectSerializer
    {
        #region General
        public static int PageSize
        {
            get { return 512; }
        }
        public static int Page(int index)
        {
            return (index - 1) / PageSize;
        }
        #endregion

        #region Edges
        public static byte[] SerializeEdges(List<IFeature> features)
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());

            foreach (Feature feature in features)
            {
                writer.Write(Convert.ToInt32(feature["EID"]));
                if ((bool)feature["ISCOMPLEX"] == true)
                {
                    writer.Write((int)-1);
                    writer.Write(Convert.ToInt32(feature["N1"]));
                    writer.Write(Convert.ToInt32(feature["N2"]));
                }
                else
                {
                    writer.Write(Convert.ToInt32(feature["FCID"]));
                    writer.Write(Convert.ToInt32(feature["OID"]));
                    writer.Write(Convert.ToInt32(feature["N1"]));
                    writer.Write(Convert.ToInt32(feature["N2"]));
                }
            }

            byte[] bytes = new byte[writer.BaseStream.Length];
            writer.BaseStream.Position = 0;
            writer.BaseStream.Read(bytes, (int)0, (int)writer.BaseStream.Length);
            writer.Close();

            return bytes;
        }

        public static EdgePage DeserializeEdges(byte[] bytes)
        {
            BinaryReader r = new BinaryReader(new MemoryStream());
            r.BaseStream.Write(bytes, 0, (bytes.Length));
            r.BaseStream.Position = 0;

            EdgePage page = new EdgePage();
            while (r.BaseStream.Position < r.BaseStream.Length)
            {
                int eid = r.ReadInt32();
                int fcid = r.ReadInt32(), oid = -1;
                if (fcid >= 0)
                    oid = r.ReadInt32();
                int n1 = r.ReadInt32();
                int n2 = r.ReadInt32();

                page.Add(eid, fcid, oid, n1, n2);
            }
            return page;
        }

        public class EdgePage
        {
            private List<IGraphEdge> _edges = new List<IGraphEdge>();

            public void Add(int eid, int fcid, int oid, int n1, int n2)
            {
                GraphEdge edge = new GraphEdge(eid, fcid, oid, n1, n2);

                _edges.Add(edge);
            }

            public IGraphEdge GetEdge(int eid)
            {
                foreach (IGraphEdge edge in _edges)
                    if (edge.Eid == eid)
                        return edge;

                return null;
            }
        }
        #endregion

        #region Graph
        public static byte[] SerializeGraph(List<IFeature> features)
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());

            foreach (Feature feature in features)
            {
                writer.Write(Convert.ToInt32(feature["N1"]));
                writer.Write(Convert.ToInt32(feature["N2"]));
                writer.Write(Convert.ToInt32(feature["EID"]));
                writer.Write(Convert.ToDouble(feature["GEOLENGTH"]));
            }

            byte[] bytes = new byte[writer.BaseStream.Length];
            writer.BaseStream.Position = 0;
            writer.BaseStream.Read(bytes, (int)0, (int)writer.BaseStream.Length);
            writer.Close();

            return bytes;
        }

        public static GraphPage DeserializeGraph(byte[] bytes)
        {
            BinaryReader r = new BinaryReader(new MemoryStream());
            r.BaseStream.Write(bytes, 0, (bytes.Length));
            r.BaseStream.Position = 0;

            GraphPage page = new GraphPage();
            while (r.BaseStream.Position < r.BaseStream.Length)
            {
                page.Add(
                    r.ReadInt32(),
                    r.ReadInt32(),
                    r.ReadInt32(),
                    r.ReadDouble());
            }
            return page;
        }

        public class GraphPage
        {
            private GraphTableRows _rows = new GraphTableRows();

            public void Add(int n1, int n2, int eid, double length)
            {
                _rows.Add(new GraphTableRow(
                    n1, n2, eid, length, length));
            }

            public GraphTableRows QueryN1(int n1)
            {
                GraphTableRows rows = new GraphTableRows();
                foreach (GraphTableRow row in _rows)
                {
                    if (row.N1 == n1)
                        rows.Add(row);
                }
                return rows;
            }
        }
        #endregion

        #region Weights
        public static byte[] SerializeWeight(IGraphWeight weight)
        {
            if (weight != null)
            {
                XmlStream stream = new XmlStream("weight");
                weight.Save(stream);

                MemoryStream ms = new MemoryStream();
                stream.WriteStream(ms);
                return ms.GetBuffer();
            }
            return null;
        }
        public static IGraphWeight DeserializeWeight(byte[] bytes)
        {
            if (bytes != null)
            {
                XmlStream stream = new XmlStream("weight");
                MemoryStream ms = new MemoryStream(bytes);
                stream.ReadStream(ms);

                GraphWeight weight = new GraphWeight();
                weight.Load(stream);
                return weight;
            }
            return null;
        }

        public static BinaryWriter GetBinaryWriter()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            return writer;
        }
        public static void WriteWeight(BinaryWriter writer, IGraphWeight weight, double val)
        {
            switch (weight.DataType)
            {
                case GraphWeightDataType.Integer:
                    writer.Write(Convert.ToInt32(val));
                    break;
                case GraphWeightDataType.Double:
                    writer.Write(val);
                    break;
            }
        }
        public static byte[] GetBuffer(BinaryWriter writer)
        {
            if (writer.BaseStream is MemoryStream)
                return ((MemoryStream)writer.BaseStream).GetBuffer();
            return null;
        }

        public interface IWeightPage
        {
            double GetWeight(int index);
        }
        private class DoubleWeightPage : IWeightPage
        {
            double[] _weights = null;

            public double GetWeight(int index)
            {
                if (_weights == null)
                    return 0.0;

                if (index >= 0 && index < _weights.Length)
                    return _weights[index];

                return 0.0;
            }

            public void Deserialize(byte[] data)
            {
                if (data == null)
                {
                    _weights = null;
                    return;
                }

                BinaryReader br = new BinaryReader(new MemoryStream(data));
                _weights = new double[data.Length / sizeof(double)];
                for (int i = 0; i < _weights.Length; i++)
                    _weights[i] = br.ReadDouble();
            }
        }
        private class IntWeightPage : IWeightPage
        {
            int[] _weights = null;

            public double GetWeight(int index)
            {
                if (_weights == null)
                    return 0.0;

                if (index >= 0 && index < _weights.Length)
                    return _weights[index];

                return 0.0;
            }

            public void Deserialize(byte[] data)
            {
                if (data == null)
                {
                    _weights = null;
                    return;
                }

                BinaryReader br = new BinaryReader(new MemoryStream(data));
                _weights = new int[data.Length / sizeof(int)];
                for (int i = 0; i < _weights.Length; i++)
                    _weights[i] = br.ReadInt32();
            }
        }
        #endregion

        #region Switches
        public class SwitchPage
        {
            private List<ISwitch> _switches = new List<ISwitch>();
            private List<int> _nodeIds = new List<int>();

            public void Add(int nodeId, bool switchState)
            {
                int index = _nodeIds.BinarySearch(nodeId);
                if (index < 0)
                {
                    _nodeIds.Insert(~index, nodeId);
                    _switches.Add(new Switch(nodeId, switchState));
                }
            }

            public bool GetSwitchState(int nodeId)
            {
                int index = _nodeIds.BinarySearch(nodeId);
                if (index < 0)  // Node is not a switch!
                    return true;

                foreach (ISwitch s in _switches)
                    if (s.NodeId == nodeId)
                        return s.SwitchState;

                return false;
            }
        }
        #endregion

        #region NodeFcIds
        public class NodeFcIdsPage
        {
            private List<int> _fcIds = new List<int>();
            private List<int> _nodeIds = new List<int>();

            public void Add(int nodeId, int fcid)
            {
                int index = _nodeIds.BinarySearch(nodeId);
                if (index < 0)
                {
                    _nodeIds.Insert(~index, nodeId);
                    _fcIds.Insert(~index, fcid);
                }
            }

            public int GetNodeFcId(int nodeId)
            {
                int index = _nodeIds.BinarySearch(nodeId);
                if (index < 0)  // Node is not Fc Node!
                    return -1;

                return _fcIds[index];
            }
        }
        #endregion

        #region NodeTypes
        public class NodeTypesPage
        {
            private Dictionary<int, NetworkNodeType> _types = new Dictionary<int, NetworkNodeType>();
            private List<int> _nodeIds = new List<int>();

            public void Add(int nodeId, NetworkNodeType nodeType)
            {
                if (nodeType == NetworkNodeType.Unknown)
                    return;

                int index = _nodeIds.BinarySearch(nodeId);
                if (index < 0)
                {
                    _nodeIds.Insert(~index, nodeId);
                    _types.Add(nodeId, nodeType);
                }
            }

            public NetworkNodeType GetNodeType(int nodeId)
            {
                int index = _nodeIds.BinarySearch(nodeId);
                if (index < 0)  // Normal Node!
                    return NetworkNodeType.Unknown;

                return _types[nodeId];
            }
        }
        #endregion

        #region NetworkProperties
        public class NetworkProperties
        {
            private int _version = 1;
            private int _pageSize = 512;
            private double _tolerance = double.Epsilon;

            public NetworkProperties(int pageSize, double tolerance)
            {
                _pageSize = pageSize;
                _tolerance = tolerance;
            }

            #region Properties
            public int PageSize
            {
                get { return _pageSize; }
            }

            public double SnapTolerance
            {
                get { return _tolerance; }
            }
            #endregion

            public byte[] Serialize()
            {
                BinaryWriter writer = new BinaryWriter(new MemoryStream());

                writer.Write(_version);
                writer.Write(_pageSize);
                writer.Write(_tolerance);

                byte[] bytes = new byte[writer.BaseStream.Length];
                writer.BaseStream.Position = 0;
                writer.BaseStream.Read(bytes, (int)0, (int)writer.BaseStream.Length);
                writer.Close();

                return bytes;
            }

            public bool Deserialize(byte[] bytes)
            {
                try
                {
                    BinaryReader r = new BinaryReader(new MemoryStream());
                    r.BaseStream.Write(bytes, 0, (bytes.Length));
                    r.BaseStream.Position = 0;

                    _version = r.ReadInt32();

                    if (_version >= 1)
                    {
                        _pageSize = r.ReadInt32();
                        _tolerance = r.ReadDouble();
                    }

                    return true;
                }
                catch { return false; }
            }
        }
        #endregion

        #region NetworkClassProperties
        public class NetworkClassProperties
        {
            private int _version = 2;
            private bool _complexEdges = false;
            private bool _isSwitch = false;
            private string _switchFieldname = String.Empty;

            public NetworkClassProperties(bool complexEdges, bool isSwitch, string switchFieldname)
            {
                _complexEdges = complexEdges;
                _isSwitch = isSwitch;
                _switchFieldname = switchFieldname;
            }

            #region Properties
            public bool ComplexEdges
            {
                get { return _complexEdges; }
            }
            #endregion

            public byte[] Serialize()
            {
                BinaryWriter writer = new BinaryWriter(new MemoryStream());

                writer.Write(_version);
                writer.Write(_complexEdges);
                writer.Write(_isSwitch);
                NetworkObjectSerializer.SerializeString(writer, _switchFieldname);

                byte[] bytes = new byte[writer.BaseStream.Length];
                writer.BaseStream.Position = 0;
                writer.BaseStream.Read(bytes, (int)0, (int)writer.BaseStream.Length);
                writer.Close();

                return bytes;
            }

            public bool Deserialize(byte[] bytes)
            {
                try
                {
                    BinaryReader r = new BinaryReader(new MemoryStream());
                    r.BaseStream.Write(bytes, 0, (bytes.Length));
                    r.BaseStream.Position = 0;

                    _version = r.ReadInt32();

                    if (_version >= 1)
                    {
                        _complexEdges = r.ReadBoolean();
                    }
                    if (_version >= 2)
                    {
                        _isSwitch = r.ReadBoolean();
                        _switchFieldname = NetworkObjectSerializer.DeserializeString(r);
                    }

                    return true;
                }
                catch { return false; }
            }
        }
        #endregion

        #region Helper Classes
        public class PageManager
        {
            // Not Threadsafe !!!
            private Dictionary<int, EdgePage> _edgePages = new Dictionary<int, EdgePage>();
            private Dictionary<int, GraphPage> _graphPages = new Dictionary<int, GraphPage>();
            private Dictionary<int, SwitchPage> _switchPages = new Dictionary<int, SwitchPage>();
            private Dictionary<int, NodeFcIdsPage> _nodeFcIdsPages = new Dictionary<int, NodeFcIdsPage>();
            private Dictionary<int, NodeTypesPage> _nodeTypePages = new Dictionary<int, NodeTypesPage>();
            private DbProviderFactory _dbfactory = null;
            private string _networkName = String.Empty, _connectionString = String.Empty;
            private object _thisLock = new object();
            private string _graphTableName, _edgeTableName, _nodeTableName;
            private Dictionary<Guid, string> _weightTableNames;
            private Dictionary<Guid, GraphWeightDataType> _weightDataTypes;
            private Dictionary<Guid, Dictionary<int, IWeightPage>> _weightPagesDict = new Dictionary<Guid, Dictionary<int, IWeightPage>>();
            private IDatabaseNames _dbNames;

            public PageManager(DbProviderFactory dbfactory, string connectionStirng, string networkName,
                string graphTablename, string edgeTableName, string nodeTableName,
                Dictionary<Guid, string> weightTableNames, Dictionary<Guid, GraphWeightDataType> weightDataTypes, IDatabaseNames dbNames)
            {
                _dbfactory = dbfactory;
                _connectionString = connectionStirng;
                _networkName = networkName;

                _graphTableName = graphTablename;
                _edgeTableName = edgeTableName;
                _nodeTableName = nodeTableName;
                _weightTableNames = weightTableNames;
                _weightDataTypes = weightDataTypes;

                _dbNames = dbNames;
            }

            #region Methods
            public void ClearPages()
            {
                _edgePages.Clear();
                _graphPages.Clear();
            }

            #region Graph
            public GraphTableRows QueryN1(int n1)
            {
                GraphPage page = LoadGraphPage(NetworkObjectSerializer.Page(n1));
                if (page == null)
                    return new GraphTableRows();

                return page.QueryN1(n1);
            }

            private GraphPage LoadGraphPage(int page)
            {
                lock (_thisLock)
                {
                    if (_graphPages.ContainsKey(page))
                        return _graphPages[page];

                    if (_dbfactory != null)
                    {
                        try
                        {
                            using (DbConnection connection = _dbfactory.CreateConnection())
                            {
                                connection.ConnectionString = _connectionString;
                                DbCommand command = _dbfactory.CreateCommand();
                                command.Connection = connection;
                                command.CommandText = "SELECT " + _dbNames.DbColName("Data") + " FROM " + _graphTableName + " WHERE " + _dbNames.DbColName("Page") + "=" + page;
                                connection.Open();

                                DbDataReader reader = command.ExecuteReader();
                                if (reader.Read())
                                {
                                    object obj = reader.GetValue(0);
                                    GraphPage newGraphPage =
                                        NetworkObjectSerializer.DeserializeGraph((byte[])obj);
                                    _graphPages.Add(page, newGraphPage);
                                    connection.Close();

                                    return newGraphPage;
                                }
                                connection.Close();
                            }
                        }
                        catch { }
                    }
                    return null;
                }
            }
            #endregion

            #region Edges
            public IGraphEdge GetEdge(int eid)
            {
                EdgePage page = LoadEdgePage(NetworkObjectSerializer.Page(eid));
                if (page == null)
                    return null;

                return page.GetEdge(eid);
            }

            private EdgePage LoadEdgePage(int page)
            {
                lock (_thisLock)
                {
                    if (_edgePages.ContainsKey(page))
                        return _edgePages[page];

                    if (_dbfactory != null)
                    {
                        try
                        {
                            using (DbConnection connection = _dbfactory.CreateConnection())
                            {
                                connection.ConnectionString = _connectionString;
                                DbCommand command = _dbfactory.CreateCommand();
                                command.Connection = connection;
                                command.CommandText = "SELECT " + _dbNames.DbColName("Data") + " FROM " + _edgeTableName + " WHERE " + _dbNames.DbColName("Page") + "=" + page;
                                connection.Open();

                                DbDataReader reader = command.ExecuteReader();
                                if (reader.Read())
                                {
                                    object obj = reader.GetValue(0);
                                    connection.Close();

                                    EdgePage newEdgePage =
                                        NetworkObjectSerializer.DeserializeEdges((byte[])obj);
                                    _edgePages.Add(page, newEdgePage);
                                    return newEdgePage;
                                }

                                connection.Close();
                            }
                        }
                        catch { }
                    }
                    return null;
                }
            }
            #endregion

            #region Weights
            public double GetEdgeWeight(Guid guid, int eid)
            {
                int pageIndex = NetworkObjectSerializer.Page(eid);
                IWeightPage page = LoadWeightPage(guid, pageIndex);
                if (page == null)
                    return 0.0;


                // Page 0: Edge   1...512: weightIndex:   0..511 auf Page 1
                // Page 1: Edge 512..1024: weightIndex:   0..511 auf Page 2
                // ...                     ....

                // Beispiele:
                //    0-> weightIndex=(   1-1)-0*512=0
                //  512-> weightIndex=( 512-1)-0*512=511
                //  513-> weightIndex=( 513-1)-1*512=0
                // 1024-> weightIndex=(1024-1)-1*512=511
                // ...


                int weightIndex = (eid - 1) - pageIndex * NetworkObjectSerializer.PageSize;
                return page.GetWeight(weightIndex);
            }
            private IWeightPage LoadWeightPage(Guid guid, int page)
            {
                lock (_thisLock)
                {
                    if (!_weightPagesDict.ContainsKey(guid))
                        _weightPagesDict.Add(guid, new Dictionary<int, IWeightPage>());

                    Dictionary<int, IWeightPage> weightPages = _weightPagesDict[guid];
                    if (weightPages.ContainsKey(page))
                        return weightPages[page];

                    if (!_weightDataTypes.ContainsKey(guid) ||
                       !_weightTableNames.ContainsKey(guid))
                        return null;

                    if (_dbfactory != null)
                    {
                        try
                        {
                            using (DbConnection connection = _dbfactory.CreateConnection())
                            {
                                connection.ConnectionString = _connectionString;
                                DbCommand command = _dbfactory.CreateCommand();
                                command.Connection = connection;
                                command.CommandText = "SELECT " + _dbNames.DbColName("Data") + " FROM " + _weightTableNames[guid] + " WHERE " + _dbNames.DbColName("Page") + "=" + page;
                                connection.Open();

                                DbDataReader reader = command.ExecuteReader();
                                if (reader.Read())
                                {
                                    object obj = reader.GetValue(0);
                                    connection.Close();

                                    IWeightPage weightPage = null;
                                    switch (_weightDataTypes[guid])
                                    {
                                        case GraphWeightDataType.Double:
                                            weightPage = new DoubleWeightPage();
                                            ((DoubleWeightPage)weightPage).Deserialize((byte[])obj);
                                            break;
                                        case GraphWeightDataType.Integer:
                                            weightPage = new IntWeightPage();
                                            ((IntWeightPage)weightPage).Deserialize((byte[])obj);
                                            break;
                                    }
                                    return weightPage;
                                }

                                connection.Close();
                            }
                        }
                        catch { }
                    }
                    return null;
                }
            }
            #endregion

            #region Switches
            public bool GetSwitchState(int nodeId)
            {
                SwitchPage page = LoadSwitchPage(NetworkObjectSerializer.Page(nodeId));
                if (page == null)
                    return false;

                return page.GetSwitchState(nodeId);
            }
            private SwitchPage LoadSwitchPage(int page)
            {
                lock (_thisLock)
                {
                    if (_switchPages.ContainsKey(page))
                        return _switchPages[page];

                    if (_dbfactory != null)
                    {
                        try
                        {
                            int from = page * NetworkObjectSerializer.PageSize + 1;
                            int to = (page + 1) * NetworkObjectSerializer.PageSize;

                            using (DbConnection connection = _dbfactory.CreateConnection())
                            {
                                connection.ConnectionString = _connectionString;
                                DbCommand command = _dbfactory.CreateCommand();
                                command.Connection = connection;
                                command.CommandText = "select " + _dbNames.DbColName("FDB_OID") + "," + _dbNames.DbColName("STATE") + " from " + _nodeTableName + " where " + _dbNames.DbColName("SWITCH") + "=1 and " + _dbNames.DbColName("FDB_OID") + ">=" + from + " and " + _dbNames.DbColName("FDB_OID") + "<=" + to;
                                connection.Open();

                                DbDataAdapter adapter = _dbfactory.CreateDataAdapter();
                                adapter.SelectCommand = command;
                                DataTable tab = new DataTable();
                                adapter.Fill(tab);

                                SwitchPage switchPage = new SwitchPage();
                                _switchPages.Add(page, switchPage);
                                foreach (DataRow row in tab.Rows)
                                {
                                    switchPage.Add(Convert.ToInt32(row["FDB_OID"]), Convert.ToBoolean(row["STATE"]));
                                }
                                connection.Close();
                                return switchPage;
                            }
                        }
                        catch { }
                    }
                    return null;
                }
            }
            #endregion

            #region NodeFcIds
            public int GetNodeFcid(int nodeId)
            {
                NodeFcIdsPage page = LoadNodeFcIdPage(NetworkObjectSerializer.Page(nodeId));
                if (page == null)
                    return -1;

                return page.GetNodeFcId(nodeId);
            }
            private NodeFcIdsPage LoadNodeFcIdPage(int page)
            {
                lock (_thisLock)
                {
                    if (_nodeFcIdsPages.ContainsKey(page))
                        return _nodeFcIdsPages[page];

                    if (_dbfactory != null)
                    {
                        try
                        {
                            int from = page * NetworkObjectSerializer.PageSize + 1;
                            int to = (page + 1) * NetworkObjectSerializer.PageSize;

                            using (DbConnection connection = _dbfactory.CreateConnection())
                            {
                                connection.ConnectionString = _connectionString;
                                DbCommand command = _dbfactory.CreateCommand();
                                command.Connection = connection;
                                command.CommandText = "select " + _dbNames.DbColName("FDB_OID") + "," + _dbNames.DbColName("FCID") + " from " + _nodeTableName + " where " + _dbNames.DbColName("FCID") + ">-1 and " + _dbNames.DbColName("FDB_OID") + ">=" + from + " and " + _dbNames.DbColName("FDB_OID") + "<=" + to;
                                connection.Open();

                                DbDataAdapter adapter = _dbfactory.CreateDataAdapter();
                                adapter.SelectCommand = command;
                                DataTable tab = new DataTable();
                                adapter.Fill(tab);

                                NodeFcIdsPage nodeFcIdsPage = new NodeFcIdsPage();
                                foreach (DataRow row in tab.Rows)
                                {
                                    nodeFcIdsPage.Add(Convert.ToInt32(row["FDB_OID"]), Convert.ToInt32(row["FCID"]));
                                }
                                connection.Close();
                                _nodeFcIdsPages.Add(page, nodeFcIdsPage);

                                return nodeFcIdsPage;
                            }
                        }
                        catch { }
                    }
                    return null;
                }
            }
            #endregion

            #region NodeTypes
            public NetworkNodeType GetNodeType(int nodeId)
            {
                NodeTypesPage page = LoadNodeTypePage(NetworkObjectSerializer.Page(nodeId));
                if (page == null)
                    return NetworkNodeType.Unknown;

                return page.GetNodeType(nodeId);
            }
            private NodeTypesPage LoadNodeTypePage(int page)
            {
                lock (_thisLock)
                {
                    if (_nodeTypePages.ContainsKey(page))
                        return _nodeTypePages[page];

                    if (_dbfactory != null)
                    {
                        try
                        {
                            int from = page * NetworkObjectSerializer.PageSize + 1;
                            int to = (page + 1) * NetworkObjectSerializer.PageSize;

                            using (DbConnection connection = _dbfactory.CreateConnection())
                            {
                                connection.ConnectionString = _connectionString;
                                DbCommand command = _dbfactory.CreateCommand();
                                command.Connection = connection;
                                command.CommandText = "select " + _dbNames.DbColName("FDB_OID") + "," + _dbNames.DbColName("NODETYPE") + " from " + _nodeTableName + " where " + _dbNames.DbColName("NODETYPE") + ">0 and " + _dbNames.DbColName("FDB_OID") + ">=" + from + " and " + _dbNames.DbColName("FDB_OID") + "<=" + to;
                                connection.Open();

                                DbDataAdapter adapter = _dbfactory.CreateDataAdapter();
                                adapter.SelectCommand = command;
                                DataTable tab = new DataTable();
                                adapter.Fill(tab);

                                NodeTypesPage nodeTypesPage = new NodeTypesPage();
                                foreach (DataRow row in tab.Rows)
                                {
                                    nodeTypesPage.Add(Convert.ToInt32(row["FDB_OID"]), (NetworkNodeType)Convert.ToInt32(row["NODETYPE"]));
                                }
                                connection.Close();
                                _nodeTypePages.Add(page, nodeTypesPage);

                                return nodeTypesPage;
                            }
                        }
                        catch { }
                    }
                    return null;
                }
            }
            #endregion

            #endregion
        }
        #endregion

        #region Helpers
        public static void SerializeString(BinaryWriter writer, string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                writer.Write((int)0);
            }
            else
            {
                byte[] bytes = Encoding.Unicode.GetBytes(str);
                writer.Write((int)bytes.Length);
                writer.Write(bytes);
            }
        }
        public static string DeserializeString(BinaryReader reader)
        {
            try
            {
                int length = reader.ReadInt32();
                if (length <= 0)
                    return String.Empty;

                byte[] bytes = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    bytes[i] = reader.ReadByte();
                }

                return Encoding.Unicode.GetString(bytes);
            }
            catch
            {
                return String.Empty;
            }
        }
        #endregion
    }
}
