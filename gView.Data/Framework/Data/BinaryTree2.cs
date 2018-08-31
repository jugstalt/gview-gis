using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.system;

namespace gView.Framework.Data
{
    public class BinaryTree2
    {
        protected Envelope _bounds;
        protected int _maxLevels = 30;
        protected int _maxPerNode = 200;
        protected double SPLIT_RATIO = 0.55;

        private long[] _levelNodeCount;

        public BinaryTree2(IEnvelope envelope)
        {
            _bounds = new Envelope(envelope);

            InitLevelNodeCount();
        }

        public BinaryTree2(IEnvelope envelope, int maxLevels, int maxPerNode)
        {
            _bounds = new Envelope(envelope);

            _maxLevels = maxLevels;
            _maxPerNode = maxPerNode;

            InitLevelNodeCount();
        }
        public BinaryTree2(IEnvelope envelope, int maxLevels, int maxPerNode, double split_ratio)
            : this(envelope, maxLevels, maxPerNode)
        {
            SPLIT_RATIO = split_ratio;
        }

        #region Properties
        public IEnvelope Bounds
        {
            get { return _bounds; }
        }
        public double SplitRatio
        {
            get { return SPLIT_RATIO; }
        }
        public int MaxPerNode
        {
            get { return _maxPerNode; }
            set { _maxPerNode = value; }
        }
        public int maxLevels
        {
            get { return _maxLevels; }
        }
        #endregion


        public long maxNodeNumber
        {
            get
            {
                return (long)Math.Pow(2, maxLevels + 1) - 2;
            }
        }

        public void ChildNodeNumbers(long nodeNumber, out long child1, out long child2)
        {
            ChildNodeNumbers(nodeNumber, NodeLevel(nodeNumber), out child1, out child2);
        }

        public void ChildNodeNumbers(long nodeNumber, int level, out long child1, out long child2)
        {
            if (level >= _maxLevels) // sollte eigentlich >= sein!!!
            {
                throw new Exception("Can't calculate childnode numbers...\nLevel>=MaxLevel");
            }
            else
            {
                child1 = (long)nodeNumber + 1;
                child2 = (long)nodeNumber + (long)Math.Pow(2, _maxLevels - level);
            }
        }

        public int NodeLevel(long nodeNumber)
        {
            return NodeLevel(0, 0, nodeNumber);
        }

        protected int NodeLevel(long n, int l, long x)
        {
            if (n == x) return l;

            long c1, c2;
            ChildNodeNumbers(n, l, out c1, out c2);
            if (x < c2)
            {
                return NodeLevel(c1, l + 1, x);
            }
            return NodeLevel(c2, l + 1, x);
        }

        public IEnvelope this[long nodeNumber]
        {
            get
            {
                if (nodeNumber > maxNodeNumber) return _bounds;
                return NodeEnvelope(0, 0, _bounds, nodeNumber);
            }
        }

        protected IEnvelope NodeEnvelope(long n, int l, IEnvelope envelope, long x)
        {
            if (n == x) return envelope;

            long c1, c2;
            ChildNodeNumbers(n, l, out c1, out c2);
            if (x < c2)
            {
                return NodeEnvelope(c1, l + 1, SplitEnvelope(envelope, true), x);
            }
            return NodeEnvelope(c2, l + 1, SplitEnvelope(envelope, false), x);
        }

        protected IEnvelope SplitEnvelope(IEnvelope Bounds, bool first)
        {
            double w = Bounds.Width;
            double h = Bounds.Height;
            double minx, miny, maxx, maxy;

            if (w > h)
            {
                if (first)
                {
                    minx = Bounds.minx; maxx = minx + w * SPLIT_RATIO;
                }
                else
                {
                    minx = Bounds.maxx - w * SPLIT_RATIO; maxx = Bounds.maxx;
                }
                miny = Bounds.miny; maxy = miny + h;
            }
            else
            {
                minx = Bounds.minx; maxx = minx + w;
                if (first)
                {
                    miny = Bounds.miny; maxy = miny + h * SPLIT_RATIO;
                }
                else
                {
                    miny = Bounds.maxy - h * SPLIT_RATIO; maxy = Bounds.maxy;
                }
            }
            return new Envelope(minx, miny, maxx, maxy);
        }

