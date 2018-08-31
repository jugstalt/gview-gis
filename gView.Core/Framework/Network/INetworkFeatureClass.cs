using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;

namespace gView.Framework.Network
{
    public interface INetworkFeatureClass
    {
        IGraphTableAdapter GraphTableAdapter();

        IFeatureCursor GetNodeFeatures(IQueryFilter filter);
        IFeatureCursor GetEdgeFeatures(IQueryFilter filter);

        IFeature GetNodeFeature(int nid);
        IFeature GetEdgeFeature(int eid);

        IFeature GetNodeFeatureAttributes(int nodeId, string[] attributes);
        IFeature GetEdgeFeatureAttributes(int edgeId, string[] attributes);

        List<IFeatureClass> NetworkClasses { get; }
        string NetworkClassName(int fcid);
        int NetworkClassId(string className);

        int MaxNodeId { get; }
        bool HasDisabledSwitches { get; }
        GraphWeights GraphWeights { get; }

        IGraphEdge GetGraphEdge(gView.Framework.Geometry.IPoint point, double tolerance);
    }
}
