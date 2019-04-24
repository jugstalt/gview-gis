using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.Editor.Core;

namespace gView.Plugins.Editor.Controls
{
    internal partial class OptionPageEditing : UserControl
    {
        private Module _module;

        public OptionPageEditing()
        {
            InitializeComponent();
        }
        public OptionPageEditing(Module module)
            : this()
        {
            _module = module;
        }

        internal Module Module
        {
            get { return _module; }
            set { _module = value; }
        }

        public void MakeGUI()
        {
            dgEditLayers.Rows.Clear();
            if (_module == null || _module.MapDocument == null ||
                _module.MapDocument.FocusMap == null) return;


            foreach (IEditLayer eLayer in _module.EditLayers)
            {
                if (!(eLayer is EditLayer) ||
                    _module.MapDocument.FocusMap[eLayer.FeatureLayer] == null) continue;

                dgEditLayers.Rows.Add(new EditLayerRow((EditLayer)eLayer));
            }
        }
        public void Commit()
        {
            if (_module == null || _module.MapDocument == null ||
                _module.MapDocument.FocusMap == null) return;

            foreach (EditLayerRow row in dgEditLayers.Rows)
            {
                foreach (IEditLayer eLayer in _module.EditLayers)
                {
                    if (eLayer.FeatureLayer == row.EditLayer.FeatureLayer &&
                        eLayer is EditLayer)
                    {
                        ((EditLayer)eLayer).Statements = row.EditLayer.Statements;
                    }
                }
            }

            _module.FireOnEditLayerCollectionChanged();
        }

        #region ItemClasses
        private class EditLayerRow : DataGridViewRow
        {
            EditLayer _eLayer;
            public EditLayerRow(EditLayer eLayer)
            {
                _eLayer = eLayer;
                if (_eLayer == null) return;

                DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                cell.Value = _eLayer.FeatureLayer.Title;
                this.Cells.Add(cell);

                DataGridViewCheckBoxCell c = new DataGridViewCheckBoxCell();
                c.Value = Bit.Has(_eLayer.Statements, EditStatements.INSERT);
                this.Cells.Add(c);

                c = new DataGridViewCheckBoxCell();
                c.Value = Bit.Has(_eLayer.Statements, EditStatements.UPDATE);
                this.Cells.Add(c);

                c = new DataGridViewCheckBoxCell();
                c.Value = Bit.Has(_eLayer.Statements, EditStatements.DELETE);
                this.Cells.Add(c);
            }

            public IEditLayer EditLayer
            {
                get
                {
                    if (_eLayer == null) return null;
                    _eLayer.Statements = EditStatements.NONE;

                    if (this.Cells[1].Value.Equals(true))
                        _eLayer.Statements |= EditStatements.INSERT;
                    if (this.Cells[2].Value.Equals(true))
                        _eLayer.Statements |= EditStatements.UPDATE;
                    if (this.Cells[3].Value.Equals(true))
                        _eLayer.Statements |= EditStatements.DELETE;

                    return _eLayer;
                }
            }
        }
        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("16D3147C-4D28-4481-BA16-4C7580F8FAB8")]
    public class SnapOptionPage : IMapOptionPage
    {
        private OptionPageEditing _page = null;

        #region IMapOptionPage Member

        public Panel OptionPage(IMapDocument document)
        {
            if (!IsAvailable(document))
                return null;

            if (_page == null)
            {
                if (document.Application is IMapApplication)
                {
                    Module module = ((IMapApplication)document.Application).IMapApplicationModule(Globals.ModuleGuid) as Module;
                    if (module == null) return null;
                    _page = new OptionPageEditing(module);
                }
            }
            _page.MakeGUI();
            return _page.panelPage;
        }

        public string Title
        {
            get { return "Editing"; }
        }

        public Image Image
        {
            get { return null; }
        }

        public void Commit()
        {
            if (_page != null)
                _page.Commit();
        }

        public bool IsAvailable(IMapDocument document)
        {
            if (document == null || document.Application == null) return false;

            if (document.Application is IMapApplication &&
                ((IMapApplication)document.Application).ReadOnly == true) return false;

            return true;
        }

        #endregion
    }
}
