using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Plugins.Editor.Dialogs;
using gView.Framework.Editor.Core;

namespace gView.Plugins.Editor.Controls
{
    [gView.Framework.system.RegisterPlugIn("9A1323A9-A25E-41f5-9D5B-65C3FF3351F8")]
    public partial class EditLayerPropertyPage : UserControl, ILayerPropertyPage
    {
        IFeatureLayer _fLayer = null;

        public EditLayerPropertyPage()
        {
            InitializeComponent();
        }

        #region ILayerPropertyPage Member

        public Panel PropertyPage(gView.Framework.Data.IDataset dataset, gView.Framework.Data.ILayer layer)
        {
            if (!ShowWith(dataset, layer)) return null;

            _fLayer = (IFeatureLayer)layer;
            foreach (IField field in _fLayer.Fields.ToEnumerable())
            {
                if (field.type == FieldType.Shape ||
                    field.type == FieldType.ID ||
                    field.type == FieldType.binary) continue;

                dgFields.Rows.Add(
                    new object[] {
                        field.IsEditable,
                        field.name,
                        field.aliasname,
                        field.IsRequired,
                        field.DefautValue,
                        (field.Domain==null) ? "none" : field.Domain.Name });
            }

            return panelPage;
        }

        public bool ShowWith(gView.Framework.Data.IDataset dataset, gView.Framework.Data.ILayer layer)
        {
            return (layer is IFeatureLayer && dataset != null && dataset.Database is IEditableDatabase);
        }

        public string Title
        {
            get { return "Edit"; }
        }

        public void Commit()
        {
            if (_fLayer == null) return;

            foreach (DataGridViewRow row in dgFields.Rows)
            {
                Field field = _fLayer.Fields.FindField(row.Cells[2].Value.ToString()) as Field;
                if (field == null) continue;

                field.IsEditable = (bool)row.Cells[0].Value;
                field.IsRequired = (bool)row.Cells[3].Value;
                field.DefautValue = row.Cells[4].Value;
            }
        }

        #endregion

        private void dgFields_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_fLayer == null) return;

            if (e.ColumnIndex == 5) // Domain
            {
                Field field=_fLayer.Fields.FindField(dgFields.Rows[e.RowIndex].Cells[2].Value.ToString()) as Field;

                if (field != null)
                {
                    FormDomains dlg = new FormDomains((field.Domain != null) ? field.Domain.Clone() as IFieldDomain : null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        field.Domain = dlg.Domain;
                        dgFields.Rows[e.RowIndex].Cells[e.ColumnIndex].Value =
                            (dlg.Domain == null) ? "none" : dlg.Domain.Name;
                    }
                }
            }
        }
    }
}
