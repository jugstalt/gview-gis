using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using gView.Framework.Data;

namespace gView.Framework.Network
{
    public interface INetworkFeatureClass
    {
        IGraphTableAdapter GraphTableAdapter();

        Task<IFeatureCursor> GetNodeFeatures(IQueryFilter filter);
        Task<IFeatureCursor> GetEdgeFeatures(IQueryFilter filter);

        Task<IFeature> GetNodeFeature(int nid);
        Task<IFeature> GetEdgeFeature(int eid);

        Task<IFeature> GetNodeFeatureAttributes(int nodeId, string[] attributes);
        Task<IFeature> GetEdgeFeatureAttributes(int edgeId, string[] attributes);

        List<IFeatureClass> NetworkClasses { get; }
        string NetworkClassName(int fcid);
        int NetworkClassId(string className);

        int MaxNodeId { get; }
        bool HasDisabledSwitches { get; }
        GraphWeights GraphWeights { get; }

        Task<IGraphEdge> GetGraphEdge(gView.Framework.Geometry.IPoint point, double tolerance);
    }
}
