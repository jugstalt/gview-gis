using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.Data.Relations;
using gView.Framework.Carto;

namespace gView.Plugins.DbTools.Relates
{
    public partial class AddTableRelationDialog : Form
    {
        private IMapDocument _mapDocument;

        public AddTableRelationDialog(IMapDocument mapDocument, IFeatureLayer layer)
        {
            InitializeComponent();

            _mapDocument = mapDocument;

            foreach (IMap map in _mapDocument.Maps)
            {
                foreach (IDatasetElement element in map.MapElements)
                {
                    if (element == null || !(element.Class is ITableClass)) continue;

                    cmbLeftTable.Items.Add(new DatasetElementItem() { Map = map, DatasetElement = element });
                    cmbRightTable.Items.Add(new DatasetElementItem() { Map = map, DatasetElement = element });
                }
            }
            this.LeftTable = layer;

            cmbLogicalOperator.SelectedIndex = 0;
        }

        #region Properties

        public string RelationName
        {
            get { return txtName.Text; }
            set
            {
                txtName.Text = value;
                ValidateOk();
            }
        }
        public IDatasetElement LeftTable
        {
            get
            {
                if (cmbLeftTable.SelectedItem == null) return null;
                return ((DatasetElementItem)cmbLeftTable.SelectedItem).DatasetElement;
            }
            set
            {
                foreach (DatasetElementItem item in cmbLeftTable.Items)
                {
                    if (item.DatasetElement == value)
                    {
                        cmbLeftTable.SelectedItem = item;
                        break;
                    }
                }
                ValidateOk();
            }
        }

        public IDatasetElement RightTable
        {
            get
            {
                if (cmbRightTable.SelectedItem == null) return null;
                return ((DatasetElementItem)cmbRightTable.SelectedItem).DatasetElement;
            }
            set
            {
                foreach (DatasetElementItem item in cmbRightTable.Items)
                {
                    if (item.DatasetElement == value)
                    {
                        cmbRightTable.SelectedItem = item;
                        break;
                    }
                }
                ValidateOk();
            }
        }

        public string LeftTableField
        {
            get { return (string)cmbLeftTableField.SelectedItem; }
            set
            {
                cmbLeftTableField.SelectedItem = value;
                ValidateOk();
            }
        }

        public string RightTableField
        {
            get { return (string)cmbRightTableField.SelectedItem; }
            set
            {
                cmbRightTableField.SelectedItem = value;
                ValidateOk();
            }
        }

        public string LogicalOperator
        {
            get { return (string)cmbLogicalOperator.SelectedItem; }
            set { cmbLogicalOperator.SelectedItem = value; }
        }

        public ITableRelation TableRelation
        {
            get
            {
                TableRelation tr = new TableRelation(_mapDocument);

                tr.RelationName = this.RelationName;
                tr.LeftTable = this.LeftTable;
                tr.LeftTableField = this.LeftTableField;
                tr.RightTable = this.RightTable;
                tr.RightTableField = this.RightTableField;
                tr.LogicalOperator = this.LogicalOperator;

                return tr;
            }

            set
            {
                if (value != null)
                {
                    this.RelationName = value.RelationName;

                    this.LeftTable = value.LeftTable;
                    this.LeftTableField = value.LeftTableField;

                    this.RightTable = value.RightTable;
                    this.RightTableField = value.RightTableField;

                    this.LogicalOperator = value.LogicalOperator;
                }
            }
        }

        #endregion

        #region Item Classes

        private class DatasetElementItem
        {
            public IMap Map { get; set; }
            public IDatasetElement DatasetElement { get; set; }
            public ITableClass TableClass
            {
                get
                {
                    return DatasetElement.Class as ITableClass;
                }
            }

            public override string ToString()
            {
                return Map.Name + "." + DatasetElement.Title;
            }
        }

        #endregion

        private void ValidateOk()
        {
            btnOk.Enabled =
                !String.IsNullOrEmpty(this.RelationName) &&
                cmbLeftTable.SelectedItem != null &&
                cmbRightTable.SelectedItem != null &&
                cmbLeftTableField.SelectedItem != null &&
                cmbRightTableField.SelectedItem != null;
        }

        #region Events

        private void cmbLeftTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbLeftTableField.Items.Clear();
            if (cmbLeftTable.SelectedItem == null) return;
            
            IDatasetElement element = ((DatasetElementItem)cmbLeftTable.SelectedItem).DatasetElement;

            foreach (IField field in ((ITableClass)element.Class).Fields.ToEnumerable())
            {
                cmbLeftTableField.Items.Add(field.name);
            }
            ValidateOk();
        }

        private void cmbRightTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbRightTableField.Items.Clear();
            if (cmbRightTable.SelectedItem == null) return;

            IDatasetElement element = ((DatasetElementItem)cmbRightTable.SelectedItem).DatasetElement;

            foreach (IField field in ((ITableClass)element.Class).Fields.ToEnumerable())
            {
                cmbRightTableField.Items.Add(field.name);
            }
            ValidateOk();
        }

        private void cmbLeftTableField_SelectedIndexChanged(object sender, EventArgs e)
        {
            Validate();
        }

        private void cmbRightTableField_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValidateOk();
        }

        #endregion
    }
}
