using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.UI.Dialogs;
using gView.Framework.FDB;
using gView.Framework.system;
using gView.Framework.UI.Controls.Filter;

namespace gView.DataSources.Fdb.UI
{
    [gView.Framework.system.RegisterPlugIn("30B86C28-FB47-4ee2-B15F-06D4BD8F47D2")]
    public partial class FeatureClassExplorerTabPage : UserControl, IExplorerTabPage
    {
        private IExplorerObject _exObject = null;

        public FeatureClassExplorerTabPage()
        {
            InitializeComponent();
        }

        #region IExplorerTabPage Members

        public new Control Control
        {
            get { return this; }
        }

        public void OnCreate(object hook)
        {
            
        }

        public void OnShow()
        {
            listView1.Items.Clear();
            if (_exObject == null || !(_exObject.Object is IFeatureClass)) return;

            IFeatureClass fc = (IFeatureClass)_exObject.Object;

            if (fc.Fields == null) return;
            foreach (IField field in fc.Fields.ToEnumerable())
            {
                int iIndex = (field is IAutoField) ? 5 : 0;
                switch (field.type)
                {
                    case FieldType.ID:
                        iIndex = 4;
                        break;
                    case FieldType.Shape:
                        switch (fc.GeometryType)
                        {
                            case geometryType.Point:
                            case geometryType.Multipoint:
                                iIndex = 1;
                                break;
                            case geometryType.Polyline:
                                iIndex = 2;
                                break;
                            case geometryType.Envelope:
                            case geometryType.Polygon:
                                iIndex = 3;
                                break;
                        }
                        break;
                }
                listView1.Items.Add(new FieldListViewItem(new Field(field), iIndex));
            }
        }

        public void OnHide()
        {
            
        }

        public IExplorerObject ExplorerObject
        {
            get
            {
                return _exObject;
            }
            set
            {
                if (value != null || value.Object is IFeatureClass)
                {
                    _exObject = value;
                }
                else
                {
                    _exObject = null;
                }
            }
        }

        public bool ShowWith(IExplorerObject exObject)
        {
            if (exObject == null) return false;
            //return exObject.Object is IFeatureClass;
            return TypeHelper.Match(exObject.ObjectType, typeof(IFeatureClass));
        }

        public string Title
        {
            get { return "FeatureClass"; }
        }

        public void RefreshContents()
        {
        }
        #endregion

        #region IOrder Members

        public int SortOrder
        {
            get { return 0; }
        }

        #endregion

        #region ContextMenu
        ListViewItem _contextItem = null;
        private void listView1_Click(object sender, EventArgs e)
        {
            ShowContextMenu();   
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            _button = e.Button;
            if (listView1.GetItemAt(_mx, _my) != null) return;

            ShowContextMenu();
        }

        private void ShowContextMenu()
        {
            if (_button != MouseButtons.Right) return;
            _contextItem = listView1.GetItemAt(_mx, _my);

            contextItem_AddField.Visible =
                contextItem_ImportFields.Visible = _contextItem == null;

            contextItem_Remove.Visible =
                contextItem_Properties.Visible = _contextItem != null;

            contextMenuStrip.Show(this, _mx, _my);
        }

        int _mx = 0, _my = 0;
        MouseButtons _button = MouseButtons.None;
        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            _mx = e.X;
            _my = e.Y;
        }
        #endregion

