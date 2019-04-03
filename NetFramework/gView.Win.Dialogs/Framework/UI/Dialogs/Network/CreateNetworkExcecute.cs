using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Network;
using gView.Framework.system;
using gView.Framework.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.UI.Dialogs.Network
{
    public class CreateNetworkExcecute : ISerializableExecute
    {
        private FormNewNetworkclass.Serialized _serialized = null;
        #region Properties

        private IFeatureDataset FeatureDataset
        {
            get;set;
        }

        private IFeatureDatabase3 FeatureDatabase { get; set; }

        private List<IFeatureClass> EdgeFeatureclasses
        {
            get
            {
                List<IFeatureClass> edges = new List<IFeatureClass>();
                foreach(var datasetElement in this.FeatureDataset.Elements().Result)
                {
                    if (datasetElement.Class is IFeatureClass && _serialized.FeatureClasses.EdgeFeatureclasses.Contains(datasetElement.Class.Name))
                        edges.Add((IFeatureClass)datasetElement.Class);
                }
                return edges;
            }
        }

        private List<IFeatureClass> NodeFeatureclasses
        {
            get
            {
                List<IFeatureClass> edges = new List<IFeatureClass>();
                foreach (var datasetElement in this.FeatureDataset.Elements().Result)
                {
                    if (datasetElement.Class is IFeatureClass && _serialized.FeatureClasses.NodeFeatureclasses.Contains(datasetElement.Class.Name))
                        edges.Add((IFeatureClass)datasetElement.Class);
                }
                return edges;
            }
        }

        private double SnapTolerance
        {
            get { return _serialized.NetworkTolerance.UseTolerance ? _serialized.NetworkTolerance.Tolerance : double.Epsilon; }
        }

        private List<int> ComplexEdgeFcIds
        {
            get
            {
                List<int> fcids = new List<int>();
                if (_serialized.ComplexEdges.UserComplexEdges)
                {
                    foreach(var fcName in _serialized.ComplexEdges.ComplexEdgeNames)
                    {
                        fcids.Add(this.FeatureDatabase.GetFeatureClassID(fcName));
                    }
                }
                return fcids;
            }
        }

        private GraphWeights GraphWeights
        {
            get
            {
                return new GraphWeights(); // ToDo
            }
        }

        private Dictionary<int, string> SwitchNodeFcIds
        {
            get
            {
                Dictionary<int, string> switchNodes = new Dictionary<int, string>();
                foreach(var switchNode in _serialized.Nodes.Rows.Where(m=>m.IsSwitch))
                {
                    switchNodes.Add(this.FeatureDatabase.GetFeatureClassID(switchNode.FeatureclassName), switchNode.FieldName == "<none>" ? String.Empty : switchNode.FieldName);
                }
                return switchNodes;
            }
        }

        private Dictionary<int, NetworkNodeType> NetworkNodeTypeFcIds
        {
            get
            {
                Dictionary<int, NetworkNodeType> nodeTypes = new Dictionary<int, NetworkNodeType>();
                foreach(var switchNode in _serialized.Nodes.Rows)
                {
                    nodeTypes.Add(this.FeatureDatabase.GetFeatureClassID(switchNode.FeatureclassName), (NetworkNodeType)Enum.Parse(typeof(NetworkNodeType), switchNode.NodeType, true));
                }
                return nodeTypes;
            }
        }

        #endregion

        #region ISerializableExecute

        public void DeserializeObject(string config)
        {
            _serialized= JsonConvert.DeserializeObject<FormNewNetworkclass.Serialized>(config);
        }

        

        public string SerializeObject()
        {
            return JsonConvert.SerializeObject(_serialized);
        }

        public void Execute(ProgressReporterEvent reporter)
        {
            if (_serialized == null)
                throw new Exception("can't execute. No config!");

            #region Instance Creator

            var assembly = Assembly.LoadFrom(SystemVariables.ApplicationDirectory + @"\" + _serialized.NetworkCreatorAssembly);
            if (assembly == null)
                throw new Exception("Assembly not found: " + _serialized.NetworkCreatorAssembly);
            var creator = assembly.CreateInstance(_serialized.NetworkCreatorType) as INetworkCreator;
            if (creator == null)
                throw new Exception("Type " + _serialized.NetworkCreatorType + " is not a network creator");

            #endregion

            #region Open Dataset

            IFeatureDataset dataset = PlugInManager.Create(new Guid(_serialized.DatasetGuid)) as IFeatureDataset;
            if (dataset == null)
                throw new Exception("Unable to crete dataset");
            dataset.ConnectionString = _serialized.ConnectionString;
            dataset.Open();
            this.FeatureDataset = dataset;
            this.FeatureDatabase = dataset.Database as IFeatureDatabase3;
            if (this.FeatureDatabase == null)
                throw new Exception("Featuredatabase no implements IFeatureDatabase3");

            #endregion

            creator.NetworkName = _serialized.FeatureClasses.NetworkName;
            creator.FeatureDataset = this.FeatureDataset;
            creator.EdgeFeatureClasses = this.EdgeFeatureclasses;
            creator.NodeFeatureClasses = this.NodeFeatureclasses;
            creator.SnapTolerance = this.SnapTolerance;
            creator.ComplexEdgeFcIds = this.ComplexEdgeFcIds;
            creator.GraphWeights = this.GraphWeights;
            creator.SwitchNodeFcIdAndFieldnames = this.SwitchNodeFcIds;
            creator.NodeTypeFcIds = this.NetworkNodeTypeFcIds;

            if (reporter != null && creator is IProgressReporter)
            {
                ((IProgressReporter)creator).ReportProgress += (ProgressReport progressEventReport) =>
                {
                    reporter(progressEventReport);
                };
            }

            creator.Run();
        }

        #endregion
    }
}
