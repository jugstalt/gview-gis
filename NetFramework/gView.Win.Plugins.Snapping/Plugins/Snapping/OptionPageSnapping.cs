using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Snapping.Core;
using gView.Framework.Data;
using gView.Framework.system;
using gView.Framework.Carto;
using gView.Framework.Globalisation;

namespace gView.Plugins.Snapping
{
    internal partial class OptionPageSnapping : UserControl
    {
        Module _module = null;

        public OptionPageSnapping()
        {
            InitializeComponent();
        }
        public OptionPageSnapping(Module module)
            :this()
        {
            _module = module;
            panelPage.Dock = DockStyle.Fill;
        }

        internal Module Module
        {
            get { return _module; }
            set { _module = value; }
        }

        public void MakeGUI()
        {
            dgSchemas.Rows.Clear();
            cmbSchemas.Items.Clear();

            if (_module == null || _module.MapDocument == null) return;
            List<ISnapSchema> schemas = _module[_module.MapDocument.FocusMap];
            if (schemas == null) return;

            foreach (ISnapSchema schema in schemas)
            {
                if (schema == null) return;
                cmbSchemas.Items.Add(new SnapSchemaItem(_module.MapDocument.FocusMap, schema));
            }
            cmbSchemas.Enabled = (cmbSchemas.Items.Count != 0);
            if (cmbSchemas.Items.Count > 0)
                cmbSchemas.SelectedIndex = 0;
        }

        private void cmbSchemas_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgSchemas.Rows.Clear();
            if (_module == null || _module.MapDocument == null || 
                _module.MapDocument.FocusMap == null ||
                !(cmbSchemas.SelectedItem is SnapSchemaItem)) return;

            numScale.Value = (decimal)((SnapSchemaItem)cmbSchemas.SelectedItem).SnapSchema.MaxScale;

            foreach (SnapLayerRow row in ((SnapSchemaItem)cmbSchemas.SelectedItem).SnapLayerRows)
            {
                dgSchemas.Rows.Add(row);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (_module == null || _module.MapDocument == null ||
                _module[_module.MapDocument.FocusMap] == null) return;

            FormNewSchema dlg = new FormNewSchema("New Schema");

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SnapSchema schema=new SnapSchema(dlg.SnapSchemaName);
                //_module[_module.MapDocument.FocusMap].Add(schema);
                cmbSchemas.Items.Add(new SnapSchemaItem(_module.MapDocument.FocusMap, schema));

                cmbSchemas.SelectedIndex = cmbSchemas.Items.Count - 1;
            }
        }

        private void toolRemoveSchema_Click(object sender, EventArgs e)
        {
            if (_module == null || _module.MapDocument == null ||
                _module[_module.MapDocument.FocusMap] == null) return;

            if (cmbSchemas.SelectedItem is SnapSchemaItem)
            { 
                cmbSchemas.Items.Remove(cmbSchemas.SelectedItem);
            }
        }

        private void toolMoveUp_Click(object sender, EventArgs e)
        {
            if (cmbSchemas.SelectedItem is SnapSchemaItem)
            {
                SnapSchemaItem item = (SnapSchemaItem)cmbSchemas.SelectedItem;

                int index = cmbSchemas.Items.IndexOf(item);
                if (index <= 0) return;

                cmbSchemas.Items.Remove(item);
                cmbSchemas.Items.Insert(index - 1, item);
                cmbSchemas.SelectedIndex = index - 1;
            }
        }

        private void toolMoveDown_Click(object sender, EventArgs e)
        {
            if (cmbSchemas.SelectedItem is SnapSchemaItem)
            {
                SnapSchemaItem item = (SnapSchemaItem)cmbSchemas.SelectedItem;

                int index = cmbSchemas.Items.IndexOf(item);
                if (index < 0 || index >= cmbSchemas.Items.Count - 1) return;

                cmbSchemas.Items.Remove(item);
                cmbSchemas.Items.Insert(index+1, item);
                cmbSchemas.SelectedIndex = index+1;
            }
        }

