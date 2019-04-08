using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Interoperability.OGC.SLD;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.Carto;
using System.IO;
using gView.Framework.Carto.UI;
using gView.Framework.Symbology.UI;

namespace gView.Interoperability.OGC.UI.SLD
{
    public partial class PropertyForm_SLDRenderer : Form, gView.Framework.Carto.Rendering.UI.IPropertyPanel
    {
        private SLDRenderer _renderer;
        private IFeatureLayer _layer;

        public PropertyForm_SLDRenderer()
        {
            InitializeComponent();
        }

        #region IPropertyPanel Member

        public object PropertyPanel(gView.Framework.Carto.IFeatureRenderer renderer, gView.Framework.Data.IFeatureLayer layer)
        {
            _renderer = renderer as SLDRenderer;
            _layer = layer;

            BuildList();

            panel1.Dock = DockStyle.Fill;
            return panel1;
        }

        #endregion

        private void BuildList()
        {
            RendererBox.Items.Clear();
            if (_renderer != null)
            {
                foreach (SLDRenderer.Rule rule in _renderer.Rules)
                {
                    RendererBox.Items.Add(new RuleItem(rule));
                }
            }
        }

        #region Helper Classes
        private class RuleItem
        {
            private SLDRenderer.Rule _rule = null;
            private bool _showLegend = false;

            public RuleItem(SLDRenderer.Rule rule)
            {
                _rule = rule;
            }

            public SLDRenderer.Rule Rule
            {
                get { return _rule; }
                set { _rule = value; }
            }

            public bool ShowLegend
            {
                get { return _showLegend; }
                set { _showLegend = value; }
            }
            public ILegendItem LegendItem
            {
                get
                {
                    return _rule.Symbol as ILegendItem;
                }
            }

            public override string ToString()
            {
                string ret = "SLD Rule";
                if (_rule.Symbol is ILegendItem)
                {
                    string legendLabel = ((ILegendItem)_rule.Symbol).LegendLabel;
                    if (legendLabel.Trim() != String.Empty)
                        ret = legendLabel.Trim();
                }
                return ret;
            }
        }

        private class LegendItem
        {
            private ILegendItem _item;

            public LegendItem(ILegendItem lItem)
            {
                _item = lItem;
            }

            public ILegendItem Item
            {
                get { return _item; }
                set { _item = value; }
            }
        }
        #endregion

        #region Toolbar Events
        private void btnAdd_Click(object sender, EventArgs e)
        {
            geometryType geomType = geometryType.Unknown;
            if (_layer != null && _layer.FeatureClass != null)
            {
                geomType = _layer.FeatureClass.GeometryType;
            }

            if (geomType == geometryType.Unknown)
            {
                FormGeometrySelector geomSel = new FormGeometrySelector();
                if (geomSel.ShowDialog() != DialogResult.OK ||
                    geomSel.GeometryType == geometryType.Unknown) return;

                geomType = geomSel.GeometryType;
            }

            ISymbol symbol = null;
            switch (geomType)
            {
                case geometryType.Point:
                    symbol = new SimplePointSymbol();
                    break;
                case geometryType.Polyline:
                    symbol = new SimpleLineSymbol();
                    break;
                case geometryType.Polygon:
                    symbol = new SimpleFillSymbol();
                    break;
            }

            FormNewSLDRule newRule = new FormNewSLDRule(_layer,
                new SLDRenderer.Rule(new gView.Framework.OGC.WFS.Filter(GmlVersion.v1), symbol));

            if (newRule.ShowDialog() == DialogResult.OK)
            {
                _renderer.Rules.Add(newRule.Rule);
                RendererBox.Items.Add(new RuleItem(newRule.Rule));
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null || _renderer == null) return;

            _renderer.Rules.Remove(_selectedItem.Rule);

            int index2;
            int index = index2 = RendererBox.Items.IndexOf(_selectedItem);
            if (index == -1) return;
            for (int i = index + 1; i < RendererBox.Items.Count; i++)
            {
                if (RendererBox.Items[i] is LegendItem)
                    index2 = i;
                else
                    break;
            }
            for (int i = index2; i >= index; i--)
                RendererBox.Items.RemoveAt(i);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null ||
                _selectedItem.Rule == null ||
                _renderer == null) return;

