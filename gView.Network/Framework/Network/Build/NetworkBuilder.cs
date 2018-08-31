using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.UI;
using System.Collections;
using gView.Framework.Network;

namespace gView.Framework.Network.Build
{
    public class NetworkBuilder
    {
        private NetworkNodes _nodes;
        private NetworkEdges _edges;
        private NetworkGraphEdges _graphEdges = null;
        private double _tolerance = double.Epsilon;

        public event ProgressReporterEvent reportProgress = null;

        public NetworkBuilder(IEnvelope bounds, double tolerance)
        {
            if (bounds == null)
                bounds = new Envelope();

            _nodes = new NetworkNodes(bounds);
            _edges = new NetworkEdges(bounds);
            _tolerance = tolerance;
        }

        public void AddEdgeFeature(int fcId, IFeature feature, bool oneWay, Hashtable weights, bool useWithComplexEdges)
        {
            if (feature == null)
                return;

            NetworkEdges edges = null;
            if (feature.Shape is IPolyline)
                edges = new NetworkEdges((IPolyline)feature.Shape);

            if (edges == null)
                return;

            foreach (NetworkEdge edge in edges)
            {
                edge.FeatureclassId = fcId;
                edge.FeatureId = feature.OID;
                edge.OneWay = oneWay;
                edge.UseWithComplexEdges = useWithComplexEdges;
                edge.Weights = weights;

                edge.FromNodeIndex = _nodes.Add(edge.FromPoint, _tolerance);
                edge.ToNodeIndex = _nodes.Add(edge.ToPoint, _tolerance);

                _edges.Add(edge);
            }
        }

        public void AddNodeFeature(int fcId, IFeature feature, bool isSwitch, bool switchState, NetworkNodeType nodeType)
        {
            if (feature == null || !(feature.Shape is IPoint))
                return;

            NetworkNode node = _nodes.Find((IPoint)feature.Shape, _tolerance);
            if (node == null)
            {
                int nodeIndex = _nodes.Add((IPoint)feature.Shape, _tolerance);
                node = _nodes[nodeIndex];
                // Edges splitten!!
                SplitToComplexEdges(node);
            }
            if (node == null)
                return;

            node.SwitchAble = isSwitch;
            node.SwitchState = switchState;
            node.FeatureclassId = fcId;
            node.FeatureId = feature.OID;
            node.Type = nodeType;
        }

        public NetworkNodes NetworkNodes
        {
            get
            {
                return _nodes;
            }
        }
        public NetworkEdges NetworkEdges
        {
            get { return _edges; }
        }
        public bool SplitToComplexEdges(NetworkNode node)
        {
            if (node == null || node.Point == null)
                return false;

            Envelope env = new Envelope(node.Point.X - _tolerance, node.Point.Y - _tolerance,
                                        node.Point.X + _tolerance, node.Point.Y + _tolerance);

            NetworkEdges edges = _edges.Collect(env);
            foreach (NetworkEdge edge in edges)
            {
                if (edge.FromNodeIndex == node.Id || edge.ToNodeIndex == node.Id)
                    continue;

                double dist, stat;

                IPoint snapped = SpatialAlgorithms.Algorithm.Point2PathDistance(edge.Path, node.Point, out dist, out stat);
                if (snapped != null)
                {
                    if (dist <= _tolerance)
                    {
                        Polyline pLine = new Polyline(edge.Path);
                        Polyline p1 = SpatialAlgorithms.Algorithm.PolylineSplit(pLine, 0, stat);
                        Polyline p2 = SpatialAlgorithms.Algorithm.PolylineSplit(pLine, stat, double.MaxValue);
                        if (p1.PathCount != 1 ||
                           p2.PathCount != 1)
                            continue;

                        // Edge bekommt neue Geometrie
                        edge.Path = p1[0];
                        edge.Length = edge.GeoLength = p1[0].Length;
                        // Edge bekommt neuen Zielknoten
                        int toNodeIndex = edge.ToNodeIndex;
                        edge.ToNodeIndex = node.Id;

                        NetworkEdge edge2 = new NetworkEdge();
                        edge2.Path = p2[0];
                        edge2.Length = edge2.GeoLength = p2[0].Length;

                        edge2.FeatureclassId = edge.FeatureclassId;
                        edge2.FeatureId = edge.FeatureId;
                        edge2.OneWay = edge.OneWay;
                        edge2.Weights = edge.Weights;
                        edge.UseWithComplexEdges = edge2.UseWithComplexEdges = true; // edge.Bounds neu berechnen
                        edge.IsComplex = edge2.IsComplex = true;

                        // Edge2 NodeIds setzen
                        edge2.FromNodeIndex = node.Id;
                        edge2.ToNodeIndex = toNodeIndex;

                        _edges.Add(edge2);
                    }
                }
            }

            return false;
        }

