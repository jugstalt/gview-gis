using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Network;
using gView.Framework.system;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace gView.Framework.UI.Dialogs.Network
{
    public class CreateNetworkExcecute : ISerializableExecute
    {
        private FormNewNetworkclass.Serialized _serialized = null;
        #region Properties

        private IFeatureDataset FeatureDataset
        {
            get; set;
        }

        private IFeatureDatabase3 FeatureDatabase { get; set; }

        async private Task<List<IFeatureClass>> EdgeFeatureclasses()
        {
            List<IFeatureClass> edges = new List<IFeatureClass>();
            foreach (var datasetElement in await this.FeatureDataset.Elements())
            {
                if (datasetElement.Class is IFeatureClass && _serialized.FeatureClasses.EdgeFeatureclasses.Contains(datasetElement.Class.Name))
                {
                    edges.Add((IFeatureClass)datasetElement.Class);
                }
            }
            return edges;
        }

        async private Task<List<IFeatureClass>> NodeFeatureclasses()
        {
            List<IFeatureClass> edges = new List<IFeatureClass>();
            foreach (var datasetElement in await this.FeatureDataset.Elements())
            {
                if (datasetElement.Class is IFeatureClass && _serialized.FeatureClasses.NodeFeatureclasses.Contains(datasetElement.Class.Name))
                {
                    edges.Add((IFeatureClass)datasetElement.Class);
                }
            }
            return edges;
        }

        private double SnapTolerance
        {
            get { return _serialized.NetworkTolerance.UseTolerance ? _serialized.NetworkTolerance.Tolerance : double.Epsilon; }
        }

        async private Task<List<int>> ComplexEdgeFcIds()
        {
            List<int> fcids = new List<int>();
            if (_serialized.ComplexEdges.UserComplexEdges)
            {
                foreach (var fcName in _serialized.ComplexEdges.ComplexEdgeNames)
                {
                    fcids.Add(await this.FeatureDatabase.GetFeatureClassID(fcName));
                }
            }
            return fcids;
        }

        private GraphWeights GraphWeights
        {
            get
            {
                return new GraphWeights(); // ToDo
            }
        }

        async private Task<Dictionary<int, string>> SwitchNodeFcIds()
        {
            Dictionary<int, string> switchNodes = new Dictionary<int, string>();
            foreach (var switchNode in _serialized.Nodes.Rows.Where(m => m.IsSwitch))
            {
                switchNodes.Add(await this.FeatureDatabase.GetFeatureClassID(switchNode.FeatureclassName), switchNode.FieldName == "<none>" ? String.Empty : switchNode.FieldName);
            }
            return switchNodes;
        }

        async private Task<Dictionary<int, NetworkNodeType>> NetworkNodeTypeFcIds()
        {
            Dictionary<int, NetworkNodeType> nodeTypes = new Dictionary<int, NetworkNodeType>();
            foreach (var switchNode in _serialized.Nodes.Rows)
            {
                nodeTypes.Add(await this.FeatureDatabase.GetFeatureClassID(switchNode.FeatureclassName), (NetworkNodeType)Enum.Parse(typeof(NetworkNodeType), switchNode.NodeType, true));
            }
            return nodeTypes;
        }

        #endregion

        #region ISerializableExecute

        async public Task DeserializeObject(string config)
        {
            _serialized = JsonConvert.DeserializeObject<FormNewNetworkclass.Serialized>(config);
        }



        public string SerializeObject()
        {
            return JsonConvert.SerializeObject(_serialized);
        }

        async public Task Execute(ProgressReporterEvent reporter)
        {
            if (_serialized == null)
            {
                throw new Exception("can't execute. No config!");
            }

            #region Instance Creator

            var assembly = Assembly.LoadFrom(SystemVariables.ApplicationDirectory + @"\" + _serialized.NetworkCreatorAssembly);
            if (assembly == null)
            {
                throw new Exception("Assembly not found: " + _serialized.NetworkCreatorAssembly);
            }

            var creator = assembly.CreateInstance(_serialized.NetworkCreatorType) as INetworkCreator;
            if (creator == null)
            {
                throw new Exception("Type " + _serialized.NetworkCreatorType + " is not a network creator");
            }

            #endregion

            #region Open Dataset

            IFeatureDataset dataset = PlugInManager.Create(new Guid(_serialized.DatasetGuid)) as IFeatureDataset;
            if (dataset == null)
            {
                throw new Exception("Unable to crete dataset");
            }

            await dataset.SetConnectionString(_serialized.ConnectionString);
            await dataset.Open();

            this.FeatureDataset = dataset;
            this.FeatureDatabase = dataset.Database as IFeatureDatabase3;
            if (this.FeatureDatabase == null)
            {
                throw new Exception("Featuredatabase no implements IFeatureDatabase3");
            }

            #endregion

            creator.NetworkName = _serialized.FeatureClasses.NetworkName;
            creator.FeatureDataset = this.FeatureDataset;
            creator.EdgeFeatureClasses = await this.EdgeFeatureclasses();
            creator.NodeFeatureClasses = await this.NodeFeatureclasses();
            creator.SnapTolerance = this.SnapTolerance;
            creator.ComplexEdgeFcIds = await this.ComplexEdgeFcIds();
            creator.GraphWeights = this.GraphWeights;
            creator.SwitchNodeFcIdAndFieldnames = await this.SwitchNodeFcIds();
            creator.NodeTypeFcIds = await this.NetworkNodeTypeFcIds();

            if (reporter != null && creator is IProgressReporter)
            {
                ((IProgressReporter)creator).ReportProgress += (ProgressReport progressEventReport) =>
                {
                    reporter(progressEventReport);
                };
            }

            await creator.Run();
        }

        #endregion
    }
}
