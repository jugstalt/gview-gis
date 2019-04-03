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
    public partial class FormGridWizard : Form
    {
        private List<GridColorClass> _classes = new List<GridColorClass>();

        public FormGridWizard(double min,double max)
        {
            InitializeComponent();

            numMinValue.Value = (decimal)min;
            numAverageValue.Value = (decimal)(max + min) / 2;
            numMaxValue.Value = (decimal)max;

            numLevelEvery.Value = (decimal)(max - min) / 100;
            numLabelEvery.Value = (decimal)(max - min) / 10;

            btnColMin.BackColor = Color.White;
            btnColAverage.BackColor = Color.Yellow;
            btnColMax.BackColor = Color.Red;
        }

        public GridColorClass[] ColorClasses
        {
            get
            {
                if (_classes.Count == 0) return null;
                return _classes.ToArray();
            }
        }

        #region Helper
        private Color GradientColor(double h)
        {
            Color c1, c2;
            if (chkUseAverage.Checked)
            {
                if (h < (double)numAverageValue.Value)
                {
                    c1 = btnColMin.BackColor;
                    c2 = btnColAverage.BackColor;

                    h = (double)(((decimal)h - numMinValue.Value) / (numAverageValue.Value - numMinValue.Value));
                }
                else
                {
                    c1 = btnColAverage.BackColor;
                    c2 = btnColMax.BackColor;

                    h = (double)(((decimal)h - numAverageValue.Value) / (numMaxValue.Value - numAverageValue.Value));
                }
            }
            else
            {
                c1 = btnColMin.BackColor;
                c2 = btnColMax.BackColor;

                h = (double)(((decimal)h - numMinValue.Value) / (numMaxValue.Value - numMinValue.Value));
            }
            h = Math.Max(Math.Min(1.0, h), 0.0);

            double r = (c2.R - c1.R) * h;
            double g = (c2.G - c1.G) * h;
            double b = (c2.B - c1.B) * h;

            return Color.FromArgb(
                c1.R + (int)r,
                c1.G + (int)g,
                c1.B + (int)b);
        }
        #endregion

        private void btnOK_Click(object sender, EventArgs e)
        {
            for (double h = (double)numMinValue.Value;
                        h < (double)(numMaxValue.Value + numLevelEvery.Value);
                        h += (double)numLevelEvery.Value)
            {
                _classes.Add(new GridColorClass(
                    h, h + (double)numLevelEvery.Value,
                    GradientColor(h)));
            }

            int legStep = (int)((numMaxValue.Value - numMinValue.Value) / Math.Max(numLabelEvery.Value,numLevelEvery.Value));
            for (int i = 0; i < _classes.Count; i += legStep)
            {
                if (i == 0)
                {
                    _classes[i].Legend =
                        Math.Round(_classes[i].MinValue, 2).ToString();
                }
                else
                {
                    _classes[i].Legend =
                        Math.Round(_classes[i].MinValue * 0.5 + _classes[i].MaxValue * 0.5, 2).ToString();
                }
            }
            if (_classes.Count > 0)
            {
                _classes[_classes.Count - 1].Legend =
                        Math.Round(_classes[_classes.Count - 1].MaxValue, 2).ToString();
            }
        }

        private void btnCol_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;

            ColorDialog dlg = new ColorDialog();
            dlg.Color = ((Button)sender).BackColor;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ((Button)sender).BackColor = dlg.Color;
            }
        }
    }
}