using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.system;

namespace gView.Framework.UI.Dialogs
{
    [RegisterPlugIn("F8055CEE-8BC3-4379-9737-6DDA2799028F")]
    public partial class FormGridSettings : Form, ILayerPropertyPage
    {
        private IGridClass _class = null;
        private double[] _hillShadeVector;

        public FormGridSettings()
        {
            InitializeComponent();
        }

        #region ILayerPropertyPage Member

        public Panel PropertyPage(gView.Framework.Data.IDataset dataset, gView.Framework.Data.ILayer layer)
        {
            if (layer == null) return null;
            _class = layer.Class as IGridClass;

            panel1.Dock = DockStyle.Fill;
            MakeGUI();
            return panel1;
        }

        public bool ShowWith(gView.Framework.Data.IDataset dataset, gView.Framework.Data.ILayer layer)
        {
            if (layer != null &&
                layer.Class is IGridClass &&
                ((IGridClass)layer.Class).ImplementsRenderMethods != GridRenderMethode.None)
            {
                return true;
            }

            return false;
        }

        public string Title
        {
            get { return "Grid"; }
        }

        public void Commit()
        {
            if (_class == null) return;

            _class.HillShadeVector = gridControl1.HillShadeVector;

            _class.UseHillShade = gridControl1.UseHillShade;
            _class.ColorClasses = gridControl1.GridColorClasses;
        }

        #endregion

        private void MakeGUI()
        {
            if (_class == null) return;

            _hillShadeVector = new double[3];
            if (_class.HillShadeVector != null &&
                _class.HillShadeVector.Length == 3)
                _class.HillShadeVector.CopyTo(_hillShadeVector, 0);

            gridControl1.UseHillShade = _class.UseHillShade;
            gridControl1.HillShadeVector = _hillShadeVector;
            gridControl1.MinValue = _class.MinValue;
            gridControl1.MaxValue = _class.MaxValue;

            gridControl1.EnableHillShade = Bit.Has(_class.ImplementsRenderMethods, GridRenderMethode.HillShade);
            gridControl1.GridColorClasses = _class.ColorClasses;
        }
    }
}