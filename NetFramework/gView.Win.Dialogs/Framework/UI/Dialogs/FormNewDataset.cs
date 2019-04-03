using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.UI.Controls;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormNewDataset : Form
    {
        public enum datasetType { FeatureDataset=0, ImageDataset=1 }

        private FormSpatialReference _sDlg;
        private bool _showSpatialIndex = false;

        public FormNewDataset()
        {
            InitializeComponent();

            _sDlg = new FormSpatialReference(null);
            _sDlg.panelReferenceSystem.Dock = DockStyle.Fill;
            tabPage2.Controls.Add(_sDlg.panelReferenceSystem);

            cmbType.SelectedIndex = 0;

            tabControl1.Invalidate();
        }

        #region Properties
        public bool ShowSpatialIndexTab
        {
            get { return _showSpatialIndex; }
            set
            {
                _showSpatialIndex = value;
                if (_showSpatialIndex)
                {
                    if (!tabControl1.TabPages.Contains(tabSpatialIndex))
                        tabControl1.TabPages.Add(tabSpatialIndex);
                }
                else
                {
                    if (tabControl1.TabPages.Contains(tabSpatialIndex))
                        tabControl1.TabPages.Remove(tabSpatialIndex);
                }
            }
        }
        #endregion

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            radioButton3.Enabled = (cmbType.SelectedIndex == 1);
            radioButton2.Enabled = radioButton1.Enabled = txtImageSpace.Enabled = btnGetImageSpace.Enabled = false;
            
            if (cmbType.SelectedIndex == 0)
            {
                if (_showSpatialIndex == false)
                    tabControl1.TabPages.Remove(tabSpatialIndex);
                tabControl1.TabPages.Remove(tabAdditionalFields);
            }
            else
            {
                if (!tabControl1.TabPages.Contains(tabSpatialIndex))
                    tabControl1.TabPages.Add(tabSpatialIndex);
                if (!tabControl1.TabPages.Contains(tabAdditionalFields))
                    tabControl1.TabPages.Add(tabAdditionalFields);
            }
        }

        private void btnGetImageSpace_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txtImageSpace.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtImageSpace.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        public string DatasetName
        {
            get { return txtName.Text; }
            set { txtName.Text = value; }
        }

        public ISpatialReference SpatialReferene
        {
            get { return _sDlg.SpatialReference; }
        }

        public datasetType DatasetType
        {
            get
            {
                return (datasetType)cmbType.SelectedIndex;
            }
        }

        public string ImageSpace
        {
            get
            {
                if (radioButton1.Checked) return "database";
                if (radioButton2.Checked) return txtImageSpace.Text;
                if (radioButton3.Checked) return "unmanaged";
                return "";
            }
        }

        public IEnvelope ImageDatasetExtent
        {
            get { return spatialIndexControl.Extent; }
        }

        public Fields AdditionalFields
        {
            get { return additionalFieldsControl1.AdditionalFields; }
        }

        public int SpatialIndexLevels
        {
            get { return spatialIndexControl.Levels; }
        }

        public ISpatialIndexDef SpatialIndexDef
        {
            get {
                if (spatialIndexControl.Type == SpatialIndexControl.IndexType.gView)
                {
                    gViewSpatialIndexDef gvIndex = new gViewSpatialIndexDef(
                        spatialIndexControl.Extent,
                        spatialIndexControl.Levels,
                        200, 0.55);
                    return gvIndex;
                }
                else if (spatialIndexControl.Type == SpatialIndexControl.IndexType.GEOGRAPHY ||
                         spatialIndexControl.Type == SpatialIndexControl.IndexType.GEOMETRY)
                {
                    return spatialIndexControl.MSIndex;
                }
                return null;
            }
        }

        public bool IndexTypeIsEditable
        {
            get { return spatialIndexControl.IndexTypeIsEditable; }
            set { spatialIndexControl.IndexTypeIsEditable = value; }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabSpatialIndex)
                spatialIndexControl.SpatialReference = this.SpatialReferene;
        }

        private void FormNewDataset_Shown(object sender, EventArgs e)
        {
            txtName.Text = "NewDataset";
            txtName.Focus();
            txtName.Select(0, txtName.Text.Length);
        }
    }
}