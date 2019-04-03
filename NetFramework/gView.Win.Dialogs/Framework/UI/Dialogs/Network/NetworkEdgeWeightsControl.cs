using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Network;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.system;
using System.Xml;
using gView.Framework.Network.Algorthm;

namespace gView.Framework.UI.Dialogs.Network
{
    public partial class NetworkEdgeWeightsControl : UserControl
    {
        private int _counter = 0;
        private IFeatureDatabase3 _database = null;
        private SelectFeatureclassesControl _selected;
        private List<ISimpleNumberCalculation> _calculators = new List<ISimpleNumberCalculation>();

        public NetworkEdgeWeightsControl(IFeatureDataset dataset, SelectFeatureclassesControl selected)
        {
            InitializeComponent();

            if (dataset != null)
                _database = dataset.Database as IFeatureDatabase3;

            if (_database == null)
                throw new ArgumentException();

            _selected = selected;

            PlugInManager pluginMan = new PlugInManager();
            foreach (var calcType in pluginMan.GetPlugins(Plugins.Type.ISimpleNumberCalculation))
            {
                ISimpleNumberCalculation calc = pluginMan.CreateInstance<ISimpleNumberCalculation>(calcType);
                if (calc == null)
                    continue;

                _calculators.Add(calc);
            }
        }

        #region Events
        private void btnAddWeight_Click(object sender, EventArgs e)
        {
            GraphWeight weight = new GraphWeight("Weight " + (++_counter).ToString(), GraphWeightDataType.Double);
            lstWeights.Items.Add(new WeightListViewItem(weight));
        }

        private void btnRemoveWeight_Click(object sender, EventArgs e)
        {
            if (lstWeights.SelectedIndices.Count == 1)
            {
                lstWeights.Items.Remove(lstWeights.SelectedItems[0]);
                propsWeight.SelectedObject = null;
            }
        }

        private void lstWeights_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemoveWeight.Enabled = lstWeights.SelectedIndices.Count == 1;
            if (lstWeights.SelectedIndices.Count == 1)
            {
                IGraphWeight weight = ((WeightListViewItem)lstWeights.SelectedItems[0]).GraphWeight;
                propsWeight.SelectedObject = weight;

                gridFcs.Rows.Clear();

                foreach (IFeatureClass fc in _selected.EdgeFeatureclasses)
                {
                    int fcId = _database.GetFeatureClassID(fc.Name);

                    DataGridViewRow row = new DataGridViewRow();
                    DataGridViewTextBoxCell idCell = new DataGridViewTextBoxCell();
                    idCell.Value = fcId;
                    row.Cells.Add(idCell);

                    DataGridViewTextBoxCell nameCell = new DataGridViewTextBoxCell();
                    nameCell.Value = fc.Name;
                    row.Cells.Add(nameCell);

                    DataGridViewComboBoxCell fieldCell = new DataGridViewComboBoxCell();
                    fieldCell.Items.Add("<none>");
                    foreach (IField field in fc.Fields.ToEnumerable())
                    {
                        switch (field.type)
                        {
                            case FieldType.integer:
                            case FieldType.smallinteger:
                            case FieldType.biginteger:
                            case FieldType.Float:
                            case FieldType.Double:
                                fieldCell.Items.Add(field.name);
                                break;
                        }
                    }
                    IGraphWeightFeatureClass gwfc = weight.FeatureClasses[fcId];
                    if (gwfc == null)
                        fieldCell.Value = "<none>";
                    else
                        fieldCell.Value = gwfc.FieldName;
                    row.Cells.Add(fieldCell);

                    DataGridViewComboBoxCell calcCell = new DataGridViewComboBoxCell();
                    calcCell.Items.Add("<none>");
                    foreach (ISimpleNumberCalculation calc in _calculators)
                        calcCell.Items.Add(calc.Name);
                    if (gwfc == null || gwfc.SimpleNumberCalculation == null)
                        calcCell.Value = "<none>";
                    else
                        calcCell.Value = gwfc.SimpleNumberCalculation.Name;

                    row.Cells.Add(calcCell);

                    DataGridViewButtonCell calcPropCell = new DataGridViewButtonCell();
                    calcPropCell.Value = "...";
                    row.Cells.Add(calcPropCell);

                    gridFcs.Rows.Add(row);
                }
            }
        }

