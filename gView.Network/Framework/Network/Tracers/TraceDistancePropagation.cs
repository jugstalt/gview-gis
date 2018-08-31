using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Network;
using gView.Framework.UI;
using gView.Framework.Network.Algorthm;
using gView.Framework.system;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Framework.Network.Tracers
{
    [RegisterPlugIn("D102AFFA-1D37-4de0-9853-C28E2F3C5DC6")]
    public class TraceDistancePropagation : INetworkTracer, INetworkTracerProperties, IProgressReporterEvent
    {
        private Properties _properties = new Properties();

        #region INetworkTracer Member

        public string Name
        {
            get { return "Trace Distance Propagation"; }
        }

        public bool CanTrace(NetworkTracerInputCollection input)
        {
            if (input == null)
                return false;

            return input.Collect(NetworkTracerInputType.SourceNode).Count == 1;
        }

        public NetworkTracerOutputCollection Trace(INetworkFeatureClass network, NetworkTracerInputCollection input, gView.Framework.system.ICancelTracker cancelTraker)
        {
            if (network == null || !CanTrace(input))
                return null;

            GraphTable gt = new GraphTable(network.GraphTableAdapter());
            NetworkSourceInput source = input.Collect(NetworkTracerInputType.SourceNode)[0] as NetworkSourceInput;
            NetworkWeighInput weight =
                input.Contains(NetworkTracerInputType.Weight) ?
                    input.Collect(NetworkTracerInputType.Weight)[0] as NetworkWeighInput :
                    null;

            Dijkstra dijkstra = new Dijkstra(cancelTraker);
            dijkstra.reportProgress += this.ReportProgress;
            dijkstra.MaxDistance = _properties.Distance;
            if (weight != null)
            {
                dijkstra.GraphWeight = weight.Weight;
                dijkstra.WeightApplying = weight.WeightApplying;
            }
            dijkstra.ApplySwitchState = input.Contains(NetworkTracerInputType.IgnoreSwitches) == false &&
                                        network.HasDisabledSwitches;
            Dijkstra.ApplyInputIds(dijkstra, input);

            dijkstra.Calculate(gt, source.NodeId);

            NetworkTracerOutputCollection output = new NetworkTracerOutputCollection();

            #region Knoten/Kanten <= Distance
            Dijkstra.Nodes dijkstraNodes = dijkstra.DijkstraNodesWithMaxDistance(_properties.Distance);
            if (dijkstraNodes == null)
                return null;

            List<int> edgeIds = new List<int>();
            foreach (Dijkstra.Node dijkstraNode in dijkstraNodes)
            {
                if (dijkstraNode.EId < 0)
                    continue;

                int index = edgeIds.BinarySearch(dijkstraNode.EId);
                if (index < 0)
                    edgeIds.Insert(~index, dijkstraNode.EId);

                if (Math.Abs(dijkstraNode.Dist - _properties.Distance) < double.Epsilon)
                {
                    // ToDo: Flag einfügen!!
                }
            }

            output.Add(new NetworkEdgeCollectionOutput(edgeIds));
            #endregion

            #region Knoten/Kanten > Distance
            Dijkstra.Nodes cnodes = dijkstra.DijkstraNodesDistanceGreaterThan(_properties.Distance);
            foreach (Dijkstra.Node cnode in cnodes)
            {
                Dijkstra.Node node = dijkstra.DijkstraNodes.ById(cnode.Pre);
                if (node == null)
                    continue;

                IGraphEdge graphEdge = gt.QueryEdge(cnode.EId);
                if (graphEdge == null)
                    continue;

                RowIDFilter filter = new RowIDFilter(String.Empty);
                filter.IDs.Add(graphEdge.Eid);
                IFeatureCursor cursor = network.GetEdgeFeatures(filter);
                if (cursor == null)
                    continue;

                IFeature feature = cursor.NextFeature;
                if (feature == null)
                    continue;

                IPath path = null;
                if (cnode.Id != graphEdge.N2 && cnode.Id == graphEdge.N1)
                {
                    ((Polyline)feature.Shape)[0].ChangeDirection();
                }

                double trimDist = _properties.Distance - node.Dist;
                string label = _properties.Distance.ToString();
                if (weight != null)
                {
                    double w = gt.QueryEdgeWeight(weight.Weight.Guid, cnode.EId);
                    switch (weight.WeightApplying)
                    {
                        case WeightApplying.Weight:
                            trimDist *= w;
                            break;
                        case WeightApplying.ActualCosts:
                            trimDist = ((IPolyline)feature.Shape)[0].Length * trimDist * w;  // ??? Prüfen
                            break;
                    }
                    label += "\n(" + Math.Round(node.GeoDist + trimDist, 2).ToString() + ")";
                }
                path = ((IPolyline)feature.Shape)[0].Trim(trimDist);
                Polyline polyline = new Polyline();
                polyline.AddPath(path);

                output.Add(new NetworkEdgePolylineOutput(cnode.EId, polyline));

                output.Add(new NetworkFlagOutput(
                    polyline[0][polyline[0].PointCount - 1],
                    label));
            }
            #endregion

            return output;
        }

        #endregion

        #region IProgressReporterEvent Member

        public event ProgressReporterEvent ReportProgress = null;

        #endregion

        #region INetworkTracerProperties Member

        public object NetworkTracerProperties(INetworkFeatureClass network, NetworkTracerInputCollection input)
        {
            return _properties;
        }

        #endregion

        private class Properties
        {
            private double _distance = 1.0;

            public double Distance
            {
                get { return _distance; }
                set { _distance = value; }
            }
        }
    }
}