        private void InitLevelNodeCount()
        {
            _levelNodeCount = new long[_maxLevels + 1];
            _levelNodeCount[0] = 1;

            for (int i = 1; i <= _maxLevels; i++)
            {
                _levelNodeCount[i] = _levelNodeCount[i - 1] * 2;
            }
            //for (int i = 1; i <= _maxLevels; i++)
            //    _levelNodeCount[i] += _levelNodeCount[i - 1];
        }

        protected long ReachableNodes(long n)
        {
            int l = NodeLevel(n);
            return ReachableNodes(n, l);
        }
        protected long ReachableNodes(long n, int l)
        {
            long r = 0;// _levelNodeCount[_maxLevels - l /*+ 1*/];
            for (int i = 0; i < _maxLevels - l + 1; i++)
            {
                r += _levelNodeCount[i];
            }
            r -= 1;
            return n + r;
        }

        protected bool IsLeftFrom(long n, long from)
        {
            long c1, c2;
            ChildNodeNumbers(from, out c1, out c2);

            return (n < c2);
        }

        protected long TransformNodeNumber(long n, int targetMaxLevels)
        {
            long x1 = 0, x2 = 0;
            int nodeLevel = this.NodeLevel(n);
            if (nodeLevel > targetMaxLevels)
            {
                throw new Exception("Can't tramsform nodenumber.\nNodelevel>=targetMaxLevels");
            }

            for (int l = 0; l < nodeLevel; l++)
            {
                if (IsLeftFrom(n, x1))
                {
                    x1 += 1;
                    x2 += 1;
                }
                else
                {
                    x1 += (long)Math.Pow(2, _maxLevels - l);
                    x2 += (long)Math.Pow(2, targetMaxLevels - l);
                }
            }
            return x2;
        }
    }

    internal class FeatureEnvelope
    {
        private int _OID;
        private IEnvelope _envelope;

        public FeatureEnvelope(IFeature feature)
        {
            _OID = feature.OID;
            _envelope = feature.Shape.Envelope;
        }

        public int OID
        {
            get { return _OID; }
        }
        public IEnvelope Envelope
        {
            get { return _envelope; }
        }
    }

    public class BinaryTree2BuilderNode
    {
        private long _nodeNumber;
        private int _nodeLevel;
        private List<int> _ids;

        internal BinaryTree2BuilderNode(BinaryTree2Builder tree, long nodeNumber, List<FeatureEnvelope> list)
        {
            _nodeNumber = nodeNumber;
            _nodeLevel = tree.NodeLevel(nodeNumber);

            _ids = new List<int>();
            foreach (FeatureEnvelope env in list)
            {
                _ids.Add(env.OID);
            }
        }

        public long Number
        {
            get { return _nodeNumber; }
        }
        public int Level
        {
            get { return _nodeLevel; }
        }
        public List<int> OIDs
        {
            get { return _ids; }
        }
    }

    class BinaryTree2BuilderNodeCompareByNumber : IComparer<BinaryTree2BuilderNode>
    {
        #region IComparer<BinaryTree2BuilderNode> Member

        public int Compare(BinaryTree2BuilderNode x, BinaryTree2BuilderNode y)
        {
            if (x.Number < y.Number) return -1;
            if (x.Number > y.Number) return 1;
            return 0;
        }

        #endregion
    }
    class BinaryTree2BuilderNodeCompareByLevel : IComparer<BinaryTree2BuilderNode>
    {
        #region IComparer<BinaryTree2BuilderNode> Member

        public int Compare(BinaryTree2BuilderNode x, BinaryTree2BuilderNode y)
        {
            if (x.Level < y.Level) return -1;
            if (x.Level > y.Level) return 1;
            return 0;
        }

        #endregion
    }

    class BinaryTree2BuilderNodeCompareByFeatureCount : IComparer<BinaryTree2BuilderNode>
    {
        #region IComparer<BinaryTree2BuilderNode> Member

