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

        Task<List<IFeatureClass>> NetworkClasses();
        Task<string> NetworkClassName(int fcid);
        Task<int> NetworkClassId(string className);

        int MaxNodeId { get; }
        bool HasDisabledSwitches { get; }
        GraphWeights GraphWeights { get; }

        Task<IGraphEdge> GetGraphEdge(gView.Framework.Geometry.IPoint point, double tolerance);
    }
}
