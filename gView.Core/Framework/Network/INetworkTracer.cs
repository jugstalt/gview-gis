using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using gView.Framework.Geometry;

namespace gView.Framework.Network
{
    #region NetworkTracerInput Classes
    public enum NetworkTracerInputType
    {
        Any = 0,
        SourceNode = 1,
        SoruceEdge = 2,
        SinkNode = 3,
        Weight = 4,
        IgnoreSwitches = 5,
        AllowedNodeIds = 6,
        ForbiddenTargetNodeIds = 7,
        ForbiddenStartNodeEdgeIds = 8,
        ForbiddenEdgeIds = 9,
        AppendNodeFlags = 10,
        BarrierNodes = 11
    }
    public interface INetworkTracerInput
    {
    }

    public interface INetworkNode
    {
        int NodeId { get; }
    }

    public interface INetworkEdge
    {
        int EdgeId { get; }
    }

    public class NetworkTracerInputCollection : List<INetworkTracerInput>
    {
        public List<INetworkTracerInput> Collect(NetworkTracerInputType type)
        {
            List<INetworkTracerInput> ret = new List<INetworkTracerInput>();

            foreach (INetworkTracerInput input in this)
            {
                if (input == null)
                    continue;

                switch (type)
                {
                    case NetworkTracerInputType.Any:
                        ret.Add(input);
                        break;
                    case NetworkTracerInputType.SinkNode:
                        if (input is NetworkSinkInput)
                            ret.Add(input);
                        break;
                    case NetworkTracerInputType.SourceNode:
                        if (input is NetworkSourceInput && !(input is NetworkSinkInput))
                            ret.Add(input);
                        break;
                    case NetworkTracerInputType.SoruceEdge:
                        if (input is NetworkSourceEdgeInput)
                            ret.Add(input);
                        break;
                    case NetworkTracerInputType.Weight:
                        if (input is NetworkWeighInput)
                            ret.Add(input);
                        break;
                    case NetworkTracerInputType.IgnoreSwitches:
                        if (input is NetworkIgnoreSwitchState)
                            ret.Add(input);
                        break;
                    case NetworkTracerInputType.AllowedNodeIds:
                        if (input is NetworkInputAllowedNodeIds)
                            ret.Add(input);
                        break;
                    case NetworkTracerInputType.ForbiddenTargetNodeIds:
                        if (input is NetworkInputForbiddenTargetNodeIds)
                            ret.Add(input);
                        break;
                    case NetworkTracerInputType.ForbiddenStartNodeEdgeIds:
                        if (input is NetworkInputForbiddenStartNodeEdgeIds)
                            ret.Add(input);
                        break;
                    case NetworkTracerInputType.ForbiddenEdgeIds:
                        if (input is NetworkInputForbiddenEdgeIds)
                            ret.Add(input);
                        break;
                    case NetworkTracerInputType.AppendNodeFlags:
                        if (input is NetworkAppendNodeFlagsInput)
                            ret.Add(input);
                        break;
                    case NetworkTracerInputType.BarrierNodes:
                        if (input is NetworkBarrierNodeInput)
                            ret.Add(input);
                        break;
                }
            }

            return ret;
        }

        public List<int> CollectNodeIds(NetworkTracerInputType type)
        {
            List<int> ret = new List<int>();

            foreach (INetworkTracerInput input in this)
            {
                if (!(input is INetworkNode))
                    continue;

                int id = ((INetworkNode)input).NodeId;

                switch (type)
                {
                    case NetworkTracerInputType.Any:
                        ret.Add(id);
                        break;
                    case NetworkTracerInputType.SinkNode:
                        if (input is NetworkSinkInput)
                            ret.Add(id);
                        break;
                    case NetworkTracerInputType.SourceNode:
                        if (input is NetworkSourceInput && !(input is NetworkSinkInput))
                            ret.Add(id);
                        break;
                    case NetworkTracerInputType.Weight:
                        if (input is NetworkWeighInput)
                            ret.Add(id);
                        break;
                    case NetworkTracerInputType.IgnoreSwitches:
                        if (input is NetworkIgnoreSwitchState)
                            ret.Add(id);
                        break;
                    case NetworkTracerInputType.AllowedNodeIds:
                        if (input is NetworkInputAllowedNodeIds)
                            ret.Add(id);
                        break;
                    case NetworkTracerInputType.ForbiddenTargetNodeIds:
                        if (input is NetworkInputForbiddenTargetNodeIds)
                            ret.Add(id);
                        break;
                    case NetworkTracerInputType.AppendNodeFlags:
                        if (input is NetworkAppendNodeFlagsInput)
                            ret.Add(id);
                        break;
                    case NetworkTracerInputType.BarrierNodes:
                        if (input is NetworkBarrierNodeInput)
                            ret.Add(id);
                        break;
                }
            }

            return ret;
        }