        public int Compare(BinaryTree2BuilderNode x, BinaryTree2BuilderNode y)
        {
            if (x.OIDs.Count < y.OIDs.Count) return -1;
            if (x.OIDs.Count > y.OIDs.Count) return 1;
            return 0;
        }

        #endregion
    }

    public class BinaryTree2Builder : BinaryTree2
    {
        internal Dictionary<long, List<FeatureEnvelope>> _nodes = null;

        public BinaryTree2Builder(IEnvelope envelope)
            : base(envelope)
        {
        }
        public BinaryTree2Builder(IEnvelope envelope, int maxLevels, int maxPerNode)
            : base(envelope, maxLevels, maxPerNode)
        {
        }
        public BinaryTree2Builder(IEnvelope envelope, int maxLevels, int maxPerNode, double split_ratio)
            : base(envelope, maxLevels, maxPerNode, split_ratio)
        {
        }


        public bool AddFeature(IFeature feature)
        {
            if (feature == null || feature.Shape == null || feature.Shape.Envelope == null) return false;

            if (_nodes == null)
            {
                _nodes = new Dictionary<long, List<FeatureEnvelope>>();
                _nodes.Add(0, new List<FeatureEnvelope>());
            }
            return AddFeature(0, 0, new FeatureEnvelope(feature));
        }

        virtual internal bool AddFeature(long n, int l, FeatureEnvelope featEnv)
        {
            List<FeatureEnvelope> list;
            //
            // Überprüfen, ob Key schon vorhanden ist
            //
            if (!_nodes.TryGetValue(n, out list)) return false;

            if (l < _maxLevels)
            {
                //
                //  Kindknoten
                //
                long c1, c2;
                this.ChildNodeNumbers(n, l, out c1, out c2);

                List<FeatureEnvelope> cList;
                if (_nodes.TryGetValue(c1, out cList) &&
                    _nodes.TryGetValue(c2, out cList))
                {
                    //
                    // Bounds des Knoten berechen
                    //
                    IEnvelope bound1 = this[c1];
                    IEnvelope bound2 = this[c2];
                    if (bound1 != null && bound1.Contains(featEnv.Envelope))
                    {
                        return AddFeature(c1, l + 1, featEnv);
                    }
                    else if (bound2 != null && bound2.Contains(featEnv.Envelope))
                    {
                        return AddFeature(c2, l + 1, featEnv);
                    }
                }
            }
            list.Add(featEnv);
            if (list.Count > _maxPerNode && l < _maxLevels)
            {
                SplitNode(n, l);
            }
            return true;
        }

        private void SplitNode(long n, int l)
        {
            List<FeatureEnvelope> list;
            if (!_nodes.TryGetValue(n, out list)) return;

            //
            //  Kindknoten
            //
            long c1, c2;
            this.ChildNodeNumbers(n, l, out c1, out c2);

            List<FeatureEnvelope> cList;
            if (!_nodes.TryGetValue(c1, out cList))
            {
                _nodes.Add(c1, new List<FeatureEnvelope>());
            }
            if (!_nodes.TryGetValue(c2, out cList))
            {
                _nodes.Add(c2, new List<FeatureEnvelope>());
            }

            //
            // Bounds des Knoten berechen
            //
            IEnvelope bound1 = this[c1];
            IEnvelope bound2 = this[c2];

            foreach (FeatureEnvelope env in gView.Framework.system.ListOperations<FeatureEnvelope>.Clone(list))
            {
                if (bound1 != null && bound1.Contains(env.Envelope))
                {
                    list.Remove(env);
                    AddFeature(c1, l + 1, env);
                }
                else if (bound2 != null && bound2.Contains(env.Envelope))
                {
                    list.Remove(env);
                    AddFeature(c2, l + 1, env);
                }
            }
            list = null;
        }

