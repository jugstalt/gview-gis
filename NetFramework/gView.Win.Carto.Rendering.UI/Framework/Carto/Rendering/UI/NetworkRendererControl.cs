using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Symbology;
using gView.Framework.Carto.UI;
using gView.Framework.Symbology.UI;

namespace gView.Framework.Carto.Rendering.UI
{
    public partial class NetworkRendererControl : UserControl, IPropertyPanel
    {
        NetworkRenderer _renderer = null;

        public NetworkRendererControl()
        {
            InitializeComponent();
        }

        #region IPropertyPanel Member

        public object PropertyPanel(gView.Framework.Carto.IFeatureRenderer renderer, gView.Framework.Data.IFeatureLayer fc)
        {
            _renderer = renderer as NetworkRenderer;
            return this;
        }

        #endregion

        #region Events
        private void btnChooseEdgeSymbol_Paint(object sender, PaintEventArgs e)
        {
            if (_renderer == null)
                return;

            ISymbol symbol = _renderer[NetworkRenderer.RendererType.Edges];
            if (symbol != null)
            {
                SymbolPreview.Draw(
                    e.Graphics,
                    new Rectangle(5, 5, btnChooseEdgeSymbol.Width - 10, btnChooseEdgeSymbol.Height - 10),
                    symbol, false);
            }
        }

        private void btnChooseSimpeSwitchSymbol_Paint(object sender, PaintEventArgs e)
        {
            if (_renderer == null)
                return;

            ISymbol symbol = _renderer[NetworkRenderer.RendererType.SimpleNodes];
            if (symbol != null)
            {
                SymbolPreview.Draw(
                    e.Graphics,
                    new Rectangle(5, 5, btnChooseSimpeSwitchSymbol.Width - 10, btnChooseSimpeSwitchSymbol.Height - 10),
                    symbol, false);
            }
        }

        private void btnChooseSwitchOnSymbol_Paint(object sender, PaintEventArgs e)
        {
            if (_renderer == null)
                return;

            ISymbol symbol = _renderer[NetworkRenderer.RendererType.SwitchesOn];
            if (symbol != null)
            {
                SymbolPreview.Draw(
                    e.Graphics,
                    new Rectangle(5, 5, btnChooseSwitchOnSymbol.Width - 10, btnChooseSwitchOnSymbol.Height - 10),
                    symbol, false);
            }
        }

        private void btnChooseSwitchOffSymbol_Paint(object sender, PaintEventArgs e)
        {
            if (_renderer == null)
                return;

            ISymbol symbol = _renderer[NetworkRenderer.RendererType.SwitchesOff];
            if (symbol != null)
            {
                SymbolPreview.Draw(
                    e.Graphics,
                    new Rectangle(5, 5, btnChooseSwitchOffSymbol.Width - 10, btnChooseSwitchOffSymbol.Height - 10),
                    symbol, false);
            }
        }

        private void btnChooseEdgeSymbol_Click(object sender, EventArgs e)
        {
            if (_renderer == null) return;

            ISymbol symbol = _renderer[NetworkRenderer.RendererType.Edges];
            if (symbol != null)
            {
                FormSymbol dlg = new FormSymbol(symbol);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _renderer[NetworkRenderer.RendererType.Edges] = dlg.Symbol;
                }
            }
        }
        
        private void btnChooseSimpeSwitchSymbol_Click(object sender, EventArgs e)
        {
            if (_renderer == null) return;

            ISymbol symbol = _renderer[NetworkRenderer.RendererType.SimpleNodes];
            if (symbol != null)
            {
                FormSymbol dlg = new FormSymbol(symbol);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _renderer[NetworkRenderer.RendererType.SimpleNodes] = dlg.Symbol;
                }
            }
        }

        private void btnChooseSwitchOnSymbol_Click(object sender, EventArgs e)
        {
            if (_renderer == null) return;

            ISymbol symbol = _renderer[NetworkRenderer.RendererType.SwitchesOn];
            if (symbol != null)
            {
                FormSymbol dlg = new FormSymbol(symbol);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _renderer[NetworkRenderer.RendererType.SwitchesOn] = dlg.Symbol;
                }
            }
        }

        private void btnChooseSwitchOffSymbol_Click(object sender, EventArgs e)
        {
            if (_renderer == null) return;

            ISymbol symbol = _renderer[NetworkRenderer.RendererType.SwitchesOff];
            if (symbol != null)
            {
                FormSymbol dlg = new FormSymbol(symbol);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _renderer[NetworkRenderer.RendererType.SwitchesOff] = dlg.Symbol;
                }
            }
        }
        #endregion
    }
}
