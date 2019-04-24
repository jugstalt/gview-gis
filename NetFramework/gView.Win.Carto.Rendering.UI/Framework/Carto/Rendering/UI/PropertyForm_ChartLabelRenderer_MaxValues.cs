using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;

namespace gView.Framework.Carto.Rendering.UI
{
    public partial class PropertyForm_ChartLabelRenderer_MaxValues : Form
    {
        IFeatureClass _fc;
        string[] _fieldNames;
        public PropertyForm_ChartLabelRenderer_MaxValues(string[] fieldNames, IFeatureClass fc)
        {
            InitializeComponent();

            _fc = fc;
            _fieldNames = fieldNames;
        }

        private void PropertyForm_ChartLabelRenderer_MaxValues_Load(object sender, EventArgs e)
        {
            if (_fc != null && _fieldNames!=null)
            {
                double sum = 0D;
                foreach (string fieldName in _fieldNames)
                {
                    if (_fc.FindField(fieldName) == null)
                        continue;

                    object minObj = FunctionFilter.QueryScalar(
                        _fc,
                         new FunctionFilter("MIN", fieldName, "fieldMin"),
                        "fieldMin");
                    object maxObj = FunctionFilter.QueryScalar(
                        _fc,
                        new FunctionFilter("MAX", fieldName, "fieldMax"),
                        "fieldMax");

                    double max;
                    if (minObj != null && maxObj != null)
                    {
                        max = Math.Max(Math.Abs(Convert.ToDouble(minObj)), Math.Abs(Convert.ToDouble(maxObj)));
                        lstMaxValues.Items.Add(
                            new ListViewItem(new string[] { fieldName, max.ToString() }));
                        sum += max;
                    }

                }
                lstMaxValues.Items.Add(
                            new ListViewItem(new string[] { "*Sum", sum.ToString() }));
            }

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (lstMaxValues.SelectedItems.Count == 1)
            {
                ListViewItem item = lstMaxValues.SelectedItems[0];
                MaxValue = Convert.ToDouble(item.SubItems[1].Text);
            }
        }

        public double MaxValue
        {
            get;
            set;
        }

        private void lstMaxValues_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = lstMaxValues.SelectedItems != null &&
                lstMaxValues.SelectedItems.Count == 1;
        }
    }
}
