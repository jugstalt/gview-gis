using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.system;
using gView.Framework.IO;

namespace gView.Framework.Network
{
    public interface IGraphTableRow
    {
        int N1 { get; }
        int N2 { get; }
        int EID { get; }
        double GEOLEN { get; }
    }

    public class GraphTableRows : List<IGraphTableRow>
    {
    }

    public interface IGraphEdge
    {
        int Eid { get; }
        int FcId { get; }
        int Oid { get; }

        int N1 { get; }
        int N2 { get; }
    }

    public interface ISwitch
    {
        int NodeId { get; }
        bool SwitchState { get; }
    }

    public interface IGraphTableAdapter
    {
        GraphTableRows QueryN1(int n1);
        IGraphTableRow QueryN1ToN2(int n1, int n2);

        IGraphEdge QueryEdge(int eid);
        double QueryEdgeWeight(Guid weightGuid, int eid);
        bool SwitchState(int nid);
        int GetNodeFcid(int nid);
        NetworkNodeType GetNodeType(int nid);

        Features QueryNodeEdgeFeatures(int n1);
        Features QueryNodeFeatures(int n1);
    }

    public enum GraphWeightDataType
    {
        Integer = 0,
        Double = 1
    }

    public enum WeightApplying
    {
        Weight = 0,
        ActualCosts = 1
    }

    public interface IGraphWeight : IPersistable
    {
        string Name { get; }
        Guid Guid { get; }
        GraphWeightDataType DataType { get; }

        GraphWeightFeatureClasses FeatureClasses { get; }
    }

    public interface INetworkCreator
    {
        IFeatureDataset FeatureDataset { get; set; }
        string NetworkName { get; set; }
        List<IFeatureClass> EdgeFeatureClasses { get; set; }
        List<IFeatureClass> NodeFeatureClasses { get; set; }

        double SnapTolerance { get; set; }
        List<int> ComplexEdgeFcIds { get; set; }

        Dictionary<int, string> SwitchNodeFcIdAndFieldnames { get; set; }

        Dictionary<int, NetworkNodeType> NodeTypeFcIds { get; set; }

        GraphWeights GraphWeights { get; set; }

        void Run();
    }

    public class GraphWeights : List<IGraphWeight>
    {

    }

    public interface IGraphWeightFeatureClass : IPersistable
    {
        int FcId { get; }
        string FieldName { get; }
        ISimpleNumberCalculation SimpleNumberCalculation { get; }
    }

    public class GraphWeightFeatureClasses : List<IGraphWeightFeatureClass>
    {
        public void Remove(int fcId)
        {
            GraphWeightFeatureClasses tmp = new GraphWeightFeatureClasses();
            foreach (IGraphWeightFeatureClass gwfc in this)
            {
                if (gwfc.FcId == fcId)
                    tmp.Add(gwfc);
            }

            foreach (IGraphWeightFeatureClass gwfc in tmp)
                this.Remove(gwfc);
        }

        public IGraphWeightFeatureClass this[int fcId]
        {
            get
            {
                foreach (IGraphWeightFeatureClass gwfc in this)
                    if (gwfc.FcId == fcId)
                        return gwfc;

                return null;
            }
            set
            {
                if (this[fcId] != null)
                    Remove(fcId);

                base.Add(value);
            }
        }

        new public void Add(IGraphWeightFeatureClass gwfc)
        {
            if (gwfc == null)
                return;

            this[gwfc.FcId] = gwfc;
        }
    }
}
