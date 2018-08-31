using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using gView.Framework.Network;
using gView.Framework.UI;
using gView.Framework.Network.Algorthm;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Framework.Network.Tracers
{
    [RegisterPlugIn("63AF7F85-6617-4944-ABED-A98CA2B45CA9")]
    class TraceConnected : INetworkTracer, IProgressReporterEvent
    {
        #region INetworkTracer Member

        public string Name
        {
            get { return "Trace Connected"; }
        }

        public bool CanTrace(NetworkTracerInputCollection input)
        {
            if (input == null)
                return false;

            return input.Collect(NetworkTracerInputType.SourceNode).Count == 1 ||
                   input.Collect(NetworkTracerInputType.SoruceEdge).Count == 1;
        }

        public NetworkTracerOutputCollection Trace(INetworkFeatureClass network, NetworkTracerInputCollection input, ICancelTracker cancelTraker)
        {
            if (network == null || !CanTrace(input))
                return null;

            GraphTable gt = new GraphTable(network.GraphTableAdapter());
            NetworkSourceInput sourceNode = null;
            NetworkSourceEdgeInput sourceEdge = null;
            if (input.Collect(NetworkTracerInputType.SourceNode).Count == 1)
                sourceNode = input.Collect(NetworkTracerInputType.SourceNode)[0] as NetworkSourceInput;
            else if (input.Collect(NetworkTracerInputType.SoruceEdge).Count == 1)
                sourceEdge = input.Collect(NetworkTracerInputType.SoruceEdge)[0] as NetworkSourceEdgeInput;
            else
                return null;

            Dijkstra dijkstra = new Dijkstra(cancelTraker);
            dijkstra.reportProgress += this.ReportProgress;
            dijkstra.ApplySwitchState = input.Contains(NetworkTracerInputType.IgnoreSwitches) == false &&
                                        network.HasDisabledSwitches;
            Dijkstra.ApplyInputIds(dijkstra, input);

            if (sourceNode != null)
            {
                dijkstra.Calculate(gt, sourceNode.NodeId);
            }
            else if (sourceEdge != null)
            {
                IGraphEdge graphEdge = gt.QueryEdge(sourceEdge.EdgeId);
                if (graphEdge == null)
                    return null;

                bool n1_2_n2 = gt.QueryN1ToN2(graphEdge.N1, graphEdge.N2) != null;
                bool n2_2_n1 = gt.QueryN1ToN2(graphEdge.N2, graphEdge.N1) != null;

                bool n1switchState = dijkstra.ApplySwitchState ? gt.SwitchState(graphEdge.N1) : true;
                bool n2switchState = dijkstra.ApplySwitchState ? gt.SwitchState(graphEdge.N2) : true;

                if (n1_2_n2 && n1switchState == true)
                    dijkstra.Calculate(gt, graphEdge.N1);
                else if (n2_2_n1 && n2switchState == true)
                    dijkstra.Calculate(gt, graphEdge.N2);
                else
                    return null;
            }

            Dijkstra.Nodes dijkstraNodes = dijkstra.DijkstraNodesWithMaxDistance(double.MaxValue);
            if (dijkstraNodes == null)
                return null;

            ProgressReport report = (ReportProgress != null ? new ProgressReport() : null);

            #region Collect EdgedIds
            if (report != null)
            {
                report.Message = "Collected Edges...";
                report.featurePos = 0;
                report.featureMax = dijkstraNodes.Count;
                ReportProgress(report);
            }

            NetworkInputForbiddenEdgeIds forbiddenEdgeIds = (input.Contains(NetworkTracerInputType.ForbiddenEdgeIds)) ? input.Collect(NetworkTracerInputType.ForbiddenEdgeIds)[0] as NetworkInputForbiddenEdgeIds : null;
            NetworkInputForbiddenStartNodeEdgeIds forbiddenStartNodeEdgeIds = (input.Contains(NetworkTracerInputType.ForbiddenStartNodeEdgeIds)) ? input.Collect(NetworkTracerInputType.ForbiddenStartNodeEdgeIds)[0] as NetworkInputForbiddenStartNodeEdgeIds : null;

            int counter = 0;
            List<int> edgeIds = new List<int>();
            foreach (Dijkstra.Node dijkstraNode in dijkstraNodes)
            {
                if (dijkstra.ApplySwitchState)
                {
                    if (gt.SwitchState(dijkstraNode.Id) == false)  // hier ist Schluss!!
                        continue;
                }

                GraphTableRows gtRows = gt.QueryN1(dijkstraNode.Id);
                if (gtRows == null)
                    continue;

                foreach (IGraphTableRow gtRow in gtRows)
                {

                    int eid = gtRow.EID;

                    if (sourceNode != null &&
                        forbiddenStartNodeEdgeIds != null && dijkstraNode.Id == sourceNode.NodeId &&
                        forbiddenStartNodeEdgeIds.Ids.Contains(eid))
                        continue;

                    if (forbiddenEdgeIds != null && forbiddenEdgeIds.Ids.Contains(eid))
                        continue;


                    int index = edgeIds.BinarySearch(eid);
                    if (index < 0)
                        edgeIds.Insert(~index, eid);
                }

                counter++;
                if (report != null && counter % 1000 == 0)
                {
                    report.featurePos = counter;
                    ReportProgress(report);
                }
            }
            #endregion

            NetworkTracerOutputCollection output = new NetworkTracerOutputCollection();

            if (report != null)
            {
                report.Message = "Add Edges...";
                report.featurePos = 0;
                report.featureMax = edgeIds.Count;
                ReportProgress(report);
            }
            counter = 0;
            NetworkPathOutput pathOutput = new NetworkPathOutput();
            foreach (int edgeId in edgeIds)
            {
                pathOutput.Add(new NetworkEdgeOutput(edgeId));
                counter++;
                if (report != null && counter % 1000 == 0)
                {
                    report.featurePos = counter;
                    ReportProgress(report);
                }
                //var x = network.GetEdgeFeatureAttributes(edgeId, null);
            }
            output.Add(pathOutput);

            if (input.Collect(NetworkTracerInputType.AppendNodeFlags).Count > 0)
                Helper.AppendNodeFlags(network, gt, Helper.NodeIds(dijkstraNodes), output);

            return output;
        }

        #endregion

        #region IProgressReporterEvent Member

        public event ProgressReporterEvent ReportProgress = null;

        #endregion
    }
}
