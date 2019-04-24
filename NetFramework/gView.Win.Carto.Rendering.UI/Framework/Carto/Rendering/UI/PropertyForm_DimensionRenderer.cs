using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Symbology;
using gView.Framework.Carto.UI;
using gView.Framework.Symbology.UI;

namespace gView.Framework.Carto.Rendering.UI
{
    public partial class PropertyForm_DimensionRenderer : Form, IPropertyPanel
    {
        private DimensionRenderer _renderer = null;
        private IFeatureClass _featureClass = null;

        public PropertyForm_DimensionRenderer()
        {
            //InitializeComponent();
        }

        #region IPropertyPanel Member

        public object PropertyPanel(IFeatureRenderer renderer, IFeatureLayer layer)
        {
            _renderer = renderer as DimensionRenderer;
            if (layer != null)
                _featureClass = layer.FeatureClass;

            InitializeComponent();

            foreach (object e in Enum.GetValues(typeof(DimensionRenderer.lineCapType)))
                cmbLineCap.Items.Add(e);

            if (_renderer != null)
            {
                cmbLineCap.SelectedItem = _renderer.LineCapType;
                txtFormat.Text = _renderer.LabelFormat;
            }
            else
            {
                if (cmbLineCap.Items.Count > 0)
                    cmbLineCap.SelectedIndex = 0;
            }
            return panel1;
        }

        #endregion

        #region Symbology Events
        private void btnLineSymbol_Paint(object sender, PaintEventArgs e)
        {
            if (_renderer == null) return;

            SymbolPreview.Draw(
                e.Graphics,
                new Rectangle(5, 5, btnLineSymbol.Width - 10, btnLineSymbol.Height - 10),
                ((DimensionRenderer)_renderer).LineSymbol, false);
        }

        private void btnTextSymbol_Paint(object sender, PaintEventArgs e)
        {
            if (_renderer == null) return;

            SymbolPreview.Draw(
                e.Graphics,
                new Rectangle(5, 5, btnTextSymbol.Width - 10, btnTextSymbol.Height - 10),
                ((DimensionRenderer)_renderer).TextSymbol, false);
        }

        private void btnLineSymbol_Click(object sender, EventArgs e)
        {
            if (_renderer == null) return;

            FormSymbol dlg = new FormSymbol(((DimensionRenderer)_renderer).LineSymbol);
            if (dlg.ShowDialog() == DialogResult.OK &&
                dlg.Symbol is ILineSymbol)
            {
                ((DimensionRenderer)_renderer).LineSymbol = (ILineSymbol)dlg.Symbol;
            }
        }

        private void btnTextSymbol_Click(object sender, EventArgs e)
        {
            if (_renderer == null) return;

            FormSymbol dlg = new FormSymbol(((DimensionRenderer)_renderer).TextSymbol);
            if (dlg.ShowDialog() == DialogResult.OK &&
                dlg.Symbol is ITextSymbol)
            {
                ((DimensionRenderer)_renderer).TextSymbol = (ITextSymbol)dlg.Symbol;
            }
        }
        #endregion

        #region Design Events
        private void cmbLineCap_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1) return;

            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
            {
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);
            }

            DimensionRenderer.lineCapType type = (DimensionRenderer.lineCapType)cmbLineCap.Items[e.Index];
            switch (type)
            {
                case DimensionRenderer.lineCapType.Arrow:
                    DrawArrow(e.Graphics, e.Bounds, true);
                    break;
                case DimensionRenderer.lineCapType.ArrowLine:
                    DrawArrowLine(e.Graphics, e.Bounds, true);
                    break;
                case DimensionRenderer.lineCapType.Line:
                    DrawLine(e.Graphics, e.Bounds, true);
                    break;
                case DimensionRenderer.lineCapType.Circle:
                    DrawCircle(e.Graphics, e.Bounds, true);
                    break;
            }
        }

        private void cmbLineCap_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_renderer != null)
                _renderer.LineCapType = (DimensionRenderer.lineCapType)cmbLineCap.SelectedItem;
        }

        private void txtFormat_TextChanged(object sender, EventArgs e)
        {
            if (_renderer != null)
                _renderer.LabelFormat = txtFormat.Text;
        }
        #endregion

        #region Drawing
        private void DrawArrow(System.Drawing.Graphics g, Rectangle bounds, bool left)
        {
            int x1 = (left ? 10 : bounds.Right - 10);
            int y1 = bounds.Top + bounds.Height / 2;
            int x2 = (left ? bounds.Right - 10 : 10);
            int y2 = y1;

            g.DrawLine(Pens.Black, x1, y1, x2, y2);
            if (left)
            {
                g.DrawLine(Pens.Black, x1, y1, x1 + 7, y1 + 7);
                g.DrawLine(Pens.Black, x1, y1, x1 + 7, y1 - 7);
            }
            else
            {
                g.DrawLine(Pens.Black, x1, y1, x1 - 7, y1 + 7);
                g.DrawLine(Pens.Black, x1, y1, x1 - 7, y1 - 7);
            }
        }
        private void DrawArrowLine(System.Drawing.Graphics g, Rectangle bounds, bool left)
        {
            int x1 = (left ? 10 : bounds.Right - 10);
            int y1 = bounds.Top + bounds.Height / 2;
            int x2 = (left ? bounds.Right - 10 : 10);
            int y2 = y1;

            DrawArrow(g, bounds, left);
            if (left)
                g.DrawLine(Pens.Black, x1, y1 - 7, x1, y1 + 7);
            else
                g.DrawLine(Pens.Black, x2, y2 - 7, x2, y2 + 7);
        }
        private void DrawLine(System.Drawing.Graphics g, Rectangle bounds, bool left)
        {
            int x1 = (left ? 10 : bounds.Right - 10);
            int y1 = bounds.Top + bounds.Height / 2;
            int x2 = (left ? bounds.Right - 10 : 10);
            int y2 = y1;

            g.DrawLine(Pens.Black, x1, y1, x2, y2);
            if (left)
            {
                g.DrawLine(Pens.Black, x1, y1 - 7, x1, y1 + 7);
                g.DrawLine(Pens.Black, x1 - 5, y1 + 5, x1 + 5, y1 - 5);
            }
            else
            {
                g.DrawLine(Pens.Black, x2, y2 - 7, x2, y2 + 7);
                g.DrawLine(Pens.Black, x2 - 5, y2 + 5, x2 + 5, y2 - 5);
            }
        }
        private void DrawCircle(System.Drawing.Graphics g, Rectangle bounds, bool left)
        {
            int x1 = (left ? 10 : bounds.Right - 10);
            int y1 = bounds.Top + bounds.Height / 2;
            int x2 = (left ? bounds.Right - 10 : 10);
            int y2 = y1;

            g.DrawLine(Pens.Black, x1, y1, x2, y2);
            if (left)
            {
                g.DrawEllipse(Pens.Black, x1 - 5, y1 - 5, 10, 10);
                g.DrawLine(Pens.Black, x1, y1 - 7, x1, y1 + 7);
            }
            else
            {
                g.DrawEllipse(Pens.Black, x2 - 5, y2 - 5, 10, 10);
                g.DrawLine(Pens.Black, x2, y2 - 7, x2, y2 + 7);
            }
        }
        #endregion
        
    }
}