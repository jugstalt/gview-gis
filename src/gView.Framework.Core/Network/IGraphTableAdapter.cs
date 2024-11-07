using gView.Framework.Core.Data;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Core.Network
{
    public interface IGraphTableAdapter
    {
        GraphTableRows QueryN1(int n1);
        IGraphTableRow QueryN1ToN2(int n1, int n2);

        IGraphEdge QueryEdge(int eid);
        double QueryEdgeWeight(Guid weightGuid, int eid);
        bool SwitchState(int nid);
        int GetNodeFcid(int nid);
        NetworkNodeType GetNodeType(int nid);

        Task<Features> QueryNodeEdgeFeatures(int n1);
        Task<Features> QueryNodeFeatures(int n1);
    }
}
