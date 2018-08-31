using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.Network;

namespace gView.Framework.Network.Algorthm
{
    public class Dijkstra
    {
        private Nodes _dNodes = null;
        private ICancelTracker _cancelTracker = null;
        private List<int> _allowedNodeIds = null;
        private List<int> _forbiddenTargetNodeIds = null;
        private List<int> _barrierNodeIds = null;
        private List<int> _forbiddenStartNodeEdgeIds = null;
        private List<int> _forbiddenEdgeIds = null;
        private List<int> _targetNodeFcIds = null;
        private NetworkNodeType? _targetNodeType = null;

        private double _maxDistance = double.MaxValue;
        private IGraphWeight _weight = null;
        private WeightApplying _weightApplying = WeightApplying.Weight;
        private bool _applySwitchState = false;

        public event ProgressReporterEvent reportProgress = null;

        #region Contructors
        public Dijkstra()
        {
            Init();
        }
        public Dijkstra(ICancelTracker cancelTracker)
            : this()
        {
            _cancelTracker = cancelTracker;
        }
        public Dijkstra(Nodes initialNodes)
        {
            if (initialNodes != null)
            {
                _dNodes = initialNodes.Clone();
                _dNodes.Sort(new Nodes.NodeIdComparer());
            }
            else
                Init();
        }
        public Dijkstra(Nodes initialNodes, ICancelTracker cancelTracker)
            : this(initialNodes)
        {
            _cancelTracker = cancelTracker;
        }
        #endregion

        public void Init()
        {
            _dNodes = new Nodes();
        }

