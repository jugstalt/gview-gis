using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Network;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Network.Algorthm;

namespace gView.Framework.Network.Tracers
{
    class Helper
    {
        public static List<int> NodeIds(Dijkstra.Nodes nodes)
        {
            List<int> ids = new List<int>();
            foreach (Dijkstra.Node node in nodes)
                ids.Add(node.Id);
            return ids;
        }

        public static void AppendNodeFlags(INetworkFeatureClass network, GraphTable gt, List<int> nodeIds, NetworkTracerOutputCollection output/*, IProgressReporterEvent reportEvent, ProgressReport report*/)
        {
            try
            {
                Dictionary<int, string> fcNames = new Dictionary<int, string>();
                //int counter = 0;
                //if (report != null)
                //{
                //    report.Message = "Add Nodes...";
                //    report.featurePos = 0;
                //    report.featureMax = nodeIds.Count;
                //    reportEvent.reportProgress(report);
                //}
                foreach (int nodeId in nodeIds)
                {
                    int fcId = gt.GetNodeFcid(nodeId);
                    if (fcId >= 0)
                    {
                        IFeature nodeFeature = network.GetNodeFeature(nodeId);
                        if (nodeFeature != null && nodeFeature.Shape is IPoint)
                        {
                            if (!fcNames.ContainsKey(fcId))
                                fcNames.Add(fcId, network.NetworkClassName(fcId));

                            string fcName = fcNames[fcId];

                            output.Add(new NetworkFlagOutput(nodeFeature.Shape as IPoint,
                                new NetworkFlagOutput.NodeFeatureData(nodeId, fcId, Convert.ToInt32(nodeFeature["OID"]), fcName)));
                        }
                    }
                    //counter++;
                    //if (report != null && counter % 1000 == 0)
                    //{
                    //    report.featurePos = counter;
                    //    reportEvent.reportProgress(report);
                    //}
                }
            }
            catch { }
        }
    }
}