        public List<BinaryTree2BuilderNode> Nodes
        {
            get
            {
                List<BinaryTree2BuilderNode> nodes = new List<BinaryTree2BuilderNode>();
                if (_nodes == null) return nodes;
                foreach (long n in _nodes.Keys)
                {
                    if (_nodes[n].Count == 0) continue;
                    nodes.Add(new BinaryTree2BuilderNode(this, n, _nodes[n]));
                }

                nodes.Sort(new BinaryTree2BuilderNodeCompareByNumber());
                return nodes;
            }
        }

        private int MaxLevel
        {
            get
            {
                int maxLevel = 0;
                foreach (long nodeNumber in _nodes.Keys)
                {
                    maxLevel = Math.Max(this.NodeLevel(nodeNumber), maxLevel);
                }
                return maxLevel;
            }
        }

        virtual public void Trim()
        {
            if (_nodes == null) return;
            int targetMaxLevels = this.MaxLevel;

            Dictionary<long, List<FeatureEnvelope>> nodes = new Dictionary<long, List<FeatureEnvelope>>();
            foreach (long nodeNumber in _nodes.Keys)
            {
                long transformed = this.TransformNodeNumber(nodeNumber, targetMaxLevels);

                List<FeatureEnvelope> features = _nodes[nodeNumber];
                nodes.Add(transformed, features);
            }

            _nodes = nodes;
            _maxLevels = targetMaxLevels;
        }
    }

    /// <summary>
    /// BinaryTree2Builder mit fixer Vorgabe für Bound, Levels, ...
    /// Hier werden nur Features gesammelt, um Sie dann Nodeweise zu schreiben
    /// </summary>
    public class BinaryTree2Builder2 : BinaryTree2Builder
    {
        private BinarySearchTree2 _searchTree;

        public BinaryTree2Builder2(IEnvelope envelope, int maxLevels, int maxPerNode)
            : base(envelope, maxLevels, maxPerNode)
        {
            _searchTree = new BinarySearchTree2(envelope, maxLevels, maxPerNode, base.SPLIT_RATIO, null);
        }
        public BinaryTree2Builder2(IEnvelope envelope, int maxLevels, int maxPerNode, double split_ratio)
            : base(envelope, maxLevels, maxPerNode, split_ratio)
        {
            _searchTree = new BinarySearchTree2(envelope, maxLevels, maxPerNode, split_ratio, null);
        }

        override internal bool AddFeature(long n, int l, FeatureEnvelope featEnv)
        {
            n = _searchTree.InsertSINode(featEnv.Envelope);

            List<FeatureEnvelope> list;
            if (!_nodes.TryGetValue(n, out list))
            {
                list = new List<FeatureEnvelope>();
                _nodes.Add(n, list);
            }
            list.Add(featEnv);

            return true;
        }

        public override void Trim()
        {
            // hier nix tun
        }
    }

    public class BinarySearchTree2 : BinaryTree2, ISearchTree
    {
        public delegate void TreeNodeAddedEventHander(object sender, long nid);
        public event TreeNodeAddedEventHander TreeNodeAdded = null;

        private List<long> _nodeNumbers = null;
        private object lockThis = new object();
        private int _treeVersion = 0;

        public BinarySearchTree2(IEnvelope bound, int maxLevels, int maxPerNode, double split_ratio, List<long> nodeNumbers)
            : base(bound, maxLevels, maxPerNode, split_ratio)
        {
            if (nodeNumbers != null)
            {
                _nodeNumbers = nodeNumbers;
            }
            else
            {
                _nodeNumbers = new List<long>();
            }
            //if (_nodeNumbers.Count == 0) _nodeNumbers.Add(0);
        }

        public int IndexVersion
        {
            get { return _treeVersion; }
            set { _treeVersion = value; }
        }

        private class NodeNumbers
        {
            internal List<long> _nn;
            int Index = -1;

            public NodeNumbers(List<long> nn)
            {
                _nn = nn;
            }