        private void contextItem_AddField_Click(object sender, EventArgs e)
        {
            Field nField = new Field("NewField", FieldType.String);
            FormPropertyGrid dlg = new FormPropertyGrid(nField);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                nField = dlg.SelectedObject as Field;

                if (AlterTable == null)
                {
                    MessageBox.Show("Change properties is not implemented for this feature...");
                    return;
                }

                if (!AlterTable.AlterTable(_exObject.Name, null, nField))
                {
                    MessageBox.Show("ERROR: " + ((AlterTable is IDatabase) ? ((IDatabase)AlterTable).lastErrorMsg : ""));
                    return;
                }

                this.OnShow();
            }
        }

        private void contextItem_ImportFields_Click(object sender, EventArgs e)
        {
            if (AlterTable == null)
            {
                MessageBox.Show("Change properties is not implemented for this feature...");
                return;
            }
            if (_exObject == null || !(_exObject.Object is ITableClass)) return;

            List<ExplorerDialogFilter> filters=new List<ExplorerDialogFilter>();
            filters.Add(new OpenFeatureclassFilter());
            
            ExplorerDialog dlg = new ExplorerDialog("Open Featureclass", filters, true);
            dlg.MulitSelection = false;

            if (dlg.ShowDialog() == DialogResult.OK &&
                dlg.ExplorerObjects != null &&
                dlg.ExplorerObjects.Count == 1 &&
                dlg.ExplorerObjects[0].Object is ITableClass)
            {
                ITableClass tcFrom = dlg.ExplorerObjects[0].Object as ITableClass;
                ITableClass tcTo = _exObject.Object as ITableClass;

                FormSelectFields selDlg = new FormSelectFields(tcFrom, tcTo);
                if (selDlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (IField field in selDlg.SelectedFields)
                    {
                        if (!AlterTable.AlterTable(_exObject.Name, null, new Field(field)))
                        {
                            MessageBox.Show("ERROR :" + ((AlterTable is IDatabase) ? ((IDatabase)AlterTable).lastErrorMsg : ""));
                            break;
                        }
                    }
                    this.OnShow();
                }
            }
        }

        private void contextItem_Remove_Click(object sender, EventArgs e)
        {
            if (AlterTable == null)
            {
                MessageBox.Show("Change properties is not implemented for this feature...");
                return;
            }

            foreach (ListViewItem item in listView1.SelectedItems)
            {
                if (!(item is FieldListViewItem)) continue;
                Field oField = ((FieldListViewItem)item).Field;

                if (!AlterTable.AlterTable(_exObject.Name, oField, null))
                {
                    MessageBox.Show("ERROR :" + ((AlterTable is IDatabase) ? ((IDatabase)AlterTable).lastErrorMsg : ""));
                    break;
                }
            }
            this.OnShow();
        }

        private void contextItem_Properties_Click(object sender, EventArgs e)
        {
            
            if (_contextItem is FieldListViewItem)
            {
                Field oField=((FieldListViewItem)_contextItem).Field;
                FormPropertyGrid dlg = new FormPropertyGrid(
                    new Field(oField));

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Field nField = dlg.SelectedObject as Field;
                    if (!oField.Equals(nField))
                    {
                        if (AlterTable == null)
                        {
                            MessageBox.Show("Change properties is not implemented for this feature...");
                            return;
                        }

                        if (!AlterTable.AlterTable(_exObject.Name, oField, nField))
                        {
                            MessageBox.Show("ERROR: " + ((AlterTable is IDatabase) ? ((IDatabase)AlterTable).lastErrorMsg : ""));
                            return;
                        }

                        this.OnShow();
                    }
                }
            }
        }

        private gView.Framework.FDB.IAltertable AlterTable
        {
            get
            {
                if (_exObject == null) return null;
                if (!(_exObject.Object is ITableClass)) return null;

                ITableClass tc = _exObject.Object as ITableClass;
                if (tc.Dataset == null || tc.Dataset.Database == null) return null;

                return tc.Dataset.Database as gView.Framework.FDB.IAltertable;
            }
        }
    }

    internal class FieldListViewItem : ListViewItem
    {
        private Field _field;

        public FieldListViewItem(Field field,int iIndex) 
        {
            _field = field;
            if (_field == null) return;

            this.Text = field.name;
            this.SubItems.Add(field.aliasname);
            this.SubItems.Add(field.type.ToString());

            this.ImageIndex = iIndex;
        }

        public Field Field
        {
            get { return _field; }
        }
    }
}