        public List<int> CollectEdgeIds(NetworkTracerInputType type)
        {
            List<int> ret = new List<int>();

            foreach (INetworkTracerInput input in this)
            {
                if (!(input is INetworkEdge))
                    continue;

                int id = ((INetworkEdge)input).EdgeId;

                switch (type)
                {
                    case NetworkTracerInputType.Any:
                        ret.Add(id);
                        break;
                    case NetworkTracerInputType.SoruceEdge:
                        if (input is NetworkSourceEdgeInput)
                            ret.Add(id);
                        break;
                    case NetworkTracerInputType.Weight:
                        if (input is NetworkWeighInput)
                            ret.Add(id);
                        break;
                    case NetworkTracerInputType.ForbiddenStartNodeEdgeIds:
                        if (input is NetworkInputForbiddenStartNodeEdgeIds)
                            ret.Add(id);
                        break;
                    case NetworkTracerInputType.ForbiddenEdgeIds:
                        if (input is NetworkInputForbiddenEdgeIds)
                            ret.Add(id);
                        break;
                }
            }

            return ret;
        }

        public bool Contains(NetworkTracerInputType type)
        {
            List<INetworkTracerInput> list = Collect(type);
            return list.Count > 0;
        }
    }

    public class NetworkSourceInput : INetworkTracerInput, INetworkNode
    {
        private int _nodeId = -1;

        public NetworkSourceInput()
        {
        }
        public NetworkSourceInput(int nodeId)
        {
            _nodeId = nodeId;
        }

        public int NodeId
        {
            get { return _nodeId; }
        }
    }

    public class NetworkSourceEdgeInput : INetworkTracerInput, INetworkEdge
    {
        private int _edgeId = -1;
        private IPoint _point = null;

        public NetworkSourceEdgeInput()
        {
        }
        public NetworkSourceEdgeInput(int nodeId)
        {
            _edgeId = nodeId;
        }
        public NetworkSourceEdgeInput(int nodeId, IPoint point)
            : this(nodeId)
        {
            _point = point;
        }

        public int EdgeId
        {
            get { return _edgeId; }
        }

        public IPoint Point
        {
            get { return _point; }
        }
    }

    public class NetworkSinkInput : NetworkSourceInput
    {
        public NetworkSinkInput()
            : base()
        {
        }
        public NetworkSinkInput(int nodeId)
            : base(nodeId)
        {
        }
    }

    public class NetworkWeighInput : INetworkTracerInput
    {
        private IGraphWeight _weight;
        private WeightApplying _weightApplying;

        public NetworkWeighInput(IGraphWeight weight, WeightApplying weightApplying)
        {
            _weight = weight;
            _weightApplying = weightApplying;
        }

        public IGraphWeight Weight
        {
            get { return _weight; }
        }
        public WeightApplying WeightApplying
        {
            get { return _weightApplying; }
        }
    }

    public class NetworkIgnoreSwitchState : INetworkTracerInput
    {
    }

    public class NetworkInputIds
    {
        List<int> _ids = new List<int>();

        protected NetworkInputIds(List<int> ids)
        {
            _ids = ids;
        }

        public List<int> Ids
        {
            get { return _ids; }
        }
    }
    public class NetworkInputAllowedNodeIds : NetworkInputIds, INetworkTracerInput
    {
        public NetworkInputAllowedNodeIds(List<int> ids)
            : base(ids)
        {
        }
    }
    public class NetworkInputForbiddenTargetNodeIds : NetworkInputIds, INetworkTracerInput
    {
        public NetworkInputForbiddenTargetNodeIds(List<int> ids)
            : base(ids)
        {
        }
    }
    public class NetworkInputForbiddenStartNodeEdgeIds : NetworkInputIds, INetworkTracerInput
    {
        public NetworkInputForbiddenStartNodeEdgeIds(List<int> ids)
            : base(ids)
        {
        }
    }
    public class NetworkInputForbiddenEdgeIds : NetworkInputIds, INetworkTracerInput
    {
        public NetworkInputForbiddenEdgeIds(List<int> ids)
            : base(ids)
        {
        }
    }

    public class NetworkAppendNodeFlagsInput : INetworkTracerInput
    {
    }

    public class NetworkBarrierNodeInput : INetworkTracerInput, INetworkNode
    {
        private int _nodeId = -1;

        public NetworkBarrierNodeInput()
        {
        }
        public NetworkBarrierNodeInput(int nodeId)
        {
            _nodeId = nodeId;
        }

        public int NodeId
        {
            get { return _nodeId; }
        }
    }
    #endregion

    #region NetworkTracerOutput Classes
    public interface INetworkTracerOutput
    {
    }

    public class NetworkTracerOutputCollection : List<INetworkTracerOutput>
    {
    }

    public class NetworkEdgeOutput : INetworkTracerOutput
    {
        private int _eid;
        private object _userData = null;