        public void CreateGraph()
        {
            _graphEdges = new NetworkGraphEdges();
            ProgressReport report = new ProgressReport();

            //List<NetworkNode> nodes = _nodes.ToList();

            #region Report
            report.Message = "Create Graph...";
            report.featureMax = _edges.Count; //nodes.Count;
            report.featurePos = 0;
            if (reportProgress != null) reportProgress(report);
            #endregion

            #region Old and Slow
            //int nodeIndex = 0;
            //foreach (NetworkNode node in nodes)
            //{
            //    foreach (NetworkEdge edge in _edges.SelectFrom(nodeIndex))
            //    {
            //        _graphEdges.Add(new NetworkGraphEdge(edge.FromNodeIndex, edge.ToNodeIndex, _edges.IndexOf(edge), edge.Length, edge.GeoLength));
            //    }
            //    foreach (NetworkEdge edge in _edges.SelectTo(nodeIndex))
            //    {
            //        if (edge.OneWay == false)
            //        {
            //            _graphEdges.Add(new NetworkGraphEdge(edge.ToNodeIndex, edge.FromNodeIndex, _edges.IndexOf(edge), edge.Length, edge.GeoLength));
            //        }
            //    }
            //    nodeIndex++;

            //    #region Report
            //    if (nodeIndex % 100 == 0)
            //    {
            //        report.featurePos = nodeIndex;
            //        if (reportProgress != null) reportProgress(report);
            //    }
            //    #endregion
            //}
            #endregion

            int edgeIndex = 0;
            foreach (NetworkEdge edge in _edges)
            {
                _graphEdges.Add(new NetworkGraphEdge(edge.FromNodeIndex, edge.ToNodeIndex, edge.Id, edge.Length, edge.GeoLength));
                if (edge.OneWay == false)
                    _graphEdges.Add(new NetworkGraphEdge(edge.ToNodeIndex, edge.FromNodeIndex, edge.Id, edge.Length, edge.GeoLength));

                edgeIndex++;
                #region Report
                if (edgeIndex % 100 == 0)
                {
                    report.featurePos = edgeIndex;
                    if (reportProgress != null) reportProgress(report);
                }
                #endregion
            }

            #region Report
            report.Message = "Sorting Graph...";
            report.featurePos = report.featureMax;
            if (reportProgress != null) reportProgress(report);
            #endregion

            _graphEdges.Sort(new NetworkGraphEdges.NodeIndexComparer());
            //_graphEdges.RemoveDoubles();

            #region Create Graph Row Index
            //List<NetworkNode> nodes = _nodes.ToList();
            //NetworkNode comparerNode = new NetworkNode(-1, null), actNode = null;
            //NetworkNodes.NodeIdComparer idComparer = new NetworkNodes.NodeIdComparer();
            //int RowIndex = 1;

            //foreach (NetworkGraphEdge graphEdge in _graphEdges)
            //{
            //    if (comparerNode.Id != graphEdge.FromNodeIndex)
            //    {
            //        if (actNode != null)
            //            actNode.LastGraphRow = RowIndex - 1;

            //        comparerNode.Id = graphEdge.FromNodeIndex;
            //        int nodeIndex = nodes.BinarySearch(comparerNode, idComparer);
            //        actNode = nodes[nodeIndex];
            //        actNode.FirstGraphRow = RowIndex;
            //    }
            //    RowIndex++;
            //}
            #endregion
        }

        public IFeatureCursor Edges
        {
            get { return new EdgeCursor(_edges); }
        }
        public IFeatureCursor Nodes
        {
            get { return new NodeCursor(_nodes); }
        }
        public IFeatureCursor Graph
        {
            get { return new GraphCursor(_graphEdges); }
        }

        #region Cursors
        private class NodeCursor : IFeatureCursor, ICount
        {
            private List<NetworkNode> _nodes;
            private int _pos = 0;