        private void gridFcs_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || lstWeights.SelectedItems.Count != 1)
                return;

            IGraphWeight weight = ((WeightListViewItem)lstWeights.SelectedItems[0]).GraphWeight;
            int fcId = Convert.ToInt32(gridFcs.Rows[e.RowIndex].Cells[0].Value);

            string fieldName = (string)((DataGridViewComboBoxCell)gridFcs.Rows[e.RowIndex].Cells[2]).Value;
            IGraphWeightFeatureClass gwfc = weight.FeatureClasses[fcId];
            if (gwfc == null)
            {
                gwfc = new GraphWeightFeatureClass(fcId, fieldName);
                weight.FeatureClasses[fcId] = gwfc;
            }
            if (e.ColumnIndex == 2)
            {
                if (fieldName == null || fieldName == "<none>")
                {
                    ((GraphWeightFeatureClass)gwfc).FieldName = String.Empty;
                }
                else
                {
                    ((GraphWeightFeatureClass)gwfc).FieldName = fieldName;
                }
            }
            if (e.ColumnIndex == 3)
            {
                if (gwfc.SimpleNumberCalculation != null &&
                    gwfc.SimpleNumberCalculation.Name == gridFcs.Rows[e.RowIndex].Cells[3].Value.ToString())
                {
                }
                else
                {
                    if (gridFcs.Rows[e.RowIndex].Cells[3].Value.ToString() == "<none>")
                        ((GraphWeightFeatureClass)gwfc).SimpleNumberCalculation = null;
                    foreach (ISimpleNumberCalculation calc in _calculators)
                    {
                        if (calc.Name == gridFcs.Rows[e.RowIndex].Cells[3].Value.ToString())
                        {
                            PlugInManager pMan = new PlugInManager();
                            ((GraphWeightFeatureClass)gwfc).SimpleNumberCalculation = pMan.CreateInstance(PlugInManager.PlugInID(calc)) as ISimpleNumberCalculation;
                        }
                    }
                }
            }
        }

        private void gridFcs_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || lstWeights.SelectedItems.Count != 1)
                return;

            IGraphWeight weight = ((WeightListViewItem)lstWeights.SelectedItems[0]).GraphWeight;
            int fcId = Convert.ToInt32(gridFcs.Rows[e.RowIndex].Cells[0].Value);
            IGraphWeightFeatureClass gwfc = weight.FeatureClasses[fcId];

            if (gwfc != null && gwfc.SimpleNumberCalculation != null && e.ColumnIndex == 4)
            {
                FormPropertyGrid dlg = new FormPropertyGrid(gwfc.SimpleNumberCalculation);
                dlg.ShowDialog();
            }
        }

        #endregion

        #region ItemClasses
        private class WeightListViewItem : ListViewItem
        {
            private IGraphWeight _weight;

            public WeightListViewItem(IGraphWeight weight)
            {
                _weight = weight;
                base.Text = _weight.Name;
            }

            public IGraphWeight GraphWeight
            {
                get { return _weight; }
            }
        }
        #endregion

        #region Properties
        public GraphWeights GraphWeights
        {
            get
            {
                GraphWeights weights = new GraphWeights();

                foreach (WeightListViewItem weightItem in lstWeights.Items)
                {
                    IGraphWeight weight = weightItem.GraphWeight;

                    foreach (IGraphWeightFeatureClass gwfc in weight.FeatureClasses)
                    {
                        if (gwfc.FieldName == null || String.IsNullOrEmpty(gwfc.FieldName) ||
                            gwfc.FieldName == "<none>")
                        {
                            weight.FeatureClasses.Remove(gwfc.FcId);
                        }
                    }

                    if (weight.FeatureClasses.Count > 0)
                        weights.Add(weight);
                }

                return weights.Count > 0 ? weights : null;
            }
        }

        public Seriazlized Serialize
        {
            get { return null; }  // ToDO
            set { }
        }

        #endregion

        #region Serialization Class

        public class Seriazlized
        {
            // ToDO:
        }

        #endregion
    }
}
