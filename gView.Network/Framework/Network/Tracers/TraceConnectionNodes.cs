using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Network;
using gView.Framework.Network.Algorthm;
using gView.Framework.Network.Tracers;
using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Network.Framework.Network.Tracers
{
    public abstract class TraceConnectedTargetNode : INetworkTracer, IProgressReporterEvent
    {
        protected abstract NetworkNodeType TargetNodeType { get; }

        #region INetworkTracer Members

        virtual public string Name
        {
            get { return "Trace Connected Nodes"; }
        }

        public bool CanTrace(NetworkTracerInputCollection input)
        {
            if (input == null)
                return false;

            return input.Collect(NetworkTracerInputType.SourceNode).Count == 1 ||
                   input.Collect(NetworkTracerInputType.SoruceEdge).Count == 1;
        }

        public NetworkTracerOutputCollection Trace(INetworkFeatureClass network, NetworkTracerInputCollection input, gView.Framework.system.ICancelTracker cancelTraker)
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
            List<int> nodeIds = new List<int>();
            foreach (Dijkstra.Node dijkstraNode in dijkstraNodes)
            {
                if (gt.GetNodeType(dijkstraNode.Id) == TargetNodeType)
                    nodeIds.Add(dijkstraNode.Id);

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

            #region Collection Nodes

            counter = 0;
            foreach (int nodeId in nodeIds)
            {
                int fcId = gt.GetNodeFcid(nodeId);
                IFeature nodeFeature = network.GetNodeFeature(nodeId);
                if (nodeFeature != null && nodeFeature.Shape is IPoint)
                {
                    //outputCollection.Add(new NetworkNodeFlagOuput(endNode.Id, nodeFeature.Shape as IPoint));

                    output.Add(new NetworkFlagOutput(nodeFeature.Shape as IPoint,
                        new NetworkFlagOutput.NodeFeatureData(nodeId, fcId, Convert.ToInt32(nodeFeature["OID"]), TargetNodeType.ToString())));
                }
                counter++;
                if (report != null && counter % 1000 == 0)
                {
                    report.featurePos = counter;
                    ReportProgress(report);
                }
            }

            #endregion

            #region Collection Edges

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
            }
            output.Add(pathOutput);

            if (input.Collect(NetworkTracerInputType.AppendNodeFlags).Count > 0)
                Helper.AppendNodeFlags(network, gt, Helper.NodeIds(dijkstraNodes), output);

            #endregion

            return output;
        }

        #endregion

        #region IProgressReporterEvent Member

        public event ProgressReporterEvent ReportProgress;

        #endregion
    }

    [RegisterPlugIn("EBBF514D-DF17-4D57-82C8-6FA73C1D31CB")]
    public class TraceConnectedSources : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Source; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Sources";
            }
        }
    }

    [RegisterPlugIn("3B0338A7-EB79-44FB-91DD-1B85054366AB")]
    public class TraceConnectedSinks : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Sink; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Sinks";
            }
        }
    }

    [RegisterPlugIn("39F2A6FD-C6F8-4036-9E92-DE611E609F47")]
    public class TraceConnectedTrafficCross : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Traffic_Cross; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Traffic Cross";
            }
        }
    }

    [RegisterPlugIn("26F1C494-7B2A-4D96-9D7C-10E58EF9646F")]
    public class TraceConnectedTrafficLight : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Traffic_Light; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Traffic Light";
            }
        }
    }

    [RegisterPlugIn("299D0F08-00BF-474B-BF79-22A6CD93F79D")]
    public class TraceConnectedTrafficRoundabout : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Traffic_Roundabout; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Traffic Roundabout";
            }
        }
    }

    [RegisterPlugIn("31EA1275-CCE3-48B0-B78F-3A0E784312CE")]
    public class TraceConnectedTrafficStop : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Traffic_Stop; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Traffic Stop";
            }
        }
    }

    [RegisterPlugIn("90F7F489-20FE-490B-B294-D1E763E8B129")]
    public class TraceConnectedGasStation : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Gas_Station; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Gas Station";
            }
        }
    }

    [RegisterPlugIn("B50F603B-ABEC-4788-A91D-9072FD6D8571")]
    public class TraceConnectedGasSwitch : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Gas_Switch; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Gas Switch";
            }
        }
    }

    [RegisterPlugIn("74D6FA1B-FBB1-4359-BAC5-86E9BD53AB06")]
    public class TraceConnectedGasCustomer : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Gas_Customer; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Gas Customer";
            }
        }
    }

    [RegisterPlugIn("8F844FC2-D810-44A9-97D1-E41B4D60117F")]
    public class TraceConnectedGasStop : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Gas_Stop; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Gas Stop";
            }
        }
    }

    [RegisterPlugIn("9BCEB61C-CC2C-4347-A48D-78BD1A96FCC2")]
    public class TraceConnectedElectricityCustomer : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Electricity_Customer; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Electricity Customer";
            }
        }
    }

    [RegisterPlugIn("2D1D5100-1756-44E9-B51B-A0690B68DB47")]
    public class TraceConnectedElectricityJunctionBox : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Electricity_JunctionBox; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Electricity JunctionBox";
            }
        }
    }

    [RegisterPlugIn("6E5745B1-4E51-4C94-AD51-E1504A3A359D")]
    public class TraceConnectedElectrictiyStation : TraceConnectedTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Electrictiy_Station; }
        }

        public override string Name
        {
            get
            {
                return "Trace Connected Electrictiy Station";
            }
        }
    }
}