        #region Calculate
        public bool Calculate(GraphTable graph, int fromNode)
        {
            return Calculate(graph, fromNode, -1);
        }
        public bool Calculate(GraphTable graph, int fromNode, int toNode)
        {
            //_dNodes.Add(new Node(fromNode, 0.0));
            Node startNode = _dNodes.ByIdOrAdd(fromNode);
            if (double.IsNaN(startNode.Dist))
                startNode.Dist = 0.0;

            if (_barrierNodeIds != null && _barrierNodeIds.BinarySearch(fromNode) >= 0)
                return true;

            bool found = false;
            int nodeId = fromNode;
            DistanceNodeStack distStack = new DistanceNodeStack();
            distStack.Add(startNode);

            #region Progress
            ProgressReport report = new ProgressReport();
            report.Message = "Network Calculations...";
            report.featureMax = 1000;
            report.featurePos = 0;
            #endregion

            while (true)
            {
                //Node minDistNode = _dNodes.MinDistNode();
                Node minDistNode = distStack.Pop();
                if (minDistNode == null)  // Keine Knoten mehr -> Ziel ist nicht ereichbar!
                    break;
                if (minDistNode.Id == toNode) // Ziel mit kürzesten Weg erreicht!
                {
                    found = true;
                    break;
                }
                minDistNode.Used = true;
                if (minDistNode.Dist >= _maxDistance)
                    continue;

                GraphTableRows graphRows = graph.QueryN1(minDistNode.Id);

                foreach (GraphTableRow graphRow in graphRows)
                {
                    if (_applySwitchState && graph.SwitchState(graphRow.N1) == false)
                        continue;

                    if (_targetNodeFcIds != null && graphRow.N1 != fromNode &&
                        _targetNodeFcIds.Contains(graph.GetNodeFcid(graphRow.N1)))
                    {
                        if (_targetNodeType != null)
                        {
                            NetworkNodeType nodeType = graph.GetNodeType(graphRow.N1);
                            if (nodeType == (NetworkNodeType)_targetNodeType)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (_allowedNodeIds != null && _allowedNodeIds.BinarySearch(graphRow.N2) < 0)
                        continue;
                    if (_forbiddenTargetNodeIds != null && _forbiddenTargetNodeIds.BinarySearch(graphRow.N2) >= 0)
                        continue;
                    if (_forbiddenEdgeIds != null && _forbiddenEdgeIds.BinarySearch(graphRow.EID) >= 0)
                        continue;
                    if (_forbiddenStartNodeEdgeIds != null && graphRow.N1 == fromNode && _forbiddenStartNodeEdgeIds.BinarySearch(graphRow.EID) >= 0)
                        continue;
                    if (_barrierNodeIds != null && _barrierNodeIds.BinarySearch(graphRow.N2) >= 0)
                        continue;

                    Node n = _dNodes.ByIdOrAdd(graphRow.N2);

                    double length = graphRow.LENGTH;
                    if (_weight != null)
                    {
                        double weight = graph.QueryEdgeWeight(_weight.Guid, graphRow.EID);
                        switch (_weightApplying)
                        {
                            case WeightApplying.Weight:
                                length /= weight;
                                break;
                            case WeightApplying.ActualCosts:
                                length = weight;
                                break;
                        }
                    }
                    double dist = minDistNode.Dist + length;
                    double geodist = minDistNode.GeoDist + graphRow.LENGTH;

                    if (double.IsNaN(n.Dist) || n.Dist > dist)
                    {
                        if (!double.IsNaN(n.Dist))
                            distStack.Remove(n);

                        n.Dist = dist;
                        n.GeoDist = geodist;
                        n.Pre = minDistNode.Id;
                        n.EId = graphRow.EID;
                        n.Used = false;

                        distStack.Add(n);
                    }
                }

                if (_cancelTracker != null && _cancelTracker.Continue == false)
                    return false;

                if (reportProgress != null && _dNodes.Count % 20 == 0)
                {
                    report.featurePos += 20;
                    if (report.featurePos > report.featureMax)
                        //    report.featurePos = 0;
                        report.featureMax += 1000;
                    reportProgress(report);
                }
            }

            return found;
        }
        #endregion

        #region Result Members
        public Nodes DijkstraNodes
        {
            get { return _dNodes; }
        }
        public Nodes DijkstraNodesDistanceGreaterThan(double distance)
        {
            Nodes nodes = new Nodes();
            foreach (Node node in _dNodes)
            {
                if (node.Dist > distance)
                    nodes.Add(node);
            }
            return nodes;
        }
        public Nodes DijkstraNodesWithMaxDistance(double distance)
        {
            Nodes nodes = new Nodes();
            foreach (Node node in _dNodes)
            {
                if (node.Dist <= distance)
                    nodes.Add(node);
            }
            return nodes;
        }

        public Nodes DijkstraEndNodes
        {
            get
            {
                Nodes nodes = new Nodes();
                //foreach (Node node in _dNodes)
                //{
                //    bool isEnd = true;
                //    foreach (Node n in _dNodes)
                //    {
                //        if (n.Pre == node.Id)
                //        {
                //            isEnd = false;
                //            break;
                //        }
                //    }
                //    if (isEnd)
                //        nodes.Add(node);
                //}

                CalcEndNodes();
                foreach (Node node in _dNodes)
                {
                    if (node.IsEndNode)
                        nodes.Add(node);
                }
                return nodes;
            }
        }

        private void CalcEndNodes()
        {
            foreach (Node node in _dNodes)
                node.IsEndNode = true;

            foreach (Node node in _dNodes)
            {
                Node pre = _dNodes.ById(node.Pre);
                if (pre != null)
                    pre.IsEndNode = false;
            }
        }
        public double DijkstraNodeDistance(int nodeId)
        {
            Node node = _dNodes.ById(nodeId);
            if (node == null)
                return double.NaN;

            return node.Dist;
        }

        public NetworkPath DijkstraPath(int endNodeId)
        {
            NetworkPath path = new NetworkPath();
            if (_dNodes == null)
                return path;
            Node endNode = _dNodes.ById(endNodeId);
            if (endNode == null)
                return path;

            while (endNode != null)
            {
                if (endNode.EId > 0)
                    path.Insert(0, new NetworkPathEdge(endNode.EId));
                endNode = _dNodes.ById(endNode.Pre);
            }

            return path;
        }

        public NetworkPath DijkstraPath(Nodes nodes)
        {
            NetworkPath path = new NetworkPath(true);
            if (nodes == null || _dNodes == null)
                return path;

            foreach (Node node in nodes)
            {
                Node endNode = _dNodes.ById(node.Id);

                while (endNode != null)
                {
                    if (endNode.EId > 0)
                        path.Insert(0, new NetworkPathEdge(endNode.EId));
                    endNode = _dNodes.ById(endNode.Pre);
                }
            }
            return path;
        }
        public Nodes DijkstraPathNodes(int endNodeId)
        {
            Nodes nodes = new Nodes();
            if (_dNodes == null)
                return nodes;
            Node endNode = _dNodes.ById(endNodeId);
            if (endNode == null)
                return nodes;

            while (endNode != null)
            {
                nodes.Insert(0, endNode);
                endNode = _dNodes.ById(endNode.Pre);
            }

            return nodes;
        }
        #endregion

        #region Properties
        public List<int> AllowedNodeIds
        {
            get { return _allowedNodeIds; }
            set
            {
                if (_allowedNodeIds != value)
                {
                    _allowedNodeIds = value;
                    if (_allowedNodeIds != null)
                        _allowedNodeIds.Sort();
                }
            }
        }
        public List<int> ForbiddenTargetNodeIds
        {
            get { return _forbiddenTargetNodeIds; }
            set
            {
                if (_forbiddenTargetNodeIds != value)
                {
                    _forbiddenTargetNodeIds = value;
                    if (_forbiddenTargetNodeIds != null)
                        _forbiddenTargetNodeIds.Sort();
                }
            }
        }
        public List<int> ForbiddenStartNodeEdgeIds
        {
            get { return _forbiddenStartNodeEdgeIds; }
            set
            {
                if (_forbiddenStartNodeEdgeIds != value)
                {
                    _forbiddenStartNodeEdgeIds = value;
                    if (_forbiddenStartNodeEdgeIds != null)
                        _forbiddenStartNodeEdgeIds.Sort();
                }
            }
        }
        public List<int> ForbiddenEdgeIds
        {
            get { return _forbiddenEdgeIds; }
            set
            {
                if (_forbiddenEdgeIds != value)
                {
                    _forbiddenEdgeIds = value;
                    if (_forbiddenEdgeIds != null)
                        _forbiddenEdgeIds.Sort();
                }
            }
        }
        public List<int> BarrierNodeIds
        {
            get
            {
                return _barrierNodeIds;
            }
            set
            {
                _barrierNodeIds = value;
                if (_barrierNodeIds != null)
                    _barrierNodeIds.Sort();
            }
        }
        public double MaxDistance
        {
            get { return _maxDistance; }
            set { _maxDistance = value; }
        }
        public IGraphWeight GraphWeight
        {
            get { return _weight; }
            set { _weight = value; }
        }
        public WeightApplying WeightApplying
        {
            get { return _weightApplying; }
            set { _weightApplying = value; }
        }
        public bool ApplySwitchState
        {
            get { return _applySwitchState; }
            set { _applySwitchState = value; }
        }
        public List<int> TargetNodeFcIds
        {
            get { return _targetNodeFcIds; }
            set { _targetNodeFcIds = value; }
        }
        public NetworkNodeType? TargetNodeType
        {
            get { return _targetNodeType; }
            set { _targetNodeType = value; }
        }
        #endregion

        #region NodeClasses
        public class Node
        {
            private int _id;
            private int _pre = -1, _eid = -1;
            private double _dist = double.NaN, _geodist = 0.0;
            private bool _used = false;
            private bool _isEndNode = false;

            public Node(int id)
            {
                _id = id;
            }
            public Node(int id, double dist)
                : this(id)
            {
                _dist = dist;
            }

            public int Id { get { return _id; } set { _id = value; } }
            public int Pre { get { return _pre; } set { _pre = value; } }
            public int EId { get { return _eid; } set { _eid = value; } }
            public double Dist { get { return _dist; } set { _dist = value; } }
            public double GeoDist { get { return _geodist; } set { _geodist = value; } }
            public bool Used { get { return _used; } set { _used = value; } }
            public bool IsEndNode { get { return _isEndNode; } set { _isEndNode = value; } }
        }
        public class Nodes : List<Node>
        {
            private Node _compNode = new Node(0);
            private NodeIdComparer _idComparer = new NodeIdComparer();

            internal Node MinDistNode()
            {
                double dist = double.MaxValue;
                Node ret = null;

                foreach (Node node in this)
                {
                    if (node.Used == false && node.Dist < dist)
                    {
                        ret = node;
                        dist = node.Dist;
                    }
                }
                return ret;
            }

            public Node ByIdOrAdd(int id)
            {
                _compNode.Id = id;
                int index = this.BinarySearch(_compNode, _idComparer);

                if (index < 0)
                {
                    Node newNode = new Node(id);
                    this.Insert(~index, newNode);
                    return newNode;
                }
                return this[index];
            }

            public Node ById(int id)
            {
                _compNode.Id = id;
                int index = this.BinarySearch(_compNode, _idComparer);
                if (index < 0)
                    return null;
                return this[index];
                //if (id == -1)
                //    return null;

                //foreach (Node node in this)
                //{
                //    if (node.Id == id)
                //        return node;
                //}
                //return null;
            }

            public List<int> IdsToList()
            {
                List<int> ids = new List<int>();
                foreach (Node node in this)
                    ids.Add(node.Id);
                return ids;
            }

            internal Nodes Clone()
            {
                Nodes clone = new Nodes();
                foreach (Node node in this)
                {
                    clone.Add(node);
                }
                return clone;
            }

            #region Comparer
            internal class NodeIdComparer : IComparer<Node>
            {
                #region IComparer<Node> Member

                public int Compare(Node x, Node y)
                {
                    if (x.Id < y.Id)
                        return -1;
                    else if (x.Id > y.Id)
                        return 1;
                    return 0;
                }

                #endregion
            }
            #endregion
        }

        public class NetworkPathEdge
        {
            private int _eid;
            private double _t = 1.0;

            public NetworkPathEdge(int eid)
            {
                _eid = eid;
            }

            public int EId { get { return _eid; } }
        }

        public class NetworkPath : List<NetworkPathEdge>
        {
            private bool _unique = false;

            public NetworkPath()
            {
            }
            public NetworkPath(bool uniqueNodeIds)
            {
                _unique = uniqueNodeIds;
            }
            public void Swap()
            {

            }
            new public void Add(NetworkPathEdge edge)
            {
                if (edge == null)
                    return;

                if (_unique == true)
                {
                    if (Contains(edge.EId))
                        return;
                }
                base.Add(edge);
            }
            new public void Insert(int index, NetworkPathEdge edge)
            {
                if (edge == null)
                    return;

                if (_unique == true)
                {
                    if (Contains(edge.EId))
                        return;
                }
                base.Insert(index, edge);
            }
            public bool Contains(int edgeId)
            {
                foreach (NetworkPathEdge edge in this)
                    if (edge.EId == edgeId)
                        return true;

                return false;
            }
        }

        public class DistanceNodeStack : List<Node>
        {
            private NodeDistanceComparer _comparer = new NodeDistanceComparer();

            new public void Add(Node node)
            {
                int index = this.BinarySearch(node, _comparer);
                if (index >= 0)
                {
                    if (this[index].Id != node.Id)
                        this.Insert(index, node);
                }
                else
                {
                    this.Insert(~index, node);
                }
            }

            public Node Pop()
            {
                if (this.Count == 0)
                    return null;

                Node n = this[this.Count - 1];
                this.RemoveAt(this.Count - 1);
                return n;
            }

            #region Comparer
            private class NodeDistanceComparer : IComparer<Node>
            {
                #region IComparer<Node> Member

                public int Compare(Node x, Node y)
                {
                    if (x.Dist < y.Dist)
                        return 1;
                    else if (x.Dist > y.Dist)
                        return -1;
                    return 0;
                }

                #endregion
            }
            #endregion
        }
        #endregion

        public static void ApplyInputIds(Dijkstra dijkstra, NetworkTracerInputCollection inputCollection)
        {
            if (dijkstra == null || inputCollection == null)
                return;

            foreach (INetworkTracerInput input in inputCollection)
            {
                if (input is NetworkInputAllowedNodeIds)
                    dijkstra.AllowedNodeIds = ((NetworkInputAllowedNodeIds)input).Ids;

                else if (input is NetworkInputForbiddenTargetNodeIds)
                    dijkstra.ForbiddenTargetNodeIds = ((NetworkInputForbiddenTargetNodeIds)input).Ids;

                else if (input is NetworkInputForbiddenStartNodeEdgeIds)
                    dijkstra.ForbiddenStartNodeEdgeIds = ((NetworkInputForbiddenStartNodeEdgeIds)input).Ids;

                else if (input is NetworkInputForbiddenEdgeIds)
                    dijkstra.ForbiddenEdgeIds = ((NetworkInputForbiddenEdgeIds)input).Ids;

                else if (input is NetworkBarrierNodeInput)
                {
                    if (dijkstra._barrierNodeIds == null)
                        dijkstra._barrierNodeIds = new List<int>();
                    dijkstra.BarrierNodeIds.Add(((NetworkBarrierNodeInput)input).NodeId);
                }
            }

            if (dijkstra._barrierNodeIds != null)
                dijkstra._barrierNodeIds.Sort();
        }
    }
}