            public long Value
            {
                get { return (Index < 0 ? 0 : (Index < _nn.Count ? _nn[Index] : -1)); }
            }
            public void Inc()
            {
                Index++;
            }
            public bool SetTo(long number)
            {
                int i = _nn.BinarySearch(number);
                if (i < 0 || i < Index) return false;

                Index = i;
                return true;
            }
            public bool Contains(long val)
            {
                return _nn.BinarySearch(val) > -1;
            }
            public bool Contains(long from, long to)
            {
                if (from == to)
                    return Contains(from);

                return _nn.BinarySearch(from) != _nn.BinarySearch(to);
                //for (long i = from; i <= to; i++)
                //    if (Contains(i))
                //        return true;
                //return false;
            }
            public bool Cancel
            {
                get { return Index >= _nn.Count; }
            }
        }

        private void Collect(long FromNodeNumber, long ToNodeNumber, NodeNumbers nn, List<long> ids, bool checkDuplicates)
        {
            //if (actIndex >= _nodeNumbers.Count) return _nodeNumbers.Count;
            //long number = _nodeNumbers[actIndex];
            if (nn.Cancel) return;
            long number = nn.Value;

            while (number <= ToNodeNumber)
            {
                if (number >= FromNodeNumber && number <= ToNodeNumber)
                {
                    if (checkDuplicates)
                    {
                        if (ids.BinarySearch(number) < 0)
                            ids.Add(number);
                    }
                    else
                    {
                        ids.Add(number);
                    }
                }
                //actIndex++;
                //if (actIndex >= _nodeNumbers.Count) return _nodeNumbers.Count;
                //number = _nodeNumbers[actIndex];
                nn.Inc();
                if (nn.Cancel) return;
                number = nn.Value;
            }
            //return actIndex;
        }

        //private List<long> history;
        private void Collect(long number, int level, IEnvelope rect, IEnvelope NodeEnvelope, NodeNumbers nn, List<long> ids, bool checkDuplicates)
        {
            //history.Add(number);
            long reachable = ReachableNodes(number, level); //(long)(number + (long)Math.Pow(2, _maxLevels - level + 1) - 2);
            if (nn.Cancel || nn.Value > reachable) return;

            if (!rect.Intersects(NodeEnvelope)) return;

            //int nodeIndex = _nodeNumbers.BinarySearch(number);
            //if (nodeIndex > -1 && ids.BinarySearch(number) < 0) ids.Add(number);
            int added = 0;
            if (nn.SetTo(number))
            {
                if (checkDuplicates)
                {
                    if (ids.BinarySearch(number) < 0) ids.Add(number);
                }
                else
                {
                    ids.Add(number);
                    added = 1;
                }
            }
            if (rect.Contains(NodeEnvelope))
            {
                if (level <= _maxLevels)
                {
                    Collect(number + added, reachable, nn, ids, checkDuplicates);
                    return;
                }
            }

            if (level < _maxLevels)
            {
                long c1, c2;
                ChildNodeNumbers(number, level, out c1, out c2);

                Collect(c1, level + 1, rect, this.SplitEnvelope(NodeEnvelope, true), nn, ids, checkDuplicates);
                Collect(c2, level + 1, rect, this.SplitEnvelope(NodeEnvelope, false), nn, ids, checkDuplicates);
            }
        }

        private void CollectPlus(long number, int level, IEnvelope rect, IEnvelope NodeEnvelope, NodeNumbers nn, List<long> ids)
        {
            long reachable = ReachableNodes(number, level); //(long)(number + (long)Math.Pow(2, _maxLevels - level + 1) - 2);
            if (nn.Cancel || nn.Value > reachable) 
                return;  // ???

            if (!rect.Intersects(NodeEnvelope)) return;

            if (!nn.Contains(number, reachable))
                return;

            if (number > 0 && rect.Contains(NodeEnvelope))
            {
                int index = ids.IndexOf(number - 1);
                if (index > 0 && ids[index - 1] < 0)
                {
                    // Optimierung:
                    // voriger knoten schon mit Between abgefragt wird ->
                    // Vorigen Between gleich mit neuem Reachable Wert versehen -> weniger Statements!!
                    ids[index] = reachable;
                }
                else
                {
                    ids.Add(-number);
                    ids.Add(reachable);
                }
                return;
            }

            if (nn.SetTo(number))
            {
                ids.Add(number);
            }

            if (level < _maxLevels)
            {
                long c1, c2;
                ChildNodeNumbers(number, level, out c1, out c2);

                CollectPlus(c1, level + 1, rect, this.SplitEnvelope(NodeEnvelope, true), nn, ids);
                CollectPlus(c2, level + 1, rect, this.SplitEnvelope(NodeEnvelope, false), nn, ids);
            }
        }