            SLDRenderer.Rule selectedRenderer = _selectedItem.Rule;
            int index = _renderer.Rules.IndexOf(_selectedItem.Rule);
            if (index >= _renderer.Rules.Count) return;

            _renderer.Rules.Remove(_selectedItem.Rule);
            _renderer.Rules.Insert(Math.Min(_renderer.Rules.Count, index + 1), _selectedItem.Rule);

            BuildList();
            SelectRendererItem(selectedRenderer);
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null ||
                _selectedItem.Rule == null ||
                _renderer == null) return;

            SLDRenderer.Rule selectedRenderer = _selectedItem.Rule;
            int index = _renderer.Rules.IndexOf(_selectedItem.Rule);
            if (index <= 0) return;

            _renderer.Rules.Remove(_selectedItem.Rule);
            _renderer.Rules.Insert(Math.Max(0, index - 1), _selectedItem.Rule);

            BuildList();
            SelectRendererItem(selectedRenderer);
        }

        private void btnProperties_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null ||
                _selectedItem.Rule == null) return;

            SLDRenderer.Rule rule = _selectedItem.Rule;
            FormNewSLDRule dlg = new FormNewSLDRule(_layer, rule);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                RendererBox.Refresh();
            }
        }

        private void btnSaveSLD_Click(object sender, EventArgs e)
        {
            if (_renderer == null || _layer == null || _layer.Class == null)
                return;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false);

                    sw.WriteLine(@"<StyledLayerDescriptor version=""1.0.0"">");
                    sw.WriteLine(_renderer.ToXmlString(_layer.Class.Name));
                    sw.WriteLine(@"</StyledLayerDescriptor>");

                    sw.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        #endregion

        #region RendererBox Events
        private void RendererBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= RendererBox.Items.Count || e.Index < 0) return;

            object item = RendererBox.Items[e.Index];

            if (item is RuleItem)
            {
                SolidBrush b, f;
                if ((e.State & DrawItemState.Selected) != 0)
                {
                    b = (SolidBrush)Brushes.DarkBlue;
                    f = (SolidBrush)Brushes.White;
                }
                else
                {
                    b = (SolidBrush)Brushes.White;
                    f = (SolidBrush)Brushes.Black;
                }
                using (System.Drawing.Font font = new Font("Arial", 10))
                {
                    e.Graphics.FillRectangle(b, e.Bounds);
                    Rectangle rect = new Rectangle(e.Bounds.X + 20, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                    e.Graphics.DrawString(item.ToString(), font, f, rect);

                    rect = new Rectangle(5, e.Bounds.Y + 2, 11, 11);
                    e.Graphics.DrawImage(
                        (((RuleItem)item).ShowLegend ?
                        global::gView.Win.Interoperability.OGC.UI.Properties.Resources.CollapseIcon :
                        global::gView.Win.Interoperability.OGC.UI.Properties.Resources.ExpandIcon),
                        rect);
                }
            }
            else if (item is LegendItem)
            {
                Rectangle rect = new Rectangle(20, e.Bounds.Top, 30, 20);

                ILegendItem legendItem = ((LegendItem)item).Item;
                if (legendItem != null)
                {
                    if (legendItem is ISymbol)
                    {
                        if (legendItem is ITextSymbol)
                        {
                            ITextSymbol ts = ((ISymbol)legendItem).Clone() as ITextSymbol;
                            if (ts != null)
                            {
                                ts.Text = "Abc";
                                ts.TextSymbolAlignment = TextSymbolAlignment.Center;
                                ts.Angle = 0;
                                ts.HorizontalOffset = ts.VerticalOffset = 0;

                                SymbolPreview.Draw(e.Graphics, rect, ts);
                                ts.Release();
                            }
                        }
                        else
                        {
                            SymbolPreview.Draw(e.Graphics, rect, (ISymbol)legendItem);
                        }
                    }
                    if (legendItem.LegendLabel != String.Empty)
                    {
                        using (Font font = new Font("Arial", 9))
                        {
                            e.Graphics.DrawString(legendItem.LegendLabel, font, Brushes.Black, 52, e.Bounds.Top + e.Bounds.Height / 2 - font.Height / 2);
                            SizeF stringSize = e.Graphics.MeasureString(legendItem.LegendLabel, font);
                            RendererBox.HorizontalExtent = (int)Math.Max(RendererBox.HorizontalExtent, 52 + stringSize.Width);
                        }
                    }
                }
            }
        }

        private void RendererBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemWidth = RendererBox.Width;
            e.ItemHeight = 20;
        }

        private RuleItem _selectedItem = null;
        private void RendererBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedItem = RendererBox.SelectedItem as RuleItem;

            btnRemove.Enabled = btnUp.Enabled = btnDown.Enabled = btnProperties.Enabled =
                _selectedItem != null;
        }

        private int _mX = 0, _mY = 0;
        private void RendererBox_MouseMove(object sender, MouseEventArgs e)
        {
            _mX = e.X;
            _mY = e.Y;
        }

        private void RendererBox_MouseDown(object sender, MouseEventArgs e)
        {
            object item = null;
            for (int i = 0; i < RendererBox.Items.Count; i++)
            {
                Rectangle rect = RendererBox.GetItemRectangle(i);

                if (rect.Y <= e.Y && (rect.Y + rect.Height) >= e.Y)
                {
                    item = RendererBox.Items[i];
                    break;
                }
            }
            if (item == null) return;

            if (item is RuleItem)
            {
                if (e.X > 5 && e.X < 17)
                {
                    ((RuleItem)item).ShowLegend = !((RuleItem)item).ShowLegend;
                    ShowHideLegend(item as RuleItem);
                }
            }
        }

        private void RendererBox_DoubleClick(object sender, EventArgs e)
        {
            object item = null;
            for (int i = 0; i < RendererBox.Items.Count; i++)
            {
                Rectangle rect = RendererBox.GetItemRectangle(i);
                if (rect.Y <= _mY && (rect.Y + rect.Height) >= _mY)
                {
                    item = RendererBox.Items[i];
                    break;
                }
            }
            if (item == null) return;

            if (item is LegendItem)
            {
                if (_mX > 20 && _mX < 50)
                {
                    ISymbol symbol = ((LegendItem)item).Item as ISymbol;
                    if (symbol != null)
                    {
                        FormSymbol dlg = new FormSymbol(symbol);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            int index=RendererBox.Items.IndexOf(item);
                            RuleItem ritem = RendererBox.Items[index - 1] as RuleItem;
                            if (ritem != null)
                            {
                                ritem.Rule.Symbol = dlg.Symbol;
                                ((LegendItem)item).Item = dlg.Symbol as ILegendItem;
                            }
                            RendererBox.Refresh();
                        }
                    }
                }
            }
        }
        #endregion

        #region Helper
        private void ShowHideLegend(RuleItem item)
        {
            if (_renderer == null) return;

            int index = RendererBox.Items.IndexOf(item);
            if (index == -1) return;

            if (!item.ShowLegend)
            {
                for (int i = index + 1; i < RendererBox.Items.Count; i++)
                {
                    if (RendererBox.Items[i] is RuleItem) break;
                    RendererBox.Items.RemoveAt(i);
                    i--;
                }
            }
            else if (item.LegendItem != null)
            {
                RendererBox.Items.Insert(index + 1, new LegendItem(item.LegendItem));
            }
        }

        private void SelectRendererItem(SLDRenderer.Rule rule)
        {
            foreach (object item in RendererBox.Items)
            {
                if (item is RuleItem && ((RuleItem)item).Rule == rule)
                {
                    RendererBox.SelectedItem = item;
                    break;
                }
            }
        }
        #endregion
 
    }
}