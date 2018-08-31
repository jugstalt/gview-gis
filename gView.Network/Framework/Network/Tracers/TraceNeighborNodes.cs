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
    [RegisterPlugIn("3D20193C-8E5E-4F45-90F3-12DFE8AD3779")]
    public class TraceNeighborNodes : INetworkTracer, INetworkTracerProperties, IProgressReporterEvent
    {
        private DynamicProperties _properties = new DynamicProperties();

        #region INetworkTracer Member

        public string Name
        {
            get { return "Trace Neighbor Nodes"; }
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

            NetworkTracerOutputCollection outputCollection = new NetworkTracerOutputCollection();
            List<int> edgeIds = new List<int>();
            NetworkPathOutput pathOutput = new NetworkPathOutput();

            List<int> neighborNodeFcIds = new List<int>();
            Dictionary<int, string> neighborFcs = new Dictionary<int, string>();
            //neighborNodeFcIds.Add(-1);
            foreach (DynamicProperties.CustomBooleanProperty prop in _properties)
            {
                if (prop.Value == false)
                    continue;

                int fcid = network.NetworkClassId(prop.Name);
                if (fcid < 0)
                    continue;

                neighborNodeFcIds.Add(fcid);
                neighborFcs.Add(fcid, prop.Name);
            }

            Dijkstra dijkstra = new Dijkstra(cancelTraker);
            dijkstra.reportProgress += this.ReportProgress;
            dijkstra.ApplySwitchState = input.Contains(NetworkTracerInputType.IgnoreSwitches) == false &&
                                        network.HasDisabledSwitches;
            dijkstra.TargetNodeFcIds = neighborNodeFcIds;
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
                if (neighborNodeFcIds.Contains(gt.GetNodeFcid(endNode.Id)))
                {
                    IFeature nodeFeature = network.GetNodeFeature(endNode.Id);
                    if (nodeFeature != null && nodeFeature.Shape is IPoint)
                    {
                        string fcName = neighborFcs.ContainsKey(fcId) ?
                            neighborFcs[fcId] : String.Empty;

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

        #region INetworkTracerProperties Member

        public object NetworkTracerProperties(INetworkFeatureClass network, NetworkTracerInputCollection input)
        {
            if (network == null)
                return null;

            _properties.Clear();
            List<IFeatureClass> fcs = network.NetworkClasses;
            if (fcs != null)
            {
                foreach (IFeatureClass fc in fcs)
                {
                    if (fc.GeometryType == geometryType.Point)
                    {
                        _properties.AddBoolProperty(fc.Name);
                    }
                }
            }
            return _properties;
        }

        #endregion

        #region IProgressReporterEvent Member

        public event ProgressReporterEvent ReportProgress;

        #endregion

        #region HelperClasses
        private class DynamicProperties : List<DynamicProperties.CustomBooleanProperty>, System.ComponentModel.ICustomTypeDescriptor
        {
            public void AddBoolProperty(string name)
            {
                this.Add(new CustomBooleanProperty(name));
            }

            #region ICustomTypeDescriptor Member

            public String GetClassName()
            {
                return TypeDescriptor.GetClassName(this, true);
            }

            public AttributeCollection GetAttributes()
            {
                return TypeDescriptor.GetAttributes(this, true);
            }

            public String GetComponentName()
            {
                return TypeDescriptor.GetComponentName(this, true);
            }

            public TypeConverter GetConverter()
            {
                return TypeDescriptor.GetConverter(this, true);
            }

            public EventDescriptor GetDefaultEvent()
            {
                return TypeDescriptor.GetDefaultEvent(this, true);
            }

            public PropertyDescriptor GetDefaultProperty()
            {
                return TypeDescriptor.GetDefaultProperty(this, true);
            }

            public object GetEditor(Type editorBaseType)
            {
                return TypeDescriptor.GetEditor(this, editorBaseType, true);
            }

            public EventDescriptorCollection GetEvents(Attribute[] attributes)
            {
                return TypeDescriptor.GetEvents(this, attributes, true);
            }

            public EventDescriptorCollection GetEvents()
            {
                return TypeDescriptor.GetEvents(this, true);
            }

            public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                PropertyDescriptor[] newProps = new PropertyDescriptor[this.Count];
                for (int i = 0; i < this.Count; i++)
                {
                    newProps[i] = new CustomBooleanPropertyDescriptor(this[i]);
                }

                return new PropertyDescriptorCollection(newProps);
            }

            public PropertyDescriptorCollection GetProperties()
            {

                return TypeDescriptor.GetProperties(this, true);

            }

            public object GetPropertyOwner(PropertyDescriptor pd)
            {
                return this;
            }

            #endregion

            #region Helper Classes
            public class CustomBooleanProperty
            {
                private string _name;
                private bool _value;

                public CustomBooleanProperty(string name)
                {
                    _name = name;
                    _value = true;
                }

                public string Name { get { return _name; } }
                public bool Value { get { return _value; } set { _value = value; } }
            }
            public class CustomBooleanPropertyDescriptor : PropertyDescriptor
            {
                CustomBooleanProperty _prop;
                public CustomBooleanPropertyDescriptor(CustomBooleanProperty prop)
                    : base(prop.Name, null)
                {
                    _prop = prop;
                }

                #region PropertyDescriptor specific

                public override bool CanResetValue(object component)
                {
                    return false;
                }

                public override Type ComponentType
                {
                    get
                    {
                        return null;
                    }
                }

                public override object GetValue(object component)
                {
                    return _prop.Value;
                }

                public override string Description
                {
                    get
                    {
                        return base.Name;
                    }
                }

                public override string Category
                {
                    get
                    {
                        return base.Category;
                    }
                }

                public override string DisplayName
                {
                    get
                    {
                        return base.DisplayName;
                    }

                }

                public override bool IsReadOnly
                {
                    get
                    {
                        return false;
                    }
                }

                public override void ResetValue(object component)
                {
                    //Have to implement
                }

                public override bool ShouldSerializeValue(object component)
                {
                    return false;
                }

                public override void SetValue(object component, object value)
                {
                    _prop.Value = (bool)value;
                }

                public override Type PropertyType
                {
                    get { return typeof(bool); }
                }

                #endregion


            }
            #endregion
        }
        #endregion
    }
}
