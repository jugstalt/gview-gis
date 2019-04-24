using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.Symbology.UI;

namespace gView.Framework.Carto.Rendering.UI
{
    public partial class PropertyForm_ChartLabelRenderer : Form, IPropertyPanel2
    {
        private ChartLabelRenderer _renderer = null;
        private IFeatureClass _fc = null;

        public PropertyForm_ChartLabelRenderer()
        {
            //InitializeComponent();
        }

        private void MakeGUI()
        {
            cmbChartType.SelectedIndex = (int)_renderer.ChartType;

            foreach (IField field in _fc.Fields.ToEnumerable())
            {
                if (field.type == FieldType.biginteger ||
                    field.type == FieldType.Double ||
                    field.type == FieldType.Float ||
                    field.type == FieldType.integer ||
                    field.type == FieldType.smallinteger)
                {
                    lstNumberFields.Items.Add(field.name);
                }
            }

            foreach (string fieldName in _renderer.FieldNames)
            {
                ISymbol symbol = _renderer.GetSymbol(fieldName);
                if (symbol is ILegendItem)
                {
                    symbolsListView1.addSymbol(symbol, new string[] { fieldName, ((ILegendItem)symbol).LegendLabel });
                }
                else
                {
                    symbolsListView1.addSymbol(symbol, new string[] { fieldName, String.Empty });
                }
            }

            numSize.Value = (decimal)_renderer.Size;
            numMaxValue.Value = (decimal)_renderer.ValueEquatesToSize;

            btnConstantSize.Checked = _renderer.SizeType == ChartLabelRenderer.sizeType.ConstantSize;
            btnValueOfEquatesToSize.Checked = _renderer.SizeType == ChartLabelRenderer.sizeType.ValueOfEquatesToSize;

            panelMaxValue.Enabled = btnValueOfEquatesToSize.Checked;
            cmbLabelPriority.SelectedIndex = (int)_renderer.LabelPriority;
        }

        #region IPropertyPanel2 Member

        public object PropertyPanel(ILabelRenderer renderer, IFeatureLayer layer)
        {
            _renderer = renderer as ChartLabelRenderer;
            _fc = layer.Class as IFeatureClass;
            if (_renderer == null || _fc == null)
                return null;

            InitializeComponent();

            MakeGUI();

            return panel1;
        }

        #endregion

        #region Events

        private void lstNumberFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnApply.Enabled = lstNumberFields.SelectedIndices != null && lstNumberFields.SelectedIndices.Count > 0;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lstNumberFields.SelectedItems.Count; i++)
            {
                string fieldName = (string)lstNumberFields.SelectedItems[i];

                if (symbolsListView1.Items != null)
                {
                    bool has = false;
                    foreach (ListViewItem item in symbolsListView1.Items)
                    {
                        if (item.Text == fieldName)
                        {
                            has = true;
                            break;
                        }
                    }
                    if (has) continue;
                }

                SimpleFillSymbol symbol = (SimpleFillSymbol)RendererFunctions.CreateStandardSymbol(geometryType.Polygon);
                symbol.OutlineSymbol = null;
                symbol.SymbolSmothingMode = SymbolSmoothing.AntiAlias;
                if (symbol is ILegendItem)
                {
                    ((ILegendItem)symbol).LegendLabel = fieldName;
                    symbolsListView1.addSymbol(symbol, new string[] { fieldName, ((ILegendItem)symbol).LegendLabel });
                }
                else
                {
                    symbolsListView1.addSymbol(symbol, new string[] { fieldName, String.Empty });
                }
                _renderer.SetSymbol(fieldName, symbol);
            }
        }

        private void symbolsListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemove.Enabled = symbolsListView1.SelectedItems != null && symbolsListView1.SelectedItems.Count > 0;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in symbolsListView1.SelectedItems)
            {
                _renderer.SetSymbol(item.Text, null);
            }
            symbolsListView1.RemoveSelected();
        }

        private void cmbChartType_SelectedIndexChanged(object sender, EventArgs e)
        {
            _renderer.ChartType = (ChartLabelRenderer.chartType)cmbChartType.SelectedIndex;
        }

        private void btnOutlineSymbol_Click(object sender, EventArgs e)
        {
            if (_renderer.OutlineSymbol == null)
            {
                _renderer.OutlineSymbol = (ILineSymbol)RendererFunctions.CreateStandardSymbol(geometryType.Polyline);
            }
            FormSymbol dlg = new FormSymbol(_renderer.OutlineSymbol);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _renderer.OutlineSymbol = dlg.Symbol as ILineSymbol;
            }
        }

        private void numMaxValue_ValueChanged(object sender, EventArgs e)
        {
            _renderer.ValueEquatesToSize = (double)numMaxValue.Value;
        }

        private void numSize_ValueChanged(object sender, EventArgs e)
        {
            _renderer.Size = (double)numSize.Value;
        }

        private void btnGetMaxValue_Click(object sender, EventArgs e)
        {
            if (symbolsListView1.Items.Count > 0)
            {
                string[] fieldNames = new string[symbolsListView1.Items.Count];
                int i=0;
                foreach (ListViewItem item in symbolsListView1.Items)
                {
                    fieldNames[i++] = item.Text;
                }
                PropertyForm_ChartLabelRenderer_MaxValues dlg = new PropertyForm_ChartLabelRenderer_MaxValues(fieldNames, _fc);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    numMaxValue.Value = (decimal)dlg.MaxValue;
                }
            }
        }

        private void btnValueOfEquatesToSize_CheckedChanged(object sender, EventArgs e)
        {
            panelMaxValue.Enabled = btnValueOfEquatesToSize.Checked;

            if (btnValueOfEquatesToSize.Checked)
                _renderer.SizeType = ChartLabelRenderer.sizeType.ValueOfEquatesToSize;
        }

        private void btnConstantSize_CheckedChanged(object sender, EventArgs e)
        {
            if (btnConstantSize.Checked)
                _renderer.SizeType = ChartLabelRenderer.sizeType.ConstantSize;
        }

        private void symbolsListView1_OnSymbolChanged(string key, ISymbol symbol)
        {
            _renderer.SetSymbol(key, symbol);
        }

        private void symbolsListView1_OnLabelChanged(ISymbol symbol, int nr, string label)
        {
            if (symbol is ILegendItem)
                ((ILegendItem)symbol).LegendLabel = label;
        }

        private void cmbLabelPriority_SelectedIndexChanged(object sender, EventArgs e)
        {
            _renderer.LabelPriority = (SimpleLabelRenderer.labelPriority)cmbLabelPriority.SelectedIndex;
        }

        #endregion
    }
}
