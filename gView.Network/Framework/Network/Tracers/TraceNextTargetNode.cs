using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using gView.Framework.Network;
using gView.Framework.UI;
using gView.Framework.Network.Algorthm;
using System.ComponentModel;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Framework.Network.Tracers
{
    public abstract class TraceNextTargetNode : INetworkTracer, IProgressReporterEvent
    {
        protected abstract NetworkNodeType TargetNodeType { get; }

        #region INetworkTracer Member

        virtual public string Name
        {
            get { return "Trace Next Node"; }
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

            input.Collect(NetworkTracerInputType.BarrierNodes);

            NetworkTracerOutputCollection outputCollection = new NetworkTracerOutputCollection();
            List<int> edgeIds = new List<int>();
            NetworkPathOutput pathOutput = new NetworkPathOutput();

            List<int> neighborNodeFcIds = new List<int>();
            Dictionary<int, string> neighborFcs = new Dictionary<int, string>();

            foreach (var networkClass in network.NetworkClasses)
            {
                if (networkClass.GeometryType != geometryType.Point)
                    continue;

                int fcid = network.NetworkClassId(networkClass.Name);

                neighborNodeFcIds.Add(fcid);
                neighborFcs.Add(fcid, networkClass.Name);
            }

            Dijkstra dijkstra = new Dijkstra(cancelTraker);
            dijkstra.reportProgress += this.ReportProgress;
            dijkstra.ApplySwitchState = input.Contains(NetworkTracerInputType.IgnoreSwitches) == false &&
                                        network.HasDisabledSwitches;
            dijkstra.TargetNodeFcIds = neighborNodeFcIds;
            dijkstra.TargetNodeType = TargetNodeType;
            Dijkstra.ApplyInputIds(dijkstra, input);

            Dijkstra.Nodes dijkstraEndNodes = null;
            if (sourceNode != null)
            {
                dijkstra.Calculate(gt, sourceNode.NodeId);

                dijkstraEndNodes = dijkstra.DijkstraEndNodes;
            }
            else if (sourceEdge != null)
            {
                IGraphEdge graphEdge = gt.QueryEdge(sourceEdge.EdgeId);
                if (graphEdge == null)
                    return null;

                bool n1_2_n2 = gt.QueryN1ToN2(graphEdge.N1, graphEdge.N2) != null;
                bool n2_2_n1 = gt.QueryN1ToN2(graphEdge.N2, graphEdge.N1) != null;
                if (n1_2_n2 == false &&
                    n2_2_n1 == false)
                    return null;

                bool n1switchState = dijkstra.ApplySwitchState ? gt.SwitchState(graphEdge.N1) : true;
                bool n2switchState = dijkstra.ApplySwitchState ? gt.SwitchState(graphEdge.N2) : true;

                bool n1isNeighbor = neighborNodeFcIds.Contains(gt.GetNodeFcid(graphEdge.N1));
                bool n2isNeighbor = neighborNodeFcIds.Contains(gt.GetNodeFcid(graphEdge.N2));

                if (n1isNeighbor && n2isNeighbor)
                {
                    dijkstraEndNodes = new Dijkstra.Nodes();
                    dijkstraEndNodes.Add(new Dijkstra.Node(graphEdge.N1));
                    dijkstraEndNodes.Add(new Dijkstra.Node(graphEdge.N2));
                    dijkstraEndNodes[0].EId = graphEdge.Eid;

                    edgeIds.Add(graphEdge.Eid);
                    pathOutput.Add(new NetworkEdgeOutput(graphEdge.Eid));
                }
                else
                {
                    if (!n1isNeighbor && n1switchState == true)
                    {
                        dijkstra.Calculate(gt, graphEdge.N1);
                        dijkstraEndNodes = dijkstra.DijkstraEndNodes;

                        if (!n1_2_n2 && n2isNeighbor)
                        {
                            Dijkstra.Node n1Node = new Dijkstra.Node(graphEdge.N2);
                            n1Node.EId = graphEdge.Eid;
                            dijkstraEndNodes.Add(n1Node);

                            edgeIds.Add(graphEdge.Eid);
                            pathOutput.Add(new NetworkEdgeOutput(graphEdge.Eid));
                        }
                    }
                    else if (!n2isNeighbor && n2switchState == true)
                    {
                        dijkstra.Calculate(gt, graphEdge.N2);
                        dijkstraEndNodes = dijkstra.DijkstraEndNodes;

                        if (!n2_2_n1 && n1isNeighbor)
                        {
                            Dijkstra.Node n1Node = new Dijkstra.Node(graphEdge.N1);
                            n1Node.EId = graphEdge.Eid;
                            dijkstraEndNodes.Add(n1Node);

                            edgeIds.Add(graphEdge.Eid);
                            pathOutput.Add(new NetworkEdgeOutput(graphEdge.Eid));
                        }
                    }
                }
            }

            #region Create Output
            if (dijkstraEndNodes == null)
                return null;

            ProgressReport report = (ReportProgress != null ? new ProgressReport() : null);

            #region Collect End Nodes
            if (report != null)
            {
                report.Message = "Collect End Nodes...";
                report.featurePos = 0;
                report.featureMax = dijkstraEndNodes.Count;
                ReportProgress(report);
            }

            int counter = 0;
            foreach (Dijkstra.Node endNode in dijkstraEndNodes)
            {
                int fcId = gt.GetNodeFcid(endNode.Id);
                if (neighborNodeFcIds.Contains(fcId) && gt.GetNodeType(endNode.Id) == this.TargetNodeType)
                {
                    IFeature nodeFeature = network.GetNodeFeature(endNode.Id);
                    if (nodeFeature != null && nodeFeature.Shape is IPoint)
                    {
                        string fcName = neighborFcs.ContainsKey(fcId) ?
                            neighborFcs[fcId] : String.Empty;

                        //outputCollection.Add(new NetworkNodeFlagOuput(endNode.Id, nodeFeature.Shape as IPoint));

                        outputCollection.Add(new NetworkFlagOutput(nodeFeature.Shape as IPoint,
                            new NetworkFlagOutput.NodeFeatureData(endNode.Id, fcId, Convert.ToInt32(nodeFeature["OID"]), fcName)));
                    }
                }
                counter++;
                if (report != null && counter % 1000 == 0)
                {
                    report.featurePos = counter;
                    ReportProgress(report);
                }
            }
            #endregion

            #region Collect EdgedIds
            if (report != null)
            {
                report.Message = "Collect Edges...";
                report.featurePos = 0;
                report.featureMax = dijkstra.DijkstraNodes.Count;
                ReportProgress(report);
            }

            counter = 0;
            foreach (Dijkstra.Node dijkstraEndNode in dijkstraEndNodes)
            {
                Dijkstra.NetworkPath networkPath = dijkstra.DijkstraPath(dijkstraEndNode.Id);
                if (networkPath == null)
                    continue;

                foreach (Dijkstra.NetworkPathEdge pathEdge in networkPath)
                {
                    int index = edgeIds.BinarySearch(pathEdge.EId);
                    if (index >= 0)
                        continue;
                    edgeIds.Insert(~index, pathEdge.EId);

                    pathOutput.Add(new NetworkEdgeOutput(pathEdge.EId));
                    counter++;
                    if (report != null && counter % 1000 == 0)
                    {
                        report.featurePos = counter;
                        ReportProgress(report);
                    }
                }
            }
            if (pathOutput.Count > 0)
                outputCollection.Add(pathOutput);
            #endregion
            #endregion

            return outputCollection;
        }

        #endregion

        #region IProgressReporterEvent Member

        public event ProgressReporterEvent ReportProgress;

        #endregion
    }

    [RegisterPlugIn("B2F1272A-FC0E-40BD-9219-5ED6A660304E")]
    public class TraceNextSources : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Source; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Sources";
            }
        }
    }

    [RegisterPlugIn("3EBC7A9E-A386-4EC2-99F9-36653A64ECBE")]
    public class TraceNextSinks : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Sink; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Sinks";
            }
        }
    }

    [RegisterPlugIn("96242C33-7221-47B1-8069-3905FEE66241")]
    public class TraceNextTrafficCross : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Traffic_Cross; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Traffic Cross";
            }
        }
    }

    [RegisterPlugIn("2EAAF416-1ABE-4957-9107-77F71007442D")]
    public class TraceNextTrafficLight : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Traffic_Light; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Traffic Light";
            }
        }
    }

    [RegisterPlugIn("B9670DCF-02A5-4EED-A59F-154DCF064B05")]
    public class TraceNextTrafficRoundabout : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Traffic_Roundabout; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Traffic Roundabout";
            }
        }
    }

    [RegisterPlugIn("DCE2F264-F464-4AEA-B534-39201C12DF3D")]
    public class TraceNextTrafficStop : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Traffic_Stop; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Traffic Stop";
            }
        }
    }

    [RegisterPlugIn("C12055D1-A981-40A1-A84D-DCB900464ACE")]
    public class TraceNextGasStation : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Gas_Station; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Gas Station";
            }
        }
    }

    [RegisterPlugIn("A966A99A-FFA0-43D3-9BBB-161ADDF171C2")]
    public class TraceNextGasSwitch : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Gas_Switch; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Gas Switch";
            }
        }
    }

    [RegisterPlugIn("D954E4D8-DEBC-40BC-9648-49A404E879AF")]
    public class TraceNextGasCustomer : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Gas_Customer; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Gas Customer";
            }
        }
    }

    [RegisterPlugIn("09F050A2-DC30-4C42-9281-C54CC1D54469")]
    public class TraceNextGasStop : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Gas_Stop; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Gas Stop";
            }
        }
    }

    [RegisterPlugIn("3CCC0592-42AD-4A14-ABE7-F3C596000C19")]
    public class TraceNextElectricityCustomer : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Electricity_Customer; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Electricity Customer";
            }
        }
    }

    [RegisterPlugIn("84F9E9C7-B945-486C-BE18-AD727377AF41")]
    public class TraceNextElectricityJunctionBox : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Electricity_JunctionBox; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Electricity JunctionBox";
            }
        }
    }

    [RegisterPlugIn("DA57D5E1-709B-4638-B322-44B37F1482DE")]
    public class TraceNextElectrictiyStation : TraceNextTargetNode
    {
        protected override NetworkNodeType TargetNodeType
        {
            get { return NetworkNodeType.Electrictiy_Station; }
        }

        public override string Name
        {
            get
            {
                return "Trace Next Electrictiy Station";
            }
        }
    }
}