        private void toolRenameSchema_Click(object sender, EventArgs e)
        {
            if (_module == null || _module.MapDocument == null ||
                _module[_module.MapDocument.FocusMap] == null) return;

            if (cmbSchemas.SelectedItem is SnapSchemaItem)
            {
                SnapSchemaItem item = (SnapSchemaItem)cmbSchemas.SelectedItem;
                if (!(item.SnapSchema is SnapSchema)) return;

                FormNewSchema dlg = new FormNewSchema(item.SnapSchema.Name);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ((SnapSchema)item.SnapSchema).Name = dlg.SnapSchemaName;

                    int index = cmbSchemas.Items.IndexOf(item);
                    if (index < 0) return;

                    // Refresh List
                    cmbSchemas.Items.Remove(item);
                    cmbSchemas.Items.Insert(index, item);
                    cmbSchemas.SelectedIndex = index;
                }
            }
        }

        #region ItemClasses
        private class SnapSchemaItem
        {
            private ISnapSchema _schema;
            private List<SnapLayerRow> _rows = new List<SnapLayerRow>();

            public SnapSchemaItem(IMap map, ISnapSchema schema)
            {
                _schema = schema;
                if (_schema == null) return;

                foreach (ISnapLayer sLayer in schema)
                {
                    if (!(sLayer is SnapLayer)) continue;
                    _rows.Add(new SnapLayerRow(sLayer as SnapLayer));
                }
                foreach (IDatasetElement element in map.MapElements)
                {
                    if (!(element is IFeatureLayer) ||
                        HasFeatureLayer(element as IFeatureLayer)) continue;

                    SnapLayer sLayer = new SnapLayer(element as IFeatureLayer, SnapMethode.None);
                    _rows.Add(new SnapLayerRow(sLayer));
                }
            }

            public ISnapSchema SnapSchema
            {
                get { return _schema; }
            }
            public override string ToString()
            {
                if (_schema == null) return "???";
                return _schema.Name;
            }
            public List<SnapLayerRow> SnapLayerRows
            {
                get { return _rows; }
            }
            #region Helper
            private bool HasFeatureLayer(IFeatureLayer layer)
            {
                foreach (SnapLayerRow row in _rows)
                    if (row.SnapLayer.FeatureLayer == layer)
                        return true;

                return false;
            }
            #endregion
        }
        private class SnapLayerRow : DataGridViewRow
        {
            SnapLayer _sLayer;
            public SnapLayerRow(SnapLayer sLayer)
            {
                _sLayer = sLayer;
                if (_sLayer == null) return;

                DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                cell.Value = _sLayer.FeatureLayer.Title;
                this.Cells.Add(cell);

                DataGridViewCheckBoxCell c = new DataGridViewCheckBoxCell();
                c.Value=Bit.Has(_sLayer.Methode, SnapMethode.Vertex);
                this.Cells.Add(c);

                c = new DataGridViewCheckBoxCell();
                c.Value = Bit.Has(_sLayer.Methode, SnapMethode.Edge);
                this.Cells.Add(c);

                c = new DataGridViewCheckBoxCell();
                c.Value = Bit.Has(_sLayer.Methode, SnapMethode.EndPoint);
                this.Cells.Add(c);
            }

            public ISnapLayer SnapLayer
            {
                get
                {
                    if (_sLayer == null) return null;
                    _sLayer.Methode = SnapMethode.None;

                    if(this.Cells[1].Value.Equals(true))
                        _sLayer.Methode |= SnapMethode.Vertex;
                    if (this.Cells[2].Value.Equals(true))
                        _sLayer.Methode |= SnapMethode.Edge;
                    if (this.Cells[3].Value.Equals(true))
                        _sLayer.Methode |= SnapMethode.EndPoint;

                    return _sLayer;
                }
            }
        }
        #endregion

        private void dgSchemas_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        public void Commit()
        {
            if (_module == null || _module.MapDocument == null ||
                _module[_module.MapDocument.FocusMap] == null) return;

            numScale_ValueChanged(numScale, new EventArgs());
            List<ISnapSchema> schemas = _module[_module.MapDocument.FocusMap];
            schemas.Clear();

            foreach (SnapSchemaItem item in cmbSchemas.Items)
            {
                SnapSchema schema = new SnapSchema(item.SnapSchema.Name);
                foreach (SnapLayerRow row in item.SnapLayerRows)
                {
                    if (row.SnapLayer.Methode != SnapMethode.None)
                        schema.Add(row.SnapLayer);
                }
                schema.MaxScale = item.SnapSchema.MaxScale;
                schemas.Add(schema);
            }

            _module.RefreshGUI();
        }

        private void numScale_ValueChanged(object sender, EventArgs e)
        {
            if (cmbSchemas.SelectedItem is SnapSchemaItem &&
                ((SnapSchemaItem)cmbSchemas.SelectedItem).SnapSchema is SnapSchema)
            {

                ((SnapSchema)((SnapSchemaItem)cmbSchemas.SelectedItem).SnapSchema).MaxScale = (double)numScale.Value;
            }
        }
    }

    [gView.Framework.system.RegisterPlugIn("F303B1D4-8CE4-4109-9959-2F19E7E9045F")]
    public class SnapOptionPage : IMapOptionPage
    {
        private OptionPageSnapping _page = null;

        #region IMapOptionPage Member

        public Panel OptionPage(IMapDocument document)
        {
            if (!IsAvailable(document))
                return null;

            if (_page == null)
            {
                if (document.Application is IMapApplication)
                {
                    Module module = ((IMapApplication)document.Application).IMapApplicationModule(gView.Framework.Snapping.Core.Globals.ModuleGuid) as Module;
                    if (module == null) return null;
                    _page = new OptionPageSnapping(module);
                }
            }
            _page.MakeGUI();
            return _page.panelPage;
        }

        public string Title
        {
            get { return LocalizedResources.GetResString("String.Snapping", "Snapping"); }
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