        private void ContainerNodeNumber(IEnvelope FeatureEnvelope, NodeNumbers nn, ref long containerNode)
        {
            //if (nn.Cancel) return;
            //int level = NodeLevel(nn.Value);
            //long reachable = (long)(nn.Value + (long)Math.Pow(2, _maxLevels - level + 1) - 2);
            //if (nn.Value > reachable) return;

            //IEnvelope env = this[nn.Value];
            //if (env.Contains(FeatureEnvelope))
            //{
            //    containerNode = nn.Value;
            //    nn.Inc();
            //    ContainerNodeNumber(FeatureEnvelope, nn, ref containerNode);
            //}
            //else
            //{
            //    while (nn.Value <= reachable)
            //    {
            //        nn.Inc();
            //        if (nn.Cancel) return;
            //    }
            //    ContainerNodeNumber(FeatureEnvelope, nn, ref containerNode);
            //}

            long reachable = ReachableNodes(0, 0); //(long)(Math.Pow(2, _maxLevels + 1) - 2);
            while (!nn.Cancel)
            {
                if (nn.Value > reachable)
                    return;

                int level = NodeLevel(nn.Value);
                long r = ReachableNodes(nn.Value, level); //(long)(nn.Value + (long)Math.Pow(2, _maxLevels - level + 1) - 2);

                IEnvelope env = this[nn.Value];

                if (env.Contains(FeatureEnvelope))
                {
                    reachable = r;
                    containerNode = nn.Value;
                }
                else
                {
                    while (nn.Value <= r)
                    {
                        nn.Inc();
                        if (nn.Cancel) return;
                    }
                }
                nn.Inc();
            }
        }

        public long ContainerNodeNumber(IEnvelope FeatureEnvelope)
        {
            NodeNumbers nn = new NodeNumbers(_nodeNumbers);

            long containerNode = 0;
            ContainerNodeNumber(FeatureEnvelope, nn, ref containerNode);
            return containerNode;
        }

        public bool SplitNode(long nodeNumber)
        {
            if (!_nodeNumbers.Contains(nodeNumber)) return false;

            int level = NodeLevel(nodeNumber);
            if (level >= _maxLevels) return false;

            lock (lockThis)
            {
                long c1, c2;
                ChildNodeNumbers(nodeNumber, level, out c1, out c2);
                if (!_nodeNumbers.Contains(c1)) _nodeNumbers.Add(c1);
                if (!_nodeNumbers.Contains(c2)) _nodeNumbers.Add(c2);

                _nodeNumbers.Sort();
            }
            return true;
        }

        public void AddNodeNumber(long number)
        {
            lock (lockThis)
            {
                int index = _nodeNumbers.BinarySearch(number);
                if (index >= 0) return;

                //_nodeNumbers.Add(number);
                _nodeNumbers.Insert(~index, number);
                if (TreeNodeAdded != null) TreeNodeAdded(this, number);
            }
        }

