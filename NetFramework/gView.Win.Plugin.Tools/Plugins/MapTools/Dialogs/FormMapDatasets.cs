using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Carto;
using gView.Framework.Data;

namespace gView.Plugins.MapTools.Dialogs
{
    public partial class FormMapDatasets : Form
    {
        private IMap _map;

        public FormMapDatasets(IMap map)
        {
            InitializeComponent();

            _map = map;

            InitUI();
        }

        private void InitUI()
        {
            tvDatasets.Nodes.Clear();

            MapTreeNode root = new MapTreeNode() { Map = _map };

            int i = 0;
            IDataset dataset = null;
            while ((dataset = _map[i++]) != null)
            {
                int count = (from l in _map.MapElements where l.DatasetID == (i - 1) select l).Count();

                var datasetNode = new DatasetTreeNode() { Dataset = dataset, LayerCount = count };
                root.Nodes.Add(datasetNode);

                if (btnRemoveUnusedDatasets.Enabled == false)
                    btnRemoveUnusedDatasets.Enabled = count == 0;

                if(count>0)
                {
                    foreach(var layer in _map.MapElements.Where(l=>l.DatasetID==i-1))
                    {
                        datasetNode.Nodes.Add(new LayerTreeNode(layer));
                    }
                }
            }

            tvDatasets.Nodes.Add(root);
            root.Expand();
        }

        #region Item Classes

        private class MapTreeNode : TreeNode
        {
            public MapTreeNode()
            {
                base.ImageIndex = base.SelectedImageIndex = 0;
            }

            private IMap _map;
            public IMap Map
            {
                get
                {
                    return _map;
                }
                set
                {
                    _map = value;
                    base.Text = this.ToString();
                }
            }

            public override string ToString()
            {
                return Map != null ? Map.Name : "???";
            }
        }

        private class DatasetTreeNode : TreeNode
        {
            public DatasetTreeNode()
            {
                base.ImageIndex = base.SelectedImageIndex = 1;
            }

            private IDataset _dataset;
            public IDataset Dataset
            {
                get { return _dataset; }
                set
                {
                    _dataset = value;
                    base.Text = this.ToString();
                }
            }


            private int _layerCount = 0;
            public int LayerCount
            {
                get
                {
                    return _layerCount;
                }
                set
                {
                    _layerCount = value;
                    base.Text = this.ToString();
                }
            }

            public override string ToString()
            {
                return Dataset != null ? Dataset.DatasetName + " (" + LayerCount + ")" : "???";
            }
        }

        private class LayerTreeNode : TreeNode
        {
            public LayerTreeNode(IDatasetElement layer)
            {
                base.ImageIndex = base.SelectedImageIndex = 1;
                this.Layer = layer;
                base.Text = this.ToString();
            }

            public IDatasetElement Layer { get; set; }

            public override string ToString()
            {
                return Layer?.Title ?? "???";
            }
        }

        #endregion

        private void tvDatasets_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DatasetTreeNode dsTreeNode = tvDatasets.SelectedNode as DatasetTreeNode;
            if (dsTreeNode != null)
            {
                datasetInfoControl1.Dataset = dsTreeNode.Dataset;
                datasetInfoControl1.Visible = true;
                btnRemoveDataset.Enabled = true;
            }
            else
            {
                datasetInfoControl1.Visible = false;
                btnRemoveDataset.Enabled = false;
            }

        }

        private void btnRemoveDataset_Click(object sender, EventArgs e)
        {
            if (datasetInfoControl1.Dataset != null)
            {
                _map.RemoveDataset(datasetInfoControl1.Dataset);
                tvDatasets.SelectedNode.Remove();
                tvDatasets_AfterSelect(tvDatasets, null);
            }
        }

        private void btnRemoveUnusedDatasets_Click(object sender, EventArgs e)
        {
            MapTreeNode root = (MapTreeNode)tvDatasets.Nodes[0];

            List<DatasetTreeNode> remove = new List<DatasetTreeNode>();
            foreach (DatasetTreeNode dsNode in root.Nodes)
            {
                if (dsNode.LayerCount == 0)
                {
                    remove.Add(dsNode);
                }
            }

            foreach (DatasetTreeNode r in remove)
            {
                _map.RemoveDataset(r.Dataset);
                r.Remove();
            }

            //tvDatasets_AfterSelect(tvDatasets, null);
            //btnRemoveUnusedDatasets.Enabled = false;

            InitUI();
        }

        private void btnCompressMap_Click(object sender, EventArgs e)
        {
            _map.Compress();

            InitUI();
        }
    }
}
