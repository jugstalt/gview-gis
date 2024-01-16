using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;

namespace gView.Framework.Network.Build
{
    public class NetworkNodes
    {
        private GridIndex<List<NetworkNode>> _grid;
        private int _nodeIdSequence = 0;
        private Dictionary<int, NetworkNode> _nodeIndex = new();

        public NetworkNodes(IEnvelope bounds)
        {
            _grid = new GridIndex<List<NetworkNode>>(bounds, 200);
        }

        public int Add(IPoint p, double tolerance)
        {
            if (p == null)
            {
                return -1;
            }

            int index = IndexOf(p, tolerance);
            if (index != -1)
            {
                return index;
            }

            int gridIndex = _grid.XYIndex(p);
            List<NetworkNode> nodes = _grid[gridIndex];
            if (nodes == null)
            {
                nodes = new List<NetworkNode>();
                _grid[gridIndex] = nodes;
            }

            NetworkNode node = new NetworkNode(++_nodeIdSequence, p);

            nodes.Add(node);
            _nodeIndex[node.Id] = node;

            return node.Id;
        }
        public NetworkNode Find(IPoint p, double tolerance)
        {
            if (p == null)
            {
                return null;
            }

            Envelope env = new Envelope(p.X - tolerance, p.Y - tolerance, p.X + tolerance, p.Y + tolerance);
            foreach (int index in _grid.XYIndices(env))
            {
                List<NetworkNode> nodes = _grid[index];
                if (nodes == null)
                {
                    continue;
                }

                foreach (NetworkNode node in nodes)
                {
                    double dx = node.Point.X - p.X;
                    if (Math.Abs(dx) > tolerance)
                    {
                        continue;
                    }

                    double dy = node.Point.Y - p.Y;
                    if (Math.Abs(dy) > tolerance)
                    {
                        continue;
                    }

                    if (node.Point.Equals(p, tolerance))
                    {
                        return node;
                    }
                }
            }
            return null;
        }
        public int IndexOf(IPoint p, double tolerance)
        {
            Envelope env = new Envelope(p.X - tolerance, p.Y - tolerance, p.X + tolerance, p.Y + tolerance);

            foreach (int index in _grid.XYIndices(env))
            {
                List<NetworkNode> nodes = _grid[index];
                if (nodes == null)
                {
                    continue;
                }

                foreach (NetworkNode node in nodes)
                {
                    double dx = node.Point.X - p.X;
                    if (Math.Abs(dx) > tolerance)
                    {
                        continue;
                    }

                    double dy = node.Point.Y - p.Y;
                    if (Math.Abs(dy) > tolerance)
                    {
                        continue;
                    }

                    if (node.Point.Equals(p, tolerance))
                    {
                        return node.Id;
                    }
                }
            }
            return -1;
        }

        public List<NetworkNode> ToList()
        {
            List<NetworkNode> list = new List<NetworkNode>();

            foreach (List<NetworkNode> t in _grid.AllCells)
            {
                list.AddRange(t);
            }
            list.Sort(new NodeIdComparer());
            return list;
        }

        public NetworkNode this[int nodeId]
        {
            get
            {
                if(_nodeIndex.TryGetValue(nodeId, out var node))
                {
                    return node;
                }

                //foreach (List<NetworkNode> t in _grid.AllCells)
                //{
                //    foreach (NetworkNode node in t)
                //    {
                //        if (node.Id == nodeId)
                //        {
                //            return node;
                //        }
                //    }
                //}

                return null;
            }
        }

        #region Comparer
        public class NodeIdComparer : IComparer<NetworkNode>
        {
            #region IComparer<NetworkGraphEdge> Member

            int IComparer<NetworkNode>.Compare(NetworkNode x, NetworkNode y)
            {
                if (x.Id < y.Id)
                {
                    return -1;
                }
                else if (x.Id > y.Id)
                {
                    return 1;
                }

                return 0;
            }

            #endregion
        }
        #endregion
    }

