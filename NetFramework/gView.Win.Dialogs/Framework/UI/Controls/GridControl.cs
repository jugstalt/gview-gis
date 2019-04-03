using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Symbology;
using gView.Framework.Data;
using gView.Framework.UI.Dialogs;

namespace gView.Framework.UI.Controls
{
    public partial class GridControl : UserControl
    {
        private List<GridColorClass> _classes = new List<GridColorClass>();
        private double[] _hillShadeVector = new double[3];
        private double _min = 0.0, _max = 10000.0;

        public GridControl()
        {
            InitializeComponent();

            symbolsListView1.OnSymbolClicked += new SymbolsListView.SymbolClicked(symbolsListView1_OnSymbolClicked);
        }

        private void btnAddColorClass_Click(object sender, EventArgs e)
        {
            FormGridColorClass dlg = new FormGridColorClass();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                GridColorClass cc = new GridColorClass(
                    dlg.MinValue, dlg.MaxValue, Color.White);
                cc.Legend = dlg.Label;

                _classes.Add(cc);

                SimpleFillSymbol symbol = new SimpleFillSymbol();
                symbol.OutlineColor = Color.Transparent;
                symbol.Color = cc.Color;

                symbolsListView1.addSymbol(
                    symbol,
                    new string[] { dlg.MinValue.ToString() + " - " + dlg.MaxValue.ToString(), dlg.Label },
                    cc
                    );

                panelGrid.Refresh();
            }
        }

        private void btnEditColorClass_Click(object sender, EventArgs e)
        {
            GridColorClass cc = symbolsListView1.UserObject as GridColorClass;
            if (cc == null) return;

            FormGridColorClass dlg = new FormGridColorClass();
            dlg.ColorClass = cc;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                cc.MinValue = dlg.MinValue;
                cc.MaxValue = dlg.MaxValue;
                cc.Legend = dlg.Label;

                symbolsListView1.ValueText =
                    cc.MinValue.ToString() + " - " + cc.MaxValue.ToString();
                symbolsListView1.LegendText =
                    cc.Legend;

                panelGrid.Refresh();
            }
        }

        private void btnRemoveColorClass_Click(object sender, EventArgs e)
        {
            GridColorClass cc = symbolsListView1.UserObject as GridColorClass;
            if (cc == null) return;

            _classes.Remove(cc);
            symbolsListView1.RemoveSelected();

            panelGrid.Refresh();
        }

        public GridColorClass[] GridColorClasses
        {
            get
            {
                if (_classes.Count == 0) return null;
                return _classes.ToArray();
            }
            set
            {
                symbolsListView1.Clear();
                _classes.Clear();
                if (value == null) return;

                foreach (GridColorClass cc in value)
                {
                    _classes.Add(cc);

                    SimpleFillSymbol symbol = new SimpleFillSymbol();
                    symbol.OutlineColor = Color.Transparent;
                    symbol.Color = cc.Color;

                    symbolsListView1.addSymbol(
                        symbol,
                        new string[] { cc.MinValue.ToString() + " - " + cc.MaxValue.ToString(), cc.Legend },
                        cc
                        );
                }
            }
        }
        public bool UseHillShade
        {
            get { return chkUseHillShade.Checked; }
            set { chkUseHillShade.Checked = value; }
        }
        public bool EnableHillShade
        {
            get { return chkUseHillShade.Enabled; }
            set
            {
                chkUseHillShade.Enabled = btnHillShade.Enabled = value;
            }
        }
        public double[] HillShadeVector
        {
            get
            {
                return _hillShadeVector;
            }
            set
            {
                if (value != null && value.Length == 3)
                    value.CopyTo(_hillShadeVector, 0);
            }
        }
        public double MinValue
        {
            get { return _min; }
            set { _min = value; }
        }
        public double MaxValue
        {
            get { return _max; }
            set { _max = value; }
        }

        void symbolsListView1_OnSymbolClicked(ISymbol symbol)
        {
            if (!(symbol is SimpleFillSymbol)) return;
            GridColorClass cc = symbolsListView1.UserObject as GridColorClass;
            if (cc == null) return;

            ColorDialog dlg = new ColorDialog();
            dlg.Color = cc.Color;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                cc.Color = dlg.Color;
                ((SimpleFillSymbol)symbol).Color = cc.Color;
            }

            panelGrid.Refresh();
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            symbolsListView1.Clear();
            _classes.Clear();

            panelGrid.Refresh();
        }

        private void btnHillShade_Click(object sender, EventArgs e)
        {
            FormHillShade dlg = new FormHillShade(
                _hillShadeVector[0],
                _hillShadeVector[1],
                _hillShadeVector[2]);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _hillShadeVector[0] = dlg.Dx;
                _hillShadeVector[1] = dlg.Dy;
                _hillShadeVector[2] = dlg.Dz;
            }
        }

        private void btnWizard_Click(object sender, EventArgs e)
        {
            FormGridWizard dlg = new FormGridWizard(_min, _max);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.GridColorClasses = dlg.ColorClasses;
                panelGrid.Refresh();
            }
        }

        private void panelGrid_Paint(object sender, PaintEventArgs e)
        {
            int height = panelGrid.Height - 20;
            if (height <= 0 || _max - _min <= 0) return;

            GridColorClass[] classes = this.GridColorClasses;
            for (int y = 0; y < height; y++)
            {
                double h = _max - (double)y * (_max - _min) / (double)height;
                using (Pen pen = new Pen(GridColorClass.FindColor(h, classes), 1))
                {
                    e.Graphics.DrawLine(pen, 1, y + 10, 20, y + 10);
                }
            }

            using (Font font = new Font("Arial", 8))
            {
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    foreach (GridColorClass cc in _classes)
                    {
                        if (String.IsNullOrEmpty(cc.Legend)) continue;

                        double h = cc.MaxValue * 0.5 + cc.MinValue * 0.5;
                        int y = (int)((_max - h) * (double)height / (_max - _min));

                        e.Graphics.DrawString(cc.Legend, font, brush, 22, y + 5);
                    }
                }
            }
        }

        
    }
}
