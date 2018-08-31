using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Topology
{
    public class PlanarGraph
    {
        Edges _edges = new Edges();
        public PlanarGraph(Nodes nodes, Edges edges)
        {
            _edges = new Edges();
            foreach (Edge edge in edges)
            {
                _edges.Add(new Edge(edge.p1, edge.p2));
                _edges.Add(new Edge(edge.p2, edge.p1));
            }

            _edges.Sort();
        }
    }
}
