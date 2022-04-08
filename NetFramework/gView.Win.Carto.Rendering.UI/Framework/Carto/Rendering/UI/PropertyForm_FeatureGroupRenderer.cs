using gView.Framework.Data;
using gView.Framework.Symbology;
using gView.Framework.Symbology.UI;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Win.Carto.Rendering.UI.Framework.Carto.Rendering.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace gView.Framework.Carto.Rendering.UI
{
    public partial class PropertyForm_FeatureGroupRenderer : Form, IPropertyPanel
    {
        private IGroupRenderer _renderer = null;
        private IFeatureLayer _layer = null;

        public PropertyForm_FeatureGroupRenderer()
        {
            InitializeComponent();
        }

        #region IPropertyPanel Member

        public object PropertyPanel(IFeatureRenderer renderer, IFeatureLayer layer)
        {
            _renderer = renderer as IGroupRenderer;
            _layer = layer;

            if (_renderer == null || _layer == null)
            {
                return null;
            }

            if (_renderer is ScaleDependentRenderer)
            {
                panelScale.Visible = true;
            }

            BuildList();

            panel1.Dock = DockStyle.Fill;
            return panel1;
        }

        #endregion

        private void BuildList()
        {
            List<RendererItem> items = new List<RendererItem>();
            foreach (object item in RendererBox.Items)
            {
                if (item is RendererItem)
                {
                    items.Add((RendererItem)item);
                }
            }

            RendererBox.Items.Clear();

            if (_renderer == null)
            {
                return;
            }

            foreach (IFeatureRenderer renderer in ListOperations<IFeatureRenderer>.Swap(this.RendererList()))
            {
                if (renderer == null)
                {
                    continue;
                }

                bool found = false;
                foreach (RendererItem item in items)
                {
                    if (item.FeatureRenderer == renderer)
                    {
                        found = true;
                        RendererBox.Items.Add(item);
                        ShowHideLegend(item);
                        break;
                    }
                }
                if (!found)
                {
                    RendererBox.Items.Add(new RendererItem(renderer));
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (_renderer == null)
            {
                return;
            }

            FormFeatureRenderers dlg = new FormFeatureRenderers(_layer);

            if (dlg.ShowDialog() == DialogResult.OK && dlg.SelectedFeatureRenderer != null)
            {
                IFeatureRenderer renderer = dlg.SelectedFeatureRenderer;
                if (renderer is SimpleRenderer && _layer != null && _layer.FeatureClass != null)
                {
                    ((SimpleRenderer)renderer).Symbol =
                        ((SimpleRenderer)renderer).CreateStandardSymbol(_layer.LayerGeometryType/*_layer.FeatureClass.GeometryType*/);
                }
                _renderer.Renderers.Add(dlg.SelectedFeatureRenderer);
            }
            BuildList();
        }

        #region ItemClasses
        private class RendererItem
        {
            private IFeatureRenderer _renderer = null;
            private bool _showLegend = false;

            public RendererItem(IFeatureRenderer renderer)
            {
                _renderer = renderer;
            }

            public IFeatureRenderer FeatureRenderer
            {
                get { return _renderer; }
                set { _renderer = value; }
            }

            public bool ShowLegend
            {
                get { return _showLegend; }
                set { _showLegend = value; }
            }

            public override string ToString()
            {
                if (_renderer != null)
                {
                    return _renderer.Name;
                }

                return "???";
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

        private RendererItem _selectedItem = null;
        private void RendererBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedItem = RendererBox.SelectedItem as RendererItem;

            btnRemove.Enabled = btnUp.Enabled = btnDown.Enabled = btnProperties.Enabled =
                _selectedItem != null;

            if (_selectedItem != null && _selectedItem.FeatureRenderer is IScaledependent)
            {
                propertyGridScaleDependent.SelectedObject =
                    new SceleDependetent((IScaledependent)_selectedItem.FeatureRenderer);
            }
        }

        private void btnProperties_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null ||
                _selectedItem.FeatureRenderer == null ||
                !(_selectedItem.FeatureRenderer is IPropertyPage))
            {
                return;
            }

            IFeatureRenderer clone = _selectedItem.FeatureRenderer.Clone() as IFeatureRenderer;
            if (clone == null)
            {
                return;
            }

            IPropertyPage page = clone as IPropertyPage;

            Control panel = page.PropertyPage(_layer) as Control;
            if (panel != null)
            {
                FormGroupRendererProperties dlg = new FormGroupRendererProperties(panel);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    int index = _renderer.Renderers.IndexOf(_selectedItem.FeatureRenderer);
                    if (index != -1)
                    {
                        _renderer.Renderers.RemoveAt(index);
                        _renderer.Renderers.Insert(index, clone);
                        _selectedItem.FeatureRenderer = clone;
                        BuildList();
                        RendererBox.SelectedItem = _selectedItem;
                    }
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null || _renderer == null)
            {
                return;
            }

            _renderer.Renderers.Remove(_selectedItem.FeatureRenderer);

            int index2;
            int index = index2 = RendererBox.Items.IndexOf(_selectedItem);
            if (index == -1)
            {
                return;
            }

            for (int i = index + 1; i < RendererBox.Items.Count; i++)
            {
                if (RendererBox.Items[i] is LegendItem)
                {
                    index2 = i;
                }
                else
                {
                    break;
                }
            }
            for (int i = index2; i >= index; i--)
            {
                RendererBox.Items.RemoveAt(i);
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null ||
                _selectedItem.FeatureRenderer == null ||
                _renderer == null)
            {
                return;
            }

            IFeatureRenderer selectedRenderer = _selectedItem.FeatureRenderer;
            int index = _renderer.Renderers.IndexOf(_selectedItem.FeatureRenderer);
            if (index >= _renderer.Renderers.Count)
            {
                return;
            }

            _renderer.Renderers.Remove(_selectedItem.FeatureRenderer);
            _renderer.Renderers.Insert(Math.Min(_renderer.Renderers.Count, index + 1), _selectedItem.FeatureRenderer);

            BuildList();
            SelectRendererItem(selectedRenderer);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null ||
                _selectedItem.FeatureRenderer == null ||
                _renderer == null)
            {
                return;
            }

            IFeatureRenderer selectedRenderer = _selectedItem.FeatureRenderer;
            int index = _renderer.Renderers.IndexOf(_selectedItem.FeatureRenderer);
            if (index <= 0)
            {
                return;
            }

            _renderer.Renderers.Remove(_selectedItem.FeatureRenderer);
            _renderer.Renderers.Insert(Math.Max(0, index - 1), _selectedItem.FeatureRenderer);

            BuildList();
            SelectRendererItem(selectedRenderer);
        }

        #region Helper
        private void SelectRendererItem(IFeatureRenderer renderer)
        {
            foreach (object item in RendererBox.Items)
            {
                if (item is RendererItem && ((RendererItem)item).FeatureRenderer == renderer)
                {
                    RendererBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void ShowHideLegend(RendererItem item)
        {
            if (_renderer == null)
            {
                return;
            }

            int index = RendererBox.Items.IndexOf(item);
            if (index == -1)
            {
                return;
            }

            if (!item.ShowLegend)
            {
                for (int i = index + 1; i < RendererBox.Items.Count; i++)
                {
                    if (RendererBox.Items[i] is RendererItem)
                    {
                        break;
                    }

                    RendererBox.Items.RemoveAt(i);
                    i--;
                }
            }
            else
            {
                if (item.FeatureRenderer is ILegendGroup)
                {
                    ILegendGroup lGroup = item.FeatureRenderer as ILegendGroup;
                    for (int i = 0; i < lGroup.LegendItemCount; i++)
                    {
                        RendererBox.Items.Insert(index + 1, new LegendItem(lGroup.LegendItem(i)));
                    }
                }
            }
        }

        private List<IFeatureRenderer> RendererList()
        {
            List<IFeatureRenderer> renderers = new List<IFeatureRenderer>();
            if (_renderer == null || _renderer.Renderers == null)
            {
                return renderers;
            }

            foreach (IFeatureRenderer renderer in _renderer.Renderers)
            {
                renderers.Add(renderer);
            }

            return renderers;
        }
        #endregion

        private void RendererBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemWidth = RendererBox.Width;
            e.ItemHeight = 20;
        }

        private void RendererBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= RendererBox.Items.Count || e.Index < 0)
            {
                return;
            }

            object item = RendererBox.Items[e.Index];

            if (item is RendererItem)
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
                        (((RendererItem)item).ShowLegend ?
                        global::gView.Win.Carto.Rendering.UI.Properties.Resources.CollapseIcon :
                        global::gView.Win.Carto.Rendering.UI.Properties.Resources.ExpandIcon),
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
                        e.Graphics.DrawSymbol((ISymbol)legendItem, rect);
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
            if (item == null)
            {
                return;
            }

            if (item is RendererItem)
            {
                if (e.X > 5 && e.X < 17)
                {
                    ((RendererItem)item).ShowLegend = !((RendererItem)item).ShowLegend;
                    ShowHideLegend(item as RendererItem);
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
            if (item == null)
            {
                return;
            }

            if (item is LegendItem && _renderer is ILegendGroup)
            {
                if (_mX > 20 && _mX < 50)
                {
                    ISymbol symbol = ((LegendItem)item).Item as ISymbol;
                    if (symbol != null)
                    {
                        FormSymbol dlg = new FormSymbol(symbol);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            ((ILegendGroup)_renderer).SetSymbol(((LegendItem)item).Item, dlg.Symbol);
                            ((LegendItem)item).Item = dlg.Symbol;
                            RendererBox.Refresh();
                        }
                    }
                }
            }
        }

        private int _mX = 0, _mY = 0;
        private void RendererBox_MouseMove(object sender, MouseEventArgs e)
        {
            _mX = e.X;
            _mY = e.Y;
        }

        private class SceleDependetent
        {
            IScaledependent _scaleDependent;

            public SceleDependetent(IScaledependent scaleDependent)
            {
                _scaleDependent = scaleDependent;
            }

            [Browsable(true)]
            [Description("Maximum (upper) scale. Type 0 to ignor this value.")]
            public double MaximumScale
            {
                get { return _scaleDependent.MinimumScale; }
                set { _scaleDependent.MinimumScale = value; }
            }

            [Browsable(true)]
            [Description("Minimum (lower) scale. Type 0 to ignor this value.")]
            public double MinimumScale
            {
                get { return _scaleDependent.MaximumScale; }
                set { _scaleDependent.MaximumScale = value; }
            }
        }
    }
}