    public class NetworkNodes_old //: List<NetworkNode>
    {
        private Dictionary<long, List<NetworkNode>> _NIDs;
        private Dictionary<int, NetworkNode> _nodeIndex = new();
        private BinarySearchTree2 _tree;
        private int _nodeIdSequence = 0;

        public NetworkNodes_old(IEnvelope bounds)
        {
            _tree = new BinarySearchTree2(bounds, 32, 200, 0.55, null);
            _NIDs = new Dictionary<long, List<NetworkNode>>();
            _NIDs.Add((long)0, new List<NetworkNode>());
        }
        public int Add(IPoint p, double tolerance)
        {
            if (p == null)
            {
                return -1;
            }

            int index = IndexOf(p, tolerance);
            if (index != -1)
            {
                return index;
            }

            long nid = _tree.InsertSINodeFast(p.Envelope);
            if (!_NIDs.ContainsKey(nid))
            {
                _NIDs.Add(nid, new List<NetworkNode>());
            }

            List<NetworkNode> nodes = _NIDs[nid];
            NetworkNode node = new NetworkNode(++_nodeIdSequence, p);

            nodes.Add(node);
            _nodeIndex.Add(node.Id, node);

            return node.Id;
        }

        public NetworkNode this[int nodeId]
        {
            get
            {
                if (_nodeIndex.ContainsKey(nodeId))
                {
                    return _nodeIndex[nodeId];
                }

                return null;
            }
        }

        public NetworkNode Find(IPoint p, double tolerance)
        {
            if (p == null)
            {
                return null;
            }

            Envelope env = new Envelope(p.X - tolerance, p.Y - tolerance, p.X + tolerance, p.Y + tolerance);
            List<long> nids = _tree.CollectNIDs(env);

            foreach (long nid in nids)
            {
                List<NetworkNode> nodes = _NIDs[nid];
                foreach (NetworkNode node in nodes)
                {
                    if (node.Point.Equals(p, tolerance))
                    {
                        return node;
                    }
                }
            }

            return null;
        }

        public int IndexOf(IPoint p, double tolerance)
        {
            Envelope env = new Envelope(p.X - tolerance, p.Y - tolerance, p.X + tolerance, p.Y + tolerance);
            List<long> nids = _tree.CollectNIDs(env);

            foreach (long nid in nids)
            {
                int index = IndexOf(nid, p, tolerance);
                if (index != -1)
                {
                    return index;
                }
            }
            return -1;
        }

        public int IndexOf(long nid, IPoint p, double tolerance)
        {
            if (!_NIDs.ContainsKey(nid))
            {
                return -1;
            }

            List<NetworkNode> nodes = _NIDs[nid];
            foreach (NetworkNode node in nodes)
            {
                double dx = node.Point.X - p.X;
                if (Math.Abs(dx) > tolerance)
                {
                    continue;
                }

                double dy = node.Point.Y - p.Y;
                if (Math.Abs(dy) > tolerance)
                {
                    continue;
                }

                if (node.Point.Equals(p, tolerance))
                {
                    return node.Id;
                }
            }

            return -1;
        }

        public List<NetworkNode> ToList()
        {
            List<NetworkNode> list = new List<NetworkNode>();
            foreach (long nid in _NIDs.Keys)
            {
                List<NetworkNode> nodes = _NIDs[nid];
                foreach (NetworkNode node in nodes)
                {
                    list.Add(node);
                }
            }
            list.Sort(new NodeIdComparer());
            return list;
        }

        #region Comparer
        public class NodeIdComparer : IComparer<NetworkNode>
        {
            #region IComparer<NetworkGraphEdge> Member

            int IComparer<NetworkNode>.Compare(NetworkNode x, NetworkNode y)
            {
                if (x.Id < y.Id)
                {
                    return -1;
                }
                else if (x.Id > y.Id)
                {
                    return 1;
                }

                return 0;
            }

            #endregion
        }
        #endregion
    }
}
