using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Geometry;
using Newtonsoft.Json;

namespace gView.Framework.UI.Dialogs.Network
{
    public partial class SelectFeatureclassesControl : UserControl
    {
        private IFeatureDataset _dataset;

        public SelectFeatureclassesControl(IFeatureDataset dataset)
        {
            InitializeComponent();
            _dataset = dataset;
        }

        #region Events
        async private void SelectFeatureclassesControl_Load(object sender, EventArgs e)
        {
            if (_dataset != null)
            {
                foreach (IDatasetElement element in await _dataset.Elements())
                {
                    if (element == null || !(element.Class is IFeatureClass))
                        continue;

                    IFeatureClass fc = (IFeatureClass)element.Class;
                    if (fc.GeometryType == GeometryType.Polyline)
                    {
                        lstEdges.Items.Add(new FcListViewItem(fc));
                    }
                    else if (fc.GeometryType == GeometryType.Point)
                    {
                        lstNodes.Items.Add(new FcListViewItem(fc));
                    }
                }
            }
        }
        #endregion

        #region Properties

        public string NetworkName
        {
            get { return txtNetworkName.Text; }
            set { txtNetworkName.Text = value; }
        }
        public List<IFeatureClass> EdgeFeatureclasses
        {
            get
            {
                List<IFeatureClass> fcs = new List<IFeatureClass>();
                foreach (FcListViewItem item in lstEdges.CheckedItems)
                {
                    fcs.Add(item.Featureclass);
                }
                return fcs;
            }
        }
        public List<IFeatureClass> NodeFeatureclasses
        {
            get
            {
                List<IFeatureClass> fcs = new List<IFeatureClass>();
                foreach (FcListViewItem item in lstNodes.CheckedItems)
                {
                    fcs.Add(item.Featureclass);
                }
                return fcs;
            }
        }

        public Serialized Serialize
        {
            get
            {
                return new Serialized()
                {
                    NetworkName = this.NetworkName,
                    EdgeFeatureclasses = this.EdgeFeatureclasses.Select(m => m.Name).ToArray(),
                    NodeFeatureclasses = this.NodeFeatureclasses.Select(m => m.Name).ToArray()
                };
            }
            set
            {
                if (value == null)
                    return;

                this.NetworkName = value.NetworkName;
                foreach (FcListViewItem item in lstEdges.Items)
                    item.Checked = false;
                if (value.EdgeFeatureclasses != null)
                {
                    foreach (var fcName in value.EdgeFeatureclasses)
                    {
                        foreach (FcListViewItem item in lstEdges.Items)
                        {
                            if (item.Featureclass.Name == fcName)
                                item.Checked = true;
                        }
                    }
                }
                foreach (FcListViewItem item in lstNodes.Items)
                    item.Checked = false;
                if (value.NodeFeatureclasses != null)
                {
                    foreach (var fcName in value.NodeFeatureclasses)
                    {
                        foreach (FcListViewItem item in lstNodes.Items)
                        {
                            if (item.Featureclass.Name == fcName)
                                item.Checked = true;
                        }
                    }
                }
            }
        }

        #endregion

        #region ItemClasses
        internal class FcListViewItem : ListViewItem
        {
            private IFeatureClass _fc;

            public FcListViewItem(IFeatureClass fc)
            {
                _fc = fc;

                base.Text = _fc.Name;
            }

            public IFeatureClass Featureclass
            {
                get { return _fc; }
            }
        }
        #endregion

        #region Serializer Class

        public class Serialized
        {
            [JsonProperty(PropertyName = "networkname")]
            public string NetworkName { get; set; }
            [JsonProperty(PropertyName = "edges")]
            public string[] EdgeFeatureclasses { get; set; }

            [JsonProperty(PropertyName = "nodes")]
            public string[] NodeFeatureclasses { get; set; }
        }

        #endregion
    }
}
