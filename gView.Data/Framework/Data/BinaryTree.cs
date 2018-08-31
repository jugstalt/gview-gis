using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.FDB;
using gView.Framework.Geometry;

namespace gView.Framework.Data
{
    /*
    public class BinaryTreeNode
    {
        private IEnvelope _bounds;
        private int _level;
        private uint _id;

        public BinaryTreeNode(IEnvelope bound)
            : this(null, 0)
        {
            _bounds = bound;
        }

        public BinaryTreeNode(BinaryTreeNode parent,int part)
        {
            if (parent == null)
            {
                _level = 0;
                _id = 1;
            }
            else
            {
                _level = parent._level + 1;
                _id = (parent._id << 1) + (uint)part;

                switch (_level % 2)
                {
                    case 0:
                        _bounds = SplitHoriz(parent._bounds, part);
                        break;
                    case 1:
                        _bounds = SplitVert(parent._bounds, part);
                        break;
                }
            }
        }

        public int Level
        {
            get { return _level; }
        }
        public uint ID
        {
            get { return _id; }
        }
        public IEnvelope Bounds
        {
            get { return _bounds; }
        }
        private IEnvelope SplitHoriz(IEnvelope envelope,int part)
        {
            double x1 = envelope.minx, y1 = envelope.miny * 0.5 + envelope.maxy * 0.5;
            double x2 = envelope.maxx, y2 = envelope.miny * 0.5 + envelope.maxy * 0.5;

            switch (part)
            {
                case 0:
                    return new Envelope(x1, y1, x2, envelope.maxy);
                case 1:
                    return new Envelope(x1, envelope.miny, x2, y2);
            }
            return null;
        }

        private IEnvelope SplitVert(IEnvelope envelope, int part)
        {
            double x1 = envelope.minx * 0.5 + envelope.maxx * 0.5, y1 = envelope.miny;
            double x2 = envelope.minx * 0.5 + envelope.maxx * 0.5, y2 = envelope.maxy;

            switch (part)
            {
                case 0:
                    return new Envelope(envelope.miny, y1, x2, y2);
                case 1:
                    return new Envelope(x1, y1, envelope.maxx, y2);
            }
            return null;
        }
    }

    public class BinaryTree : IIndexTree
    {
        private IEnvelope _bounds;
        private int _depth;
        private BinaryTreeNode _root;
        private List<BinaryTreeNode> _nodes = new List<BinaryTreeNode>();

        public BinaryTree(IEnvelope boundary, int depth)
        {
            _bounds = boundary;
            _depth = depth;

            _root = new BinaryTreeNode(_bounds);
        }

        private void Split(BinaryTreeNode node)
        {
            BinaryTreeNode n1 = new BinaryTreeNode(node, 0);
            BinaryTreeNode n2 = new BinaryTreeNode(node, 1);
            _nodes.Add(n1);
            _nodes.Add(n2);
            if (n1.Level == _depth) return;

            Split(n1);
            Split(n2);
        }

        public void Calculate()
        {
            Split(_root);
        }

        public List<BinaryTreeNode> Nodes(int level)
        {
            List<BinaryTreeNode> nodes = new List<BinaryTreeNode>();

            foreach (BinaryTreeNode node in _nodes)
            {
                if (node.Level == level) nodes.Add(node);
            }
            nodes.Sort(new BinaryTreeNodeComparer());
            return nodes;
        }
        #region IIndexTree Member

        public List<int> FindShapeIds(gView.Framework.Geometry.IEnvelope Bounds)
        {
            return null;
        }

        #endregion
    }

    public class BinaryTreeNodeComparer : IComparer<BinaryTreeNode>
    {
        #region IComparer<BinaryTreeNode> Member

        public int Compare(BinaryTreeNode x, BinaryTreeNode y)
        {
            IEnvelope env1 = x.Bounds, env2 = y.Bounds;

            if (env1.miny == env2.miny)
            {
                if (env1.minx < env2.minx) return -1;
                if (env1.minx > env2.minx) return 1;
            }
            if (env1.miny < env2.miny) return -1;
            if (env1.miny > env2.miny) return 1;

            return 0;
        }

        #endregion
    }
     * */
}
