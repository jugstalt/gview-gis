using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using gView.Framework.Data;
using System.Collections;

namespace gView.Framework.Network.Build
{
    public class NetworkEdge
    {
        private int _id;
        private IPath _path;
        private double _length = 0.0, _geoLength = 0.0;
        private bool _oneway = false;
        private int _fcId = -1;
        private int _featureId = -1;
        private int _fromNodeIndex = -1, _toNodeIndex = -1;
        private bool _isComplex = false;
        private bool _useWithComplexEdges = false;
        private IEnvelope _bounds = null;
        private Hashtable _weights = null;

        #region Properties

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public IPath Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public IPoint FromPoint
        {
            get
            {
                if (_path == null)
                    return null;
                return _path[0];
            }
        }

        public IPoint ToPoint
        {
            get
            {
                if (_path == null)
                    return null;
                return _path[_path.PointCount - 1];
            }
        }

        public double Length
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
            }
        }

        public double GeoLength
        {
            get
            {
                return _geoLength;
            }
            set
            {
                _geoLength = value;
            }
        }

        public bool OneWay
        {
            get { return _oneway; }
            set { _oneway = value; }
        }

        public int FeatureclassId
        {
            get { return _fcId; }
            set { _fcId = value; }
        }

        public int FeatureId
        {
            get { return _featureId; }
            set { _featureId = value; }
        }

        public int FromNodeIndex
        {
            get { return _fromNodeIndex; }
            set { _fromNodeIndex = value; }
        }
        public int ToNodeIndex
        {
            get { return _toNodeIndex; }
            set { _toNodeIndex = value; }
        }

        public bool IsComplex
        {
            get { return _isComplex; }
            set { _isComplex = value; }
        }

        public bool UseWithComplexEdges
        {
            get { return _useWithComplexEdges; }
            set
            {
                _useWithComplexEdges = value;
                if (value == true && _path != null)
                    _bounds = _path.Envelope;
            }
        }

        public Hashtable Weights
        {
            get { return _weights; }
            set { _weights = value; }
        }

        public IEnvelope Bounds
        {
            get { return _bounds; }
        }
        #endregion
    }

    public class NetworkEdges : List<NetworkEdge>
    {
        private GridArray<List<NetworkEdge>> _gridArray = null;
        private int _edgeIdSequence = 0;

        public NetworkEdges()
        {
        }

        public NetworkEdges(IEnvelope bounds)
        {
            if (bounds != null)
            {
                _gridArray = new GridArray<List<NetworkEdge>>(bounds,
                                                               new int[] { 200, 180, 160, 130, 100, 70, 50, 25, 18, 10, 5, 2 },
                                                               new int[] { 200, 180, 160, 130, 100, 70, 50, 25, 18, 10, 5, 2 });
            }
        }

        public NetworkEdges(IPolyline polyline)
        {
            if (polyline == null || polyline.PathCount == 0)
                return;

            bool isComplex = polyline.PathCount > 1;
            for (int i = 0; i < polyline.PathCount; i++)
            {
                IPath path = polyline[i];
                if (path == null || path.PointCount < 2)
                    continue;

                double geolength = path.Length;
                if (geolength == 0.0)
                    continue;

                NetworkEdge edge = new NetworkEdge();
                edge.Path = path;
                edge.Length = edge.GeoLength = geolength;
                edge.IsComplex = isComplex;

                if (edge.FromPoint == null || edge.ToPoint == null)
                    continue;

                this.Add(edge);
            }
        }

        new public void Add(NetworkEdge edge)
        {
            edge.Id = ++_edgeIdSequence;
            base.Add(edge);

            if (edge.UseWithComplexEdges && _gridArray != null)
            {
                List<NetworkEdge> indexedEdges = _gridArray[edge.Bounds];
                indexedEdges.Add(edge);
            }
        }

        public NetworkEdges SelectFrom(int fromNodeIndex)
        {
            NetworkEdges edges = new NetworkEdges();
            foreach (NetworkEdge edge in this)
            {
                if (edge.FromNodeIndex == fromNodeIndex)
                    edges.Add(edge);
            }
            edges.Sort(new SortToNodeIndex());
            return edges;
        }
        public NetworkEdges SelectTo(int toNodeIndex)
        {
            NetworkEdges edges = new NetworkEdges();
            foreach (NetworkEdge edge in this)
            {
                if (edge.ToNodeIndex == toNodeIndex)
                    edges.Add(edge);
            }
            edges.Sort(new SortFromNodeIndex());
            return edges;
        }

        public NetworkEdges Collect(IEnvelope env)
        {
            NetworkEdges edges = new NetworkEdges();

            if (_gridArray != null)
            {
                foreach (List<NetworkEdge> e in _gridArray.Collect(env))
                {
                    edges.AddRange(e);
                }
            }
            return edges;
        }

        #region Comparer
        private class SortToNodeIndex : IComparer<NetworkEdge>
        {
            #region IComparer<NetworkEdge> Member

            public int Compare(NetworkEdge x, NetworkEdge y)
            {
                if (x.ToNodeIndex < y.ToNodeIndex)
                    return -1;
                else if (x.ToNodeIndex > y.ToNodeIndex)
                    return 1;
                return 0;
            }

            #endregion
        }
        private class SortFromNodeIndex : IComparer<NetworkEdge>
        {
            #region IComparer<NetworkEdge> Member

            public int Compare(NetworkEdge x, NetworkEdge y)
            {
                if (x.FromNodeIndex < y.FromNodeIndex)
                    return -1;
                else if (x.FromNodeIndex > y.FromNodeIndex)
                    return 1;
                return 0;
            }

            #endregion
        }
        #endregion
    }
}
