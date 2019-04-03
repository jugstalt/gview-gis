using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.UI.Controls;
using gView.Framework.FDB;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormNewFeatureclass : Form
    {
        //private List<IField> _fields = new List<IField>();

        public FormNewFeatureclass(IFeatureClass fc)
        {
            InitializeComponent();

            tabControl1.TabPages.Remove(tabGeometry);
            if (fc == null) return;

            foreach (FieldType fieldType in Enum.GetValues(typeof(FieldType)))
            {
                if (fieldType == FieldType.unknown ||
                    fieldType == FieldType.ID ||
                    fieldType == FieldType.Shape) continue;

                colFieldtype.Items.Add(fieldType);
            }

            this.Fields = fc.Fields;

            tabControl1.Invalidate();

            this.IndexTypeIsEditable = false;
            if (fc.Dataset is IFDBDataset)
                this.SpatialIndexDef = ((IFDBDataset)fc.Dataset).SpatialIndexDef;
        }
        public FormNewFeatureclass(IFeatureDataset dataset)
        {
            InitializeComponent();

            if (dataset == null) return;

            cmbGeometry.SelectedIndex = 0;
            spatialIndexControl.SpatialReference = dataset.SpatialReference;

            foreach (FieldType fieldType in Enum.GetValues(typeof(FieldType)))
            {
                if (fieldType == FieldType.unknown ||
                    fieldType == FieldType.ID ||
                    fieldType == FieldType.Shape) continue;

                colFieldtype.Items.Add(fieldType);
            }

            tabControl1.Invalidate();

            this.IndexTypeIsEditable = false;
            if (dataset is IFDBDataset)
                this.SpatialIndexDef = ((IFDBDataset)dataset).SpatialIndexDef;
        }

        #region Properties
        public string FeatureclassName
        {
            get { return txtFCName.Text; }
            set { txtFCName.Text = value; }
        }

        public IGeometryDef GeometryDef
        {
            get
            {
                geometryType gType = geometryType.Unknown;
                switch (cmbGeometry.SelectedIndex)
                {
                    case 0:
                        gType = geometryType.Point;
                        break;
                    case 1:
                        gType = geometryType.Polyline;
                        break;
                    case 2:
                        gType = geometryType.Polygon;
                        break;
                }

                GeometryDef geomDef = new GeometryDef(gType, null, chkHasZ.Checked);
                
                return geomDef;
            }
        }

        public IEnvelope SpatialIndexExtents
        {
            get
            {
                return spatialIndexControl.Extent;
            }
            set
            {
                spatialIndexControl.Extent = value;
            }
        }
        public int SpatialIndexLevels
        {
            get
            {
                return spatialIndexControl.Levels;
            }
            set
            {
                spatialIndexControl.Levels = value;
            }
        }

        public MSSpatialIndex MSSpatialIndexDef
        {
            get { return spatialIndexControl.MSIndex; }
            set { spatialIndexControl.MSIndex = value; }

        }
        public IFields Fields
        {
            get
            {
                Fields fields = new Fields();
                foreach (DataRow row in dsFields.Tables[0].Rows)
                {
                    if (row["FieldField"] is IField)
                        fields.Add(row["FieldField"] as IField);
                }
                return fields;
            }
            set
            {
                if (value == null) return;

                foreach (IField field in value.ToEnumerable())
                {
                    if (field.type == FieldType.ID ||
                        field.type == FieldType.Shape) continue;

                    Field f = new Field(field);

                    DataRow row = dsFields.Tables[0].NewRow();
                    row["FieldName"] = f.name;
                    row["FieldType"] = f.type;
                    row["FieldField"] = f;
                    dsFields.Tables[0].Rows.Add(row);
                }
            }
        }

        public ISpatialIndexDef SpatialIndexDef
        {
            get { return spatialIndexControl.SpatialIndexDef; }
            set { spatialIndexControl.SpatialIndexDef = value; }
        }
        public bool IndexTypeIsEditable
        {
            get { return spatialIndexControl.IndexTypeIsEditable; }
            set { spatialIndexControl.IndexTypeIsEditable = value; }
        }
        #endregion

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            //if (e.ColumnIndex == 1)
            //    dataGridView1[e.ColumnIndex, e.RowIndex].Value = FieldType.String;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            SetFieldProperties(e.RowIndex);
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            SetFieldProperties(e.RowIndex);
        }

        private void SetFieldProperties(int index)
        {
            if (index >= dsFields.Tables[0].Rows.Count) return;

            DataRow row = dsFields.Tables[0].Rows[index];

            if (row["FieldField"] == null || row["FieldField"] == DBNull.Value)
                row["FieldField"] = new Field();

            ((Field)row["FieldField"]).name = (string)dataGridView1[0, index].Value;
            ((Field)row["FieldField"]).type = (FieldType)dataGridView1[1, index].Value;

            if (((Field)row["FieldField"]).type == FieldType.String &&
                ((Field)row["FieldField"]).size <= 0)
                ((Field)row["FieldField"]).size = 100;

            pgField.SelectedObject =
                ((Field)row["FieldField"]).type == FieldType.String ?
                new StringFieldPropertyObject((Field)row["FieldField"]) :
                new FieldPropertyObject((Field)row["FieldField"]);
        }

        private void FormNewFeatureclass_Shown(object sender, EventArgs e)
        {
            txtFCName.Text = "NewFeatureclass";
            txtFCName.Focus();
            txtFCName.Select(0, txtFCName.Text.Length);
        }
    }

    internal class FieldPropertyObject
    {
        protected Field _field;

        public FieldPropertyObject(Field field)
        {
            _field = field;
        }

        public string Aliasname
        {
            get { return _field.aliasname; }
            set { _field.aliasname = value; }
        }
    }

    internal class StringFieldPropertyObject : FieldPropertyObject
    {
        public StringFieldPropertyObject(Field field)
            : base(field)
        {
        }

        public int Size
        {
            get { return _field.size; }
            set { _field.size = value; }
        }
    }
}