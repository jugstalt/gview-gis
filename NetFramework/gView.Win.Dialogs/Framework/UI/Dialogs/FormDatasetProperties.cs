using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormDatasetProperties : Form
    {
        List<DatasetItemElement> _elements = new List<DatasetItemElement>();

        public FormDatasetProperties(IDataset dataset)
        {
            if (dataset == null) this.Close();

            InitializeComponent();

            AddDataset(dataset);

            if (_elements.Count == 0) this.Close();
        }
        public FormDatasetProperties(List<IDataset> datasets)
        {
            if (datasets == null) this.Close();

            InitializeComponent();

            foreach (IDataset dataset in datasets)
            {
                AddDataset(dataset);
            }

            if (_elements.Count == 0) this.Close();
        }

        private void AddDataset(IDataset dataset)
        {
            if (dataset == null || dataset.Elements().Result == null) return;

            if (dataset.State != DatasetState.opened)
            {
                if (!dataset.Open())
                {
                    MessageBox.Show("Can't open dataset '" + dataset.DatasetName + "'.\n" + dataset.LastErrorMessage);
                    return;
                }
            }

            foreach (IDatasetElement element in dataset.Elements().Result)
            {
                if (element == null) continue;
                ILayer layer = LayerFactory.Create(element.Class);

                dgLayers.Rows.Add(new object[] { true, element.Title, true });
                _elements.Add(new DatasetItemElement(dataset, layer));
            }
        }

        private void dgLayers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_elements == null) return;
            if (_elements.Count <= e.RowIndex || e.RowIndex < 0) return;

            if (e.ColumnIndex == 3)
            {
                FormLayerProperties dlg = new FormLayerProperties(_elements[e.RowIndex].dataset, _elements[e.RowIndex].layer);
                dlg.ShowDialog();
            }
        }

        private void dgLayers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_elements == null) return;
            if (_elements.Count <= e.RowIndex || e.RowIndex < 0) return;

            if (e.ColumnIndex == 2)
            {
                ILayer layer = _elements[e.RowIndex].layer;
                layer.Visible = (bool)dgLayers.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_elements == null) return;

            for (int i = 0; i < dgLayers.Rows.Count; i++)
            {
                if (!(bool)dgLayers[0, i].Value)
                {
                    _elements[i].dataset.Elements().Result.Remove(_elements[i].layer);
                }
            }
        }

        public List<ILayer> Layers
        {
            get
            {
                List<ILayer> _layers = new List<ILayer>();

                for (int i = 0; i < dgLayers.Rows.Count; i++)
                {
                    if ((bool)dgLayers[0, i].Value)
                    {
                        _layers.Add(_elements[i].layer);
                    }
                }
                return _layers;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }
    }

    struct DatasetItemElement
    {
        public IDataset dataset;
        public ILayer layer;

        public DatasetItemElement(IDataset ds, ILayer l)
        {
            dataset = ds;
            layer = l;
        }
    }
}