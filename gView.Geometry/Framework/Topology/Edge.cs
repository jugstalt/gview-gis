using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Topology
{
    public class Edge : IEquatable<Edge>
    {
        /// <summary>
        /// Start of edge index
        /// </summary>
        public int p1;
        /// <summary>
        /// End of edge index
        /// </summary>
        public int p2;
        /// <summary>
        /// Initializes a new edge instance
        /// </summary>
        /// <param name="point1">Start edge vertex index</param>
        /// <param name="point2">End edge vertex index</param>
        public Edge(int point1, int point2)
        {
            p1 = point1; p2 = point2;
        }
        /// <summary>
        /// Initializes a new edge instance with start/end indexes of '0'
        /// </summary>
        public Edge()
            : this(0, 0)
        {
        }

        #region IEquatable<dEdge> Members

        /// <summary>
        /// Checks whether two edges are equal disregarding the direction of the edges
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Edge other)
        {
            return
                ((this.p1 == other.p2) && (this.p2 == other.p1)) ||
                ((this.p1 == other.p1) && (this.p2 == other.p2));
        }

        #endregion
    }

    public class Edges : List<Edge>
    {
        new public bool Contains(Edge edge)
        {
            foreach (Edge e in this)
            {
                if (e.Equals(edge))
                    return true;
            }
            return false;
        }

        new public void Sort()
        {
            base.Sort(new Comparer());
        }

        private class Comparer : IComparer<Edge>
        {
            #region IComparer<Edge> Member

            public int Compare(Edge x, Edge y)
            {
                int ret = x.p1.CompareTo(y.p1);
                if (ret == 0)
                    ret = x.p2.CompareTo(y.p2);
                return ret;
            }

            #endregion
        }
    }
}
