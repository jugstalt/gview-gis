using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Network;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.Network.Algorthm;

namespace gView.Framework.Network.Tracers
{
    [RegisterPlugIn("325AD6CC-A7A7-4d21-960B-5C2B437DFB69")]
    public class TraceShortestPathConnectioned : INetworkTracer, IProgressReporterEvent
    {
        #region INetworkTracer Member

        public string Name
        {
            get { return "Trace Shortest Path & Connected"; }
        }

        public bool CanTrace(NetworkTracerInputCollection input)
        {
            if (input == null)
                return false;

            return input.Collect(NetworkTracerInputType.SourceNode).Count == 1 &&
                   input.Collect(NetworkTracerInputType.SinkNode).Count == 1;
        }

        public NetworkTracerOutputCollection Trace(INetworkFeatureClass network, NetworkTracerInputCollection input, ICancelTracker cancelTraker)
        {
            if (network == null || !CanTrace(input))
                return null;

            GraphTable gt = new GraphTable(network.GraphTableAdapter());
            NetworkSourceInput source = input.Collect(NetworkTracerInputType.SourceNode)[0] as NetworkSourceInput;
            NetworkSinkInput sink = input.Collect(NetworkTracerInputType.SinkNode)[0] as NetworkSinkInput;

            Dijkstra dijkstra = new Dijkstra(cancelTraker);
            dijkstra.reportProgress += this.ReportProgress;
            dijkstra.ApplySwitchState = input.Contains(NetworkTracerInputType.IgnoreSwitches) == false &&
                                        network.HasDisabledSwitches;
            Dijkstra.ApplyInputIds(dijkstra, input);

            // Kürzesten Weg berechenen
            if (!dijkstra.Calculate(gt, source.NodeId, sink.NodeId))
                return null;

            Dijkstra.Nodes initialNodes = dijkstra.DijkstraPathNodes(sink.NodeId);
            dijkstra = new Dijkstra(initialNodes, cancelTraker);
            dijkstra.ForbiddenTargetNodeIds = initialNodes.IdsToList();

            for (int i = 1; i < initialNodes.Count - 1; i++)
                dijkstra.Calculate(gt, initialNodes[i].Id);

            NetworkTracerOutputCollection output = new NetworkTracerOutputCollection();

            NetworkPathOutput pathOutput = new NetworkPathOutput();
            Dijkstra.Nodes nodes = dijkstra.DijkstraNodes;
            foreach (Dijkstra.Node node in nodes)
                pathOutput.Add(new NetworkEdgeOutput(node.EId));
            output.Add(pathOutput);

            if (input.Collect(NetworkTracerInputType.AppendNodeFlags).Count > 0)
                Helper.AppendNodeFlags(network, gt, Helper.NodeIds(nodes), output);

            return output;
        }

        #endregion

        #region IProgressReporterEvent Member

        public event ProgressReporterEvent ReportProgress = null;

        #endregion
    }
}