        public NetworkEdgeOutput(int eid)
            : this(eid, null)
        {
        }
        public NetworkEdgeOutput(int eid, object userData)
        {
            _eid = eid;
            _userData = userData;
        }

        public int EdgeId
        {
            get { return _eid; }
        }

        public object UserData
        {
            get { return _userData; }
        }
    }

    public class NetworkEdgeCollectionOutput : List<NetworkEdgeOutput>, INetworkTracerOutput
    {
        public NetworkEdgeCollectionOutput()
            : base()
        { }
        public NetworkEdgeCollectionOutput(List<int> edgeIds)
            : base()
        {
            if (edgeIds != null)
                foreach (int edgeId in edgeIds)
                    this.Add(new NetworkEdgeOutput(edgeId));
        }
    }
    public class NetworkPathOutput : NetworkEdgeCollectionOutput
    {
    }

    public class NetworkNodeOutput : INetworkTracerOutput
    {
        private int _nodeId;
        private object _userData = null;

        public NetworkNodeOutput(int nodeId)
            : this(nodeId, null)
        {
        }
        public NetworkNodeOutput(int nodeId, object userData)
        {
            _nodeId = nodeId;
            _userData = userData;
        }

        public int NodeId
        {
            get { return _nodeId; }
        }

        public object UserData
        {
            get { return _userData; }
        }
    }

    public class NetworkNodesOutput : List<NetworkNodeOutput>, INetworkTracerOutput
    {
    }

    public class NetworkPolylineOutput : INetworkTracerOutput
    {
        private IPolyline _polyline;

        public NetworkPolylineOutput(IPolyline polyline)
        {
            _polyline = polyline;
        }

        public IPolyline Polyline
        {
            get { return _polyline; }
        }
    }

    public class NetworkEdgePolylineOutput : INetworkTracerOutput
    {
        private IPolyline _polyline;
        private int _eid;

        public NetworkEdgePolylineOutput(int edgeId, IPolyline polyline)
        {
            _eid = edgeId;
            _polyline = polyline;
        }

        public IPolyline Polyline
        {
            get { return _polyline; }
        }

        public int EdgeId
        {
            get { return _eid; }
        }
    }

    public class NetworkFlagOutput : INetworkTracerOutput
    {
        private IPoint _location;
        private object _userData;

        public NetworkFlagOutput(IPoint location)
            : this(location, null)
        {
        }
        public NetworkFlagOutput(IPoint location, object userData)
        {
            _location = location;
            _userData = userData;
        }

        public IPoint Location
        {
            get { return _location; }
        }
        public object UserData
        {
            get { return _userData; }
        }

        #region DataClasses
        public class NodeFeatureData
        {
            private int _nodeId;
            private int _fcId;
            private int _oid;
            private string _text;

            public NodeFeatureData(int nodeId, int fcId, int oid)
                : this(nodeId, fcId, oid, String.Empty)
            {
            }
            public NodeFeatureData(int nodeId, int fcId, int oid, string text)
            {
                _nodeId = nodeId;
                _fcId = fcId;
                _oid = oid;
                _text = text;
            }

            public int NodeId { get { return _nodeId; } }
            public int FeatureClassId { get { return _fcId; } }
            public int ObjectId { get { return _oid; } }
            public string Text { get { return _text; } }

            public override string ToString()
            {
                return Text;
            }
        }
        #endregion
    }



    public class NetworkNodeFlagOuput : INetworkTracerOutput
    {
        private int _nid;
        private object _userData;

        public NetworkNodeFlagOuput(int nodeId)
            : this(nodeId, null)
        {
        }
        public NetworkNodeFlagOuput(int nodeId, object userData)
        {
            _nid = nodeId;
            _userData = userData;
        }

        public int NodeId
        {
            get { return _nid; }
        }
        public object UserData
        {
            get { return _userData; }
        }
    }
    #endregion

    public interface INetworkTracer
    {
        string Name { get; }
        bool CanTrace(NetworkTracerInputCollection input);
        NetworkTracerOutputCollection Trace(INetworkFeatureClass network, NetworkTracerInputCollection input, ICancelTracker cancelTraker);
    }

    public interface INetworkTracerProperties
    {
        object NetworkTracerProperties(INetworkFeatureClass network, NetworkTracerInputCollection input);
    }

    public enum NetworkNodeType
    {
        Unknown = 0,
        Source = 1,
        Sink = 2,

        Traffic_Cross = 100,
        Traffic_Roundabout = 101,
        Traffic_Stop = 102,
        Traffic_Light = 103,

        Electrictiy_Station = 200,
        Electricity_Customer = 201,
        Electricity_JunctionBox = 202,

        Gas_Station = 300,
        Gas_Customer = 301,
        Gas_Switch = 302,
        Gas_Stop = 303
    }
}