        public long InsertSINode(IEnvelope geometryEnvelope)
        {
            if (geometryEnvelope == null) return 0;

            long nodeNumber = this.ContainerNodeNumber(geometryEnvelope);
            IEnvelope nodeEnvelope = this[nodeNumber];

            int level = this.NodeLevel(nodeNumber);
            bool found = false;
            while (level <= this.maxLevels - 1)
            {
                long c1, c2;
                this.ChildNodeNumbers(nodeNumber, level, out c1, out c2);
                level++;

                IEnvelope envC1 = this.SplitEnvelope(nodeEnvelope, true);
                IEnvelope envC2 = this.SplitEnvelope(nodeEnvelope, false);
                if (envC1.Contains(geometryEnvelope))
                {
                    found = true;
                    nodeNumber = c1;
                    nodeEnvelope = envC1;
                }
                else if (envC2.Contains(geometryEnvelope))
                {
                    found = true;
                    nodeNumber = c2;
                    nodeEnvelope = envC2;
                }
                else
                {
                    break;
                }
            }

            if (found || nodeNumber == 0) this.AddNodeNumber(nodeNumber);
            return nodeNumber;
        }
        public long InsertSINodeFast(IEnvelope geometryEnvelope)
        {
            //return InsertSINode(geometryEnvelope);

            if (geometryEnvelope == null) return 0;

            IEnvelope nodeEnvelope = this.Bounds;
            long nodeNumber = 0;
            int level = 0;
            while (level <= this.maxLevels - 1)
            {
                long c1, c2;
                this.ChildNodeNumbers(nodeNumber, level, out c1, out c2);
                level++;

                IEnvelope envC1 = this.SplitEnvelope(nodeEnvelope, true);
                IEnvelope envC2 = this.SplitEnvelope(nodeEnvelope, false);
                if (envC1.Contains(geometryEnvelope))
                {
                    nodeNumber = c1;
                    nodeEnvelope = envC1;
                }
                else if (envC2.Contains(geometryEnvelope))
                {
                    nodeNumber = c2;
                    nodeEnvelope = envC2;
                }
                else
                {
                    break;
                }
            }

            return nodeNumber;
        }

        public long UpdadeSINode(IEnvelope geometryEnvelope, long nid)
        {
            if (geometryEnvelope == null) return 0;

            IEnvelope nodeEnvelope = this[nid];
            if (nodeEnvelope.Contains(geometryEnvelope))
            {
                this.AddNodeNumber(nid);
                return nid;
            }

            return InsertSINode(geometryEnvelope);
        }

        public long UpdadeSINodeFast(IEnvelope geometryEnvelope, long nid)
        {
            if (geometryEnvelope == null) return 0;

            IEnvelope nodeEnvelope = this[nid];
            if (nid != 0 &&
                nid <= maxNodeNumber &&
                nodeEnvelope.Contains(geometryEnvelope))
            {
                return nid;
            }

            return InsertSINode(geometryEnvelope);
        }

        #region ISearchTree Member

        public List<long> CollectNIDs(IGeometry geometry)
        {
            if (geometry is IEnvelope)
            {
                // Schnelle Suche für Envelope (GetMap)
                return CollectNIDs(geometry as IEnvelope);
            }
            // sonst ausführliche Suche

            // 
            //  Envelopes der Geometrie Teile bestimmen
            //
            List<IEnvelope> envelops = gView.Framework.SpatialAlgorithms.Algorithm.PartEnvelops(geometry);

            List<long> ids = new List<long>();
            //if (_nodeNumbers.BinarySearch(0) > -1) ids.Add(0);

            //
            //  IDs für die Envelopes suchen
            //
            foreach (IEnvelope envelope in envelops)
            {
                if (envelops == null) continue;
                NodeNumbers nn = new NodeNumbers(_nodeNumbers);
                Collect(0, 0, envelope, _bounds, nn, ids, true);
                ids.Sort();
            }

            // 
            //  IDs wieder entfernen, die sich nicht mit der Geometrie schneiden
            //
            foreach (long id in ListOperations<long>.Clone(ids))
            {
                IEnvelope bound = this[id];
                if (!SpatialAlgorithms.Algorithm.IntersectBox(geometry, bound))
                {
                    ids.Remove(id);
                }
            }

            if (ids.Count == 0)
                ids.Add(0);
            else if (ids.IndexOf(0) == -1 && _nodeNumbers.BinarySearch(0) > -1) ids.Add(0);

            return ids;
        }

        private List<long> CollectNIDs(IEnvelope bounds)
        {
            List<long> ids = new List<long>();
            //if (_nodeNumbers.BinarySearch(0) > -1) ids.Add(0);

            NodeNumbers nn = new NodeNumbers(_nodeNumbers);
            //history = new List<long>();
            Collect(0, 0, bounds, _bounds, nn, ids, false);
            if (ids.Count == 0)
                ids.Add(0);
            else if (ids.IndexOf(0) == -1 && _nodeNumbers.BinarySearch(0) > -1) ids.Add(0);

            return ids;
        }

