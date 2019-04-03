using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Network;
using Newtonsoft.Json;
using gView.Framework.system;
using System.IO;
using System.Reflection;

namespace gView.Framework.UI.Dialogs.Network
{
    public partial class FormNewNetworkclass : Form, ISerializableObject
    {
        private IFeatureDataset _dataset;
        private SelectFeatureclassesControl _selecteFeturesclasses;
        private NetworkToleranceControl _tolerance;
        private ComplexEdgesControl _complexEdges;
        private NetworkEdgeWeightsControl _edgeWeights;
        private NetworkSwitchesControl _switches;
        private Type _networkCreatorType;

        public FormNewNetworkclass()
            : this(null, null)
        {
        }

        public FormNewNetworkclass(IFeatureDataset dataset, Type networkCreatorType)
        {
            InitializeComponent();
            _networkCreatorType = networkCreatorType;
            this.FeatureDataset = dataset;
        }

        private void FormNewNetworkclass_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void Init()
        {
            if (this.FeatureDataset != null)
                wizardControl1.Init();
        }

        #region Properties

        public IFeatureDataset FeatureDataset
        {
            get
            {
                return _dataset;
            }
            set
            {
                if (_dataset != null)
                    throw new Exception("Dataset already set...");
                if (value != null)
                {
                    _dataset = value;

                    _selecteFeturesclasses = new SelectFeatureclassesControl(_dataset);
                    _tolerance = new NetworkToleranceControl();
                    _complexEdges = new ComplexEdgesControl(_dataset, _selecteFeturesclasses);
                    _switches = new NetworkSwitchesControl(_dataset, _selecteFeturesclasses);
                    _edgeWeights = new NetworkEdgeWeightsControl(_dataset, _selecteFeturesclasses);

                    wizardControl1.AddPage(_selecteFeturesclasses);
                    wizardControl1.AddPage(_tolerance);
                    wizardControl1.AddPage(_complexEdges);
                    wizardControl1.AddPage(_switches);
                    wizardControl1.AddPage(_edgeWeights);

                    Init();
                }
            }
        }

        public string NetworkName
        {
            get { return _selecteFeturesclasses.NetworkName; }
        }
        public List<IFeatureClass> EdgeFeatureclasses
        {
            get
            {
                return _selecteFeturesclasses.EdgeFeatureclasses;
            }
        }
        public List<IFeatureClass> NodeFeatureclasses
        {
            get
            {
                return _selecteFeturesclasses.NodeFeatureclasses;
            }
        }
        public double SnapTolerance
        {
            get { return _tolerance.Tolerance; }
        }
        public List<int> ComplexEdgeFcIds
        {
            get { return _complexEdges.ComplexEdgeFcIds; }
        }
        public GraphWeights GraphWeights
        {
            get { return _edgeWeights.GraphWeights; }
        }
        public Dictionary<int, string> SwitchNodeFcIds
        {
            get
            {
                return _switches.SwitchNodeFcIds;
            }
        }
        public Dictionary<int, NetworkNodeType> NetworkNodeTypeFcIds
        {
            get
            {
                return _switches.NetworkNodeTypeFcIds;
            }
        }
        #endregion

        public Serialized Serialize
        {
            get
            {
                return new Serialized()
                {
                    ConnectionString = _dataset.ConnectionString,
                    DatasetGuid = PlugInManager.PlugInID(_dataset).ToString(),
                    DatasetName = _dataset.DatasetName,
                    NetworkCreatorAssembly = new FileInfo(this._networkCreatorType.Assembly.Location).Name,
                    NetworkCreatorType = this._networkCreatorType.ToString(),

                    FeatureClasses = _selecteFeturesclasses.Serialize,
                    NetworkTolerance = _tolerance.Serialize,
                    ComplexEdges = _complexEdges.Serialize,
                    Nodes = _switches.Serialize,
                    EdgeWeights = _edgeWeights.Serialize
                };
            }
            set
            {
                if (value == null)
                    return;

                if(this.FeatureDataset==null)
                {
                    IFeatureDataset dataset = PlugInManager.Create(new Guid(value.DatasetGuid)) as IFeatureDataset;
                    if (dataset == null)
                        throw new Exception("Unable to crete dataset");
                    dataset.ConnectionString = value.ConnectionString;

                    this.FeatureDataset = dataset;
                }

                _selecteFeturesclasses.Serialize = value.FeatureClasses;
                _tolerance.Serialize = value.NetworkTolerance;
                _complexEdges.OnShowWizardPage();
                _complexEdges.Serialize = value.ComplexEdges;
                _switches.OnShowWizardPage();
                _switches.Serialize = value.Nodes;
                _edgeWeights.Serialize = value.EdgeWeights;

                var assembly = Assembly.LoadFrom(SystemVariables.ApplicationDirectory + @"\" + value.NetworkCreatorAssembly);
                _networkCreatorType = (assembly.CreateInstance(value.NetworkCreatorType) as INetworkCreator)?.GetType(); 
            }
        }

        #region Serialze Class

        public class Serialized
        {
            [JsonProperty(PropertyName = "connectionstring")]
            public string ConnectionString { get; set; }
            [JsonProperty(PropertyName = "dataset_guid")]
            public string DatasetGuid { get; set; }
            [JsonProperty(PropertyName = "dataset_name")]
            public string DatasetName { get; set; }

            [JsonProperty(PropertyName = "featureses")]
            public SelectFeatureclassesControl.Serialized FeatureClasses { get; set; }

            [JsonProperty(PropertyName = "tolerance")]
            public NetworkToleranceControl.Serialized NetworkTolerance { get; set; }

            [JsonProperty(PropertyName = "complex_edges")]
            public ComplexEdgesControl.Serialized ComplexEdges { get; set; }

            [JsonProperty(PropertyName = "nodes")]
            public NetworkSwitchesControl.Serialized Nodes { get; set; }

            [JsonProperty(PropertyName = "edge_weights")]
            public NetworkEdgeWeightsControl.Seriazlized EdgeWeights { get; set; }

            [JsonProperty(PropertyName = "network_createor_assembly")]
            public string NetworkCreatorAssembly { get; set; }

            [JsonProperty(PropertyName = "network_createor_type")]
            public string NetworkCreatorType { get; set; }
        }

        #endregion

        private void btnSave_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(dlg.FileName, JsonConvert.SerializeObject(this.Serialize, Formatting.Indented));
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.Serialize = JsonConvert.DeserializeObject<Serialized>(System.IO.File.ReadAllText(dlg.FileName));
            }
        }

        #region ISerializableObject


        public void DeserializeObject(string config)
        {
            this.Serialize = JsonConvert.DeserializeObject<Serialized>(config);
        }

        public string SerializeObject()
        {
            return JsonConvert.SerializeObject(this.Serialize);
        }

        #endregion
    }
}
