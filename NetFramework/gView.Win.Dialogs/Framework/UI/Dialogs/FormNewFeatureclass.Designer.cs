namespace gView.Framework.UI.Dialogs
{
    partial class FormNewFeatureclass
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNewFeatureclass));
            gView.Framework.Geometry.Envelope envelope1 = new gView.Framework.Geometry.Envelope();
            gView.Framework.Geometry.Point point1 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point2 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point3 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point4 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point5 = new gView.Framework.Geometry.Point();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeometry = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkHasZ = new System.Windows.Forms.CheckBox();
            this.cmbGeometry = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabData = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.colFieldName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFieldtype = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dsFields = new System.Data.DataSet();
            this.dataTable1 = new System.Data.DataTable();
            this.dsFieldName = new System.Data.DataColumn();
            this.dsFieldType = new System.Data.DataColumn();
            this.dsFieldField = new System.Data.DataColumn();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.pgField = new System.Windows.Forms.PropertyGrid();
            this.txtFCName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.dataGridViewComboBoxColumn1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.spatialIndexControl = new gView.Framework.UI.Controls.SpatialIndexControl();
            this.tabControl1.SuspendLayout();
            this.tabGeometry.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dsFields)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.AccessibleDescription = null;
            this.tabControl1.AccessibleName = null;
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.BackgroundImage = null;
            this.tabControl1.Controls.Add(this.tabGeometry);
            this.tabControl1.Controls.Add(this.tabData);
            this.tabControl1.Font = null;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabGeometry
            // 
            this.tabGeometry.AccessibleDescription = null;
            this.tabGeometry.AccessibleName = null;
            resources.ApplyResources(this.tabGeometry, "tabGeometry");
            this.tabGeometry.BackgroundImage = null;
            this.tabGeometry.Controls.Add(this.groupBox1);
            this.tabGeometry.Controls.Add(this.label2);
            this.tabGeometry.Controls.Add(this.label1);
            this.tabGeometry.Controls.Add(this.spatialIndexControl);
            this.tabGeometry.Font = null;
            this.tabGeometry.Name = "tabGeometry";
            this.tabGeometry.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = null;
            this.groupBox1.AccessibleName = null;
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.BackgroundImage = null;
            this.groupBox1.Controls.Add(this.chkHasZ);
            this.groupBox1.Controls.Add(this.cmbGeometry);
            this.groupBox1.Font = null;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // chkHasZ
            // 
            this.chkHasZ.AccessibleDescription = null;
            this.chkHasZ.AccessibleName = null;
            resources.ApplyResources(this.chkHasZ, "chkHasZ");
            this.chkHasZ.BackgroundImage = null;
            this.chkHasZ.Font = null;
            this.chkHasZ.Name = "chkHasZ";
            this.chkHasZ.UseVisualStyleBackColor = true;
            // 
            // cmbGeometry
            // 
            this.cmbGeometry.AccessibleDescription = null;
            this.cmbGeometry.AccessibleName = null;
            resources.ApplyResources(this.cmbGeometry, "cmbGeometry");
            this.cmbGeometry.BackgroundImage = null;
            this.cmbGeometry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGeometry.Font = null;
            this.cmbGeometry.FormattingEnabled = true;
            this.cmbGeometry.Items.AddRange(new object[] {
            resources.GetString("cmbGeometry.Items"),
            resources.GetString("cmbGeometry.Items1"),
            resources.GetString("cmbGeometry.Items2")});
            this.cmbGeometry.Name = "cmbGeometry";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label2.Name = "label2";
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label1.Name = "label1";
            // 
            // tabData
            // 
            this.tabData.AccessibleDescription = null;
            this.tabData.AccessibleName = null;
            resources.ApplyResources(this.tabData, "tabData");
            this.tabData.BackgroundImage = null;
            this.tabData.Controls.Add(this.dataGridView1);
            this.tabData.Controls.Add(this.splitter1);
            this.tabData.Controls.Add(this.pgField);
            this.tabData.Font = null;
            this.tabData.Name = "tabData";
            this.tabData.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AccessibleDescription = null;
            this.dataGridView1.AccessibleName = null;
            resources.ApplyResources(this.dataGridView1, "dataGridView1");
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.BackgroundImage = null;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colFieldName,
            this.colFieldtype});
            this.dataGridView1.DataMember = "Table1";
            this.dataGridView1.DataSource = this.dsFields;
            this.dataGridView1.Font = null;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Sunken;
            this.dataGridView1.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_RowEnter);
            this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
            this.dataGridView1.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridView1_DataError);
            // 
            // colFieldName
            // 
            this.colFieldName.DataPropertyName = "FieldName";
            resources.ApplyResources(this.colFieldName, "colFieldName");
            this.colFieldName.Name = "colFieldName";
            // 
            // colFieldtype
            // 
            this.colFieldtype.DataPropertyName = "FieldType";
            resources.ApplyResources(this.colFieldtype, "colFieldtype");
            this.colFieldtype.Name = "colFieldtype";
            this.colFieldtype.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colFieldtype.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // dsFields
            // 
            this.dsFields.DataSetName = "NewDataSet";
            this.dsFields.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTable1});
            // 
            // dataTable1
            // 
            this.dataTable1.Columns.AddRange(new System.Data.DataColumn[] {
            this.dsFieldName,
            this.dsFieldType,
            this.dsFieldField});
            this.dataTable1.TableName = "Table1";
            // 
            // dsFieldName
            // 
            this.dsFieldName.ColumnName = "FieldName";
            this.dsFieldName.DefaultValue = "";
            // 
            // dsFieldType
            // 
            this.dsFieldType.ColumnName = "FieldType";
            this.dsFieldType.DataType = typeof(gView.Framework.Data.FieldType);
            this.dsFieldType.DefaultValue = gView.Framework.Data.FieldType.String;
            // 
            // dsFieldField
            // 
            this.dsFieldField.ColumnName = "FieldField";
            this.dsFieldField.DataType = typeof(gView.Framework.Data.Field);
            // 
            // splitter1
            // 
            this.splitter1.AccessibleDescription = null;
            this.splitter1.AccessibleName = null;
            resources.ApplyResources(this.splitter1, "splitter1");
            this.splitter1.BackgroundImage = null;
            this.splitter1.Font = null;
            this.splitter1.Name = "splitter1";
            this.splitter1.TabStop = false;
            // 
            // pgField
            // 
            this.pgField.AccessibleDescription = null;
            this.pgField.AccessibleName = null;
            resources.ApplyResources(this.pgField, "pgField");
            this.pgField.BackgroundImage = null;
            this.pgField.Font = null;
            this.pgField.Name = "pgField";
            this.pgField.ToolbarVisible = false;
            // 
            // txtFCName
            // 
            this.txtFCName.AccessibleDescription = null;
            this.txtFCName.AccessibleName = null;
            resources.ApplyResources(this.txtFCName, "txtFCName");
            this.txtFCName.BackgroundImage = null;
            this.txtFCName.Font = null;
            this.txtFCName.Name = "txtFCName";
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // panel2
            // 
            this.panel2.AccessibleDescription = null;
            this.panel2.AccessibleName = null;
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.BackgroundImage = null;
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Font = null;
            this.panel2.Name = "panel2";
            // 
            // panel3
            // 
            this.panel3.AccessibleDescription = null;
            this.panel3.AccessibleName = null;
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.BackgroundImage = null;
            this.panel3.Controls.Add(this.btnOK);
            this.panel3.Font = null;
            this.panel3.Name = "panel3";
            // 
            // btnOK
            // 
            this.btnOK.AccessibleDescription = null;
            this.btnOK.AccessibleName = null;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.BackgroundImage = null;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Font = null;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleDescription = null;
            this.btnCancel.AccessibleName = null;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.BackgroundImage = null;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = null;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // dataGridViewComboBoxColumn1
            // 
            this.dataGridViewComboBoxColumn1.DataPropertyName = "FieldType";
            resources.ApplyResources(this.dataGridViewComboBoxColumn1, "dataGridViewComboBoxColumn1");
            this.dataGridViewComboBoxColumn1.Name = "dataGridViewComboBoxColumn1";
            this.dataGridViewComboBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewComboBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // panel1
            // 
            this.panel1.AccessibleDescription = null;
            this.panel1.AccessibleName = null;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackgroundImage = null;
            this.panel1.Controls.Add(this.txtFCName);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Font = null;
            this.panel1.Name = "panel1";
            // 
            // spatialIndexControl
            // 
            this.spatialIndexControl.AccessibleDescription = null;
            this.spatialIndexControl.AccessibleName = null;
            resources.ApplyResources(this.spatialIndexControl, "spatialIndexControl");
            this.spatialIndexControl.BackgroundImage = null;
            point1.M = 0;
            point1.X = 0;
            point1.Y = 0;
            point1.Z = 0;
            envelope1.Center = point1;
            point2.M = 0;
            point2.X = 0;
            point2.Y = 0;
            point2.Z = 0;
            envelope1.LowerLeft = point2;
            point3.M = 0;
            point3.X = 0;
            point3.Y = 0;
            point3.Z = 0;
            envelope1.LowerRight = point3;
            envelope1.maxx = 0;
            envelope1.maxy = 0;
            envelope1.minx = 0;
            envelope1.miny = 0;
            point4.M = 0;
            point4.X = 0;
            point4.Y = 0;
            point4.Z = 0;
            envelope1.UpperLeft = point4;
            point5.M = 0;
            point5.X = 0;
            point5.Y = 0;
            point5.Z = 0;
            envelope1.UpperRight = point5;
            this.spatialIndexControl.Extent = envelope1;
            this.spatialIndexControl.Font = null;
            this.spatialIndexControl.Levels = 0;
            this.spatialIndexControl.Name = "spatialIndexControl";
            // 
            // FormNewFeatureclass
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormNewFeatureclass";
            this.Shown += new System.EventHandler(this.FormNewFeatureclass_Shown);
            this.tabControl1.ResumeLayout(false);
            this.tabGeometry.ResumeLayout(false);
            this.tabGeometry.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dsFields)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeometry;
        private System.Windows.Forms.TabPage tabData;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private gView.Framework.UI.Controls.SpatialIndexControl spatialIndexControl;
        private System.Windows.Forms.CheckBox chkHasZ;
        private System.Windows.Forms.ComboBox cmbGeometry;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtFCName;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.PropertyGrid pgField;
        private System.Data.DataSet dsFields;
        private System.Data.DataTable dataTable1;
        private System.Data.DataColumn dsFieldName;
        private System.Data.DataColumn dsFieldType;
        private System.Data.DataColumn dsFieldField;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFieldName;
        private System.Windows.Forms.DataGridViewComboBoxColumn colFieldtype;
    }
}