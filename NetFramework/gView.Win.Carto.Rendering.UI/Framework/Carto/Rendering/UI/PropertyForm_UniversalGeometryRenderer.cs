using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.UI;
using gView.Framework.Carto.UI;
using gView.Framework.Symbology.UI;

namespace gView.Framework.Carto.Rendering.UI
{
    public partial class PropertyForm_UniversalGeometryRenderer : Form, IPropertyPanel
    {
        private IFeatureLayer _layer = null;
        private UniversalGeometryRenderer _renderer = null;
        public PropertyForm_UniversalGeometryRenderer()
        {
            
        }

        private void btnChooseSymbol_Paint(object sender, PaintEventArgs e)
        {
            if (_renderer == null) return;

            SymbolPreview.Draw(
                e.Graphics,
                new Rectangle(5, 5, btnChooseSymbol.Width - 10, btnChooseSymbol.Height - 10),
                _renderer[geometryType.Point], false);
        }

        private void btnChoosePolygonSymbol_Paint(object sender, PaintEventArgs e)
        {
            if (_renderer == null) return;

            SymbolPreview.Draw(
                e.Graphics,
                new Rectangle(5, 5, btnChooseSymbol.Width - 10, btnChooseSymbol.Height - 10),
                _renderer[geometryType.Polygon], false);
        }

        private void btnChooseLineSymbol_Paint(object sender, PaintEventArgs e)
        {
            if (_renderer == null) return;

            SymbolPreview.Draw(
                e.Graphics,
                new Rectangle(5, 5, btnChooseSymbol.Width - 10, btnChooseSymbol.Height - 10),
                _renderer[geometryType.Polyline], false);
        }

        private void btnChooseSymbol_Click(object sender, EventArgs e)
        {
            if (_renderer == null) return;

            FormSymbol dlg = new FormSymbol(_renderer[geometryType.Point]);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _renderer[geometryType.Point] = dlg.Symbol;
            }
        }

        private void btnChooseLineSymbol_Click(object sender, EventArgs e)
        {
            if (_renderer == null) return;

            FormSymbol dlg = new FormSymbol(_renderer[geometryType.Polyline]);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _renderer[geometryType.Polyline] = dlg.Symbol;
            }
        }

        private void btnChoosePolygonSymbol_Click(object sender, EventArgs e)
        {
            if (_renderer == null) return;

            FormSymbol dlg = new FormSymbol(_renderer[geometryType.Polygon]);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _renderer[geometryType.Polygon] = dlg.Symbol;
            }
        }

        private void btnRotation_Click(object sender, EventArgs e)
        {
            if (_renderer == null || _layer==null || _layer.FeatureClass==null) return;

            FormRotationType dlg = new FormRotationType(_renderer.SymbolRotation, _layer.FeatureClass);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
               
            }
        }

        private void chkUsePointSymbol_CheckedChanged(object sender, EventArgs e)
        {
            if (_renderer != null)
                _renderer.UsePointSymbol = chkUsePointSymbol.Checked;
        }

        private void chkUseLineSymbol_CheckedChanged(object sender, EventArgs e)
        {
            if (_renderer != null)
                _renderer.UseLineSymbol = chkUseLineSymbol.Checked;
        }

        private void chkUsePolyonSymbol_CheckedChanged(object sender, EventArgs e)
        {
            if (_renderer != null)
                _renderer.UsePolygonSymbol = chkUsePolyonSymbol.Checked;
        }

        #region IPropertyPanel Member

        public object PropertyPanel(IFeatureRenderer renderer, IFeatureLayer layer)
        {
            InitializeComponent();

            _renderer = renderer as UniversalGeometryRenderer;
            _layer = layer;

            if (_renderer != null)
            {
                chkUsePointSymbol.Checked = _renderer.UsePointSymbol;
                chkUseLineSymbol.Checked = _renderer.UseLineSymbol;
                chkUsePolyonSymbol.Checked = _renderer.UsePolygonSymbol;
            }

            return panel1;
        }

        #endregion
    }
}