        public List<long> CollectNIDsPlus(IEnvelope envelope)
        {
            // envelope fully contains bounds -> all nodes -> return Zero!
            if (envelope == null || envelope.Contains(_bounds))
                return null;

            List<long> ids = new List<long>();
            NodeNumbers nn = new NodeNumbers(_nodeNumbers);
            
            // Query Envelope is outside bounds
            if (!envelope.Intersects(_bounds))
            {
                if (nn.Contains(0))  // if features outside bounds exists -> add zero node
                    ids.Add(0);
                
                return ids;
            }
            
            CollectPlus(0, 0, envelope, _bounds, nn, ids);
            return ids;
        }
        #endregion
    }

    public class BinaryTreeDef
    {
        private IEnvelope _bounds = null;
        private int _maxLevel = 30, _maxPerNode = 200;
        private double _split_radio = 0.55;
        private ISpatialReference _sRef = null;

        public BinaryTreeDef() { }

        public BinaryTreeDef(IEnvelope bounds)
        {
            _bounds = bounds;
        }
        public BinaryTreeDef(IEnvelope bounds, int maxLevel)
            : this(bounds)
        {
            _maxLevel = maxLevel;
        }
        public BinaryTreeDef(IEnvelope bounds, int maxLevel, int maxPerNode)
            : this(bounds, maxLevel)
        {
            _maxPerNode = maxPerNode;
        }
        public BinaryTreeDef(IEnvelope bounds, int maxLevel, int maxPerNode, double split_ration)
            : this(bounds, maxLevel, maxPerNode)
        {
            _split_radio = split_ration;
        }

        public Envelope Bounds
        {
            get { return ((_bounds == null) ? null : new Envelope(_bounds)); }
        }
        public int MaxLevel
        {
            get { return _maxLevel; }
        }
        public int MaxPerNode
        {
            get { return _maxPerNode; }
        }
        public double SplitRatio
        {
            get { return _split_radio; }
        }
        public ISpatialReference SpatialReference
        {
            get { return _sRef; }
            set { _sRef = value; }
        }
        public bool ProjectTo(ISpatialReference sRef)
        {
            if (_bounds == null) return false;

            if (_sRef != null && !_sRef.Equals(sRef))
            {
                IGeometry result = GeometricTransformer.Transform2D(_bounds, _sRef, sRef);
                if (result != null && result.Envelope != null)
                {
                    _bounds = result.Envelope;
                    _sRef = sRef;
                    return true;
                }
            }
            return true;
        }
        public override string ToString()
        {
            IFormatProvider nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
            if (_bounds == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(_bounds.minx.ToString(nhi) + ";");
            sb.Append(_bounds.miny.ToString(nhi) + ";");
            sb.Append(_bounds.maxx.ToString(nhi) + ";");
            sb.Append(_bounds.maxy.ToString(nhi) + ";");
            sb.Append(_maxLevel.ToString() + ";");
            sb.Append(_maxPerNode.ToString() + ";");
            sb.Append(_split_radio.ToString(nhi));

            return sb.ToString();
        }

        public static BinaryTreeDef FromString(string str)
        {
            try
            {
                string[] s = str.Split(';');

                if (s.Length != 7) return null;

                IFormatProvider nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

                IEnvelope bounds = new Envelope(
                    double.Parse(s[0], nhi),
                    double.Parse(s[1], nhi),
                    double.Parse(s[2], nhi),
                    double.Parse(s[3], nhi));
                int maxLevel = int.Parse(s[4]);
                int maxPerNode = int.Parse(s[5]);
                double splitRatio = double.Parse(s[6], nhi);

                return new BinaryTreeDef(bounds, maxLevel, maxPerNode, splitRatio);
            }
            catch
            {
                return null;
            }
        }
    }

    public interface IImplementsBinarayTreeDef
    {
        BinaryTreeDef BinaryTreeDef(string fcname);
    }
}