            public NodeCursor(NetworkNodes nodes)
            {
                _nodes = nodes.ToList();
            }

            #region IFeatureCursor Member

            public IFeature NextFeature
            {
                get
                {
                    if (_nodes == null || _pos >= _nodes.Count)
                        return null;

                    NetworkNode node = _nodes[_pos++];

                    Feature feature = new Feature();
                    feature.Shape = node.Point;
                    //feature.Fields.Add(new FieldValue("NID", node.Id));
                    //feature.Fields.Add(new FieldValue("G1", node.FirstGraphRow));
                    //feature.Fields.Add(new FieldValue("G2", node.LastGraphRow));
                    feature.Fields.Add(new FieldValue("SWITCH", node.SwitchAble));
                    feature.Fields.Add(new FieldValue("STATE", node.SwitchState));
                    feature.Fields.Add(new FieldValue("FCID", node.FeatureclassId));
                    feature.Fields.Add(new FieldValue("OID", node.FeatureId));
                    feature.Fields.Add(new FieldValue("NODETYPE", node.Type));

                    return feature;
                }
            }

            #endregion

            #region IDisposable Member

            public void Dispose()
            {

            }

            #endregion

            #region ICount Member

            public int Count
            {
                get
                {
                    if (_nodes != null)
                        return _nodes.Count;
                    return 0;
                }
            }

            #endregion
        }

        private class EdgeCursor : IFeatureCursor, ICount
        {
            private NetworkEdges _edges;
            private int _pos = 0;

            public EdgeCursor(NetworkEdges edges)
            {
                _edges = edges;
            }
            #region IFeatureCursor Member

            public IFeature NextFeature
            {
                get
                {
                    if (_edges == null || _pos >= _edges.Count)
                        return null;

                    NetworkEdge edge = _edges[_pos++];

                    Feature feature = new Feature();
                    feature.Shape = new Polyline();
                    ((Polyline)feature.Shape).AddPath(edge.Path);
                    feature.Fields.Add(new FieldValue("EID", edge.Id));
                    feature.Fields.Add(new FieldValue("N1", edge.FromNodeIndex));
                    feature.Fields.Add(new FieldValue("N2", edge.ToNodeIndex));
                    //feature.Fields.Add(new FieldValue("Length", edge.Length));
                    //feature.Fields.Add(new FieldValue("GeoLength", edge.GeoLength));
                    feature.Fields.Add(new FieldValue("FCID", edge.FeatureclassId));
                    feature.Fields.Add(new FieldValue("OID", edge.FeatureId));
                    feature.Fields.Add(new FieldValue("ISCOMPLEX", edge.IsComplex));
                    return feature;
                }
            }

            #endregion

            #region IDisposable Member

            public void Dispose()
            {

            }

            #endregion

            #region ICount Member

            public int Count
            {
                get
                {
                    if (_edges != null)
                        return _edges.Count;
                    return 0;
                }
            }

            #endregion
        }

        private class GraphCursor : IFeatureCursor, ICount
        {
            private NetworkGraphEdges _graphEdges;
            private int _pos = 0;

            public GraphCursor(NetworkGraphEdges graphEdges)
            {
                _graphEdges = graphEdges;
            }
            #region IFeatureCursor Member

            public IFeature NextFeature
            {
                get
                {
                    if (_graphEdges == null || _pos >= _graphEdges.Count)
                        return null;

                    NetworkGraphEdge edge = _graphEdges[_pos++];

                    Feature feature = new Feature();
                    feature.Fields.Add(new FieldValue("EID", edge.EdgeIndex));
                    feature.Fields.Add(new FieldValue("N1", edge.FromNodeIndex));
                    feature.Fields.Add(new FieldValue("N2", edge.ToNodeIndex));
                    feature.Fields.Add(new FieldValue("LENGTH", edge.Length));
                    feature.Fields.Add(new FieldValue("GEOLENGTH", edge.GeoLength));
                    return feature;
                }
            }

            #endregion

            #region IDisposable Member

            public void Dispose()
            {

            }

            #endregion

            #region ICount Member

            public int Count
            {
                get
                {
                    if (_graphEdges != null)
                        return _graphEdges.Count;

                    return 0;
                }
            }

            #endregion
        }

        #endregion
    }
}
