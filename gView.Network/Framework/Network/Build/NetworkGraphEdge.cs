using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;

namespace gView.Framework.Network.Build
{
    class NetworkGraphEdge
    {
        private int _fromNodeIndex = -1, _toNodeIndex = -1, _edgeIndex = -1;
        private double _length, _geolength;
        public NetworkGraphEdge(int fromNode, int toNode, int edge, double length, double geolength)
        {
            _fromNodeIndex = fromNode;
            _toNodeIndex = toNode;
            _edgeIndex = edge;

            _length = length;
            _geolength = geolength;
        }

        #region Properties
        public int FromNodeIndex
        {
            get { return _fromNodeIndex; }
        }
        public int ToNodeIndex
        {
            get { return _toNodeIndex; }
        }
        public int EdgeIndex
        {
            get { return _edgeIndex; }
        }
        public double Length
        {
            get { return _length; }
        }
        public double GeoLength
        {
            get { return _geolength; }
        }
        #endregion
    }

    class NetworkGraphEdges : List<NetworkGraphEdge>
    {
        public void RemoveDoubles()
        {
            List<string> couples = new List<string>();
            foreach (NetworkGraphEdge graphEdge in ListOperations<NetworkGraphEdge>.Clone(this))
            {
                string s = graphEdge.FromNodeIndex + "-" + graphEdge.ToNodeIndex + "-" + graphEdge.EdgeIndex;
                if (couples.Contains(s))
                {
                    this.Remove(graphEdge);
                }
                else
                {
                    couples.Add(s);
                }
            }
        }

        public class NodeIndexComparer : IComparer<NetworkGraphEdge>
        {
            #region IComparer<NetworkGraphEdge> Member

            int IComparer<NetworkGraphEdge>.Compare(NetworkGraphEdge x, NetworkGraphEdge y)
            {
                if (x.FromNodeIndex < y.FromNodeIndex)
                    return -1;
                else if (x.FromNodeIndex > y.FromNodeIndex)
                    return 1;

                // bei gleichstand, nach ToNodeIndex Sortieren
                if (x.ToNodeIndex < y.ToNodeIndex)
                    return -1;
                else if (x.ToNodeIndex > y.ToNodeIndex)
                    return 1;

                return 0;
            }

            #endregion
        }

    }
}
