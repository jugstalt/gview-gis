using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using gView.Framework.Carto;
using gView.Framework.Data;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormDatasetProperties : Form
    {
        private List<DatasetItemElement> _elements = new List<DatasetItemElement>();
        private IMap _map;

        private FormDatasetProperties(IMap map)
        {
            _map = map;

            InitializeComponent();
        }

        async static public Task<FormDatasetProperties> CreateAsync(IMap map, IDataset dataset)
        {
            var dlg = new FormDatasetProperties(map);

            await dlg.AddDataset(dataset);

            return dlg;
        }
        async static public Task<FormDatasetProperties> CreateAsync(IMap map, List<IDataset> datasets)
        {
            var dlg = new FormDatasetProperties(map);

            foreach (IDataset dataset in datasets)
            {
                await dlg.AddDataset(dataset);
            }

            return dlg;
        }

        async private Task AddDataset(IDataset dataset)
        {
            if (dataset == null || await dataset.Elements() == null) return;

            if (dataset.State != DatasetState.opened)
            {
                if (!await dataset.Open())
                {
                    MessageBox.Show("Can't open dataset '" + dataset.DatasetName + "'.\n" + dataset.LastErrorMessage);
                    return;
                }
            }

            foreach (IDatasetElement element in await dataset.Elements())
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
                FormLayerProperties dlg = new FormLayerProperties(null, _map, _elements[e.RowIndex].dataset, _elements[e.RowIndex].layer);
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

        async private void btnOK_Click(object sender, EventArgs e)
        {
            if (_elements == null) return;

            for (int i = 0; i < dgLayers.Rows.Count; i++)
            {
                if (!(bool)dgLayers[0, i].Value)
                {
                    (await _elements[i].dataset.Elements()).Remove(_elements[i].layer);
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