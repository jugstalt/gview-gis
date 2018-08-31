using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Network;
using gView.Framework.Network.Algorthm;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Framework.Network.Tracers
{
    [RegisterPlugIn("343F2D31-5CE7-40ca-8FF6-8D8DE8D081F2")]
    public class TraceShortestPath : INetworkTracer, IProgressReporterEvent
    {
        #region INetworkTracer Member

        public string Name
        {
            get { return "Trace Shortest Path"; }
        }

        public bool CanTrace(NetworkTracerInputCollection input)
        {
            if (input == null)
                return false;

            return input.Collect(NetworkTracerInputType.SourceNode).Count == 1 &&
                   input.Collect(NetworkTracerInputType.SinkNode).Count == 1;
        }

        public NetworkTracerOutputCollection Trace(INetworkFeatureClass network, NetworkTracerInputCollection input, gView.Framework.system.ICancelTracker cancelTraker)
        {
            if (network == null || !CanTrace(input))
                return null;

            GraphTable gt = new GraphTable(network.GraphTableAdapter());
            NetworkSourceInput source = input.Collect(NetworkTracerInputType.SourceNode)[0] as NetworkSourceInput;
            NetworkSinkInput sink = input.Collect(NetworkTracerInputType.SinkNode)[0] as NetworkSinkInput;
            NetworkWeighInput weight =
                input.Contains(NetworkTracerInputType.Weight) ?
                    input.Collect(NetworkTracerInputType.Weight)[0] as NetworkWeighInput :
                    null;

            Dijkstra dijkstra = new Dijkstra(cancelTraker);
            dijkstra.reportProgress += this.ReportProgress;
            if (weight != null)
            {
                dijkstra.GraphWeight = weight.Weight;
                dijkstra.WeightApplying = weight.WeightApplying;
            }
            dijkstra.ApplySwitchState = input.Contains(NetworkTracerInputType.IgnoreSwitches) == false &&
                                        network.HasDisabledSwitches;
            Dijkstra.ApplyInputIds(dijkstra, input);

            dijkstra.Calculate(gt, source.NodeId, sink.NodeId);
            Dijkstra.NetworkPath networkPath = dijkstra.DijkstraPath(sink.NodeId);
            if (networkPath == null)
                return null;

            NetworkTracerOutputCollection output = new NetworkTracerOutputCollection();

            NetworkPathOutput pathOutput = new NetworkPathOutput();
            foreach (Dijkstra.NetworkPathEdge pathEdge in networkPath)
            {
                pathOutput.Add(new NetworkEdgeOutput(pathEdge.EId));
            }

            output.Add(pathOutput);

            if (input.Collect(NetworkTracerInputType.AppendNodeFlags).Count > 0)
            {
                Dijkstra.Nodes pathNodes = dijkstra.DijkstraPathNodes(sink.NodeId);
                Helper.AppendNodeFlags(network, gt, Helper.NodeIds(pathNodes), output);
            }
            //if (pathNodes != null)
            //{
            //    foreach (Dijkstra.Node node in pathNodes)
            //    {
            //        string label = node.Dist.ToString();
            //        if (weight != null)
            //        {
            //            label += "\n(" + (Math.Round(node.GeoDist, 2)).ToString() + ")";
            //        }
            //        output.Add(new NetworkNodeFlagOuput(node.Id));
            //    }
            //}

            return output;
        }

        #endregion

        #region IProgressReporterEvent Member

        public event ProgressReporterEvent ReportProgress;

        #endregion
    }
}
