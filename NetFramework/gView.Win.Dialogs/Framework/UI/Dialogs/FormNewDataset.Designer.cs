namespace gView.Framework.UI.Dialogs
{
    partial class FormNewDataset
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNewDataset));
            gView.Framework.Geometry.Envelope envelope3 = new gView.Framework.Geometry.Envelope();
            gView.Framework.Geometry.Point point11 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point12 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point13 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point14 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point15 = new gView.Framework.Geometry.Point();
            gView.Framework.Data.gViewSpatialIndexDef gViewSpatialIndexDef2 = new gView.Framework.Data.gViewSpatialIndexDef();
            gView.Framework.Geometry.Envelope envelope4 = new gView.Framework.Geometry.Envelope();
            gView.Framework.Geometry.Point point16 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point17 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point18 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point19 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point20 = new gView.Framework.Geometry.Point();
            this.btnGetImageSpace = new System.Windows.Forms.Button();
            this.txtImageSpace = new System.Windows.Forms.TextBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.tabSpatialIndex = new System.Windows.Forms.TabPage();
            this.spatialIndexControl = new gView.Framework.UI.Controls.SpatialIndexControl();
            this.tabAdditionalFields = new System.Windows.Forms.TabPage();
            this.additionalFieldsControl1 = new gView.Framework.UI.Controls.AdditionalFieldsControl();
            this.panel4 = new System.Windows.Forms.Panel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabSpatialIndex.SuspendLayout();
            this.tabAdditionalFields.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGetImageSpace
            // 
            this.btnGetImageSpace.AccessibleDescription = null;
            this.btnGetImageSpace.AccessibleName = null;
            resources.ApplyResources(this.btnGetImageSpace, "btnGetImageSpace");
            this.btnGetImageSpace.BackgroundImage = null;
            this.btnGetImageSpace.Font = null;
            this.btnGetImageSpace.Name = "btnGetImageSpace";
            this.btnGetImageSpace.UseVisualStyleBackColor = true;
            this.btnGetImageSpace.Click += new System.EventHandler(this.btnGetImageSpace_Click);
            // 
            // txtImageSpace
            // 
            this.txtImageSpace.AccessibleDescription = null;
            this.txtImageSpace.AccessibleName = null;
            resources.ApplyResources(this.txtImageSpace, "txtImageSpace");
            this.txtImageSpace.BackgroundImage = null;
            this.txtImageSpace.Font = null;
            this.txtImageSpace.Name = "txtImageSpace";
            // 
            // radioButton2
            // 
            this.radioButton2.AccessibleDescription = null;
            this.radioButton2.AccessibleName = null;
            resources.ApplyResources(this.radioButton2, "radioButton2");
            this.radioButton2.BackgroundImage = null;
            this.radioButton2.Font = null;
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AccessibleDescription = null;
            this.radioButton1.AccessibleName = null;
            resources.ApplyResources(this.radioButton1, "radioButton1");
            this.radioButton1.BackgroundImage = null;
            this.radioButton1.Font = null;
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // txtName
            // 
            this.txtName.AccessibleDescription = null;
            this.txtName.AccessibleName = null;
            resources.ApplyResources(this.txtName, "txtName");
            this.txtName.BackgroundImage = null;
            this.txtName.Font = null;
            this.txtName.Name = "txtName";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // cmbType
            // 
            this.cmbType.AccessibleDescription = null;
            this.cmbType.AccessibleName = null;
            resources.ApplyResources(this.cmbType, "cmbType");
            this.cmbType.BackgroundImage = null;
            this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbType.Font = null;
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            resources.GetString("cmbType.Items"),
            resources.GetString("cmbType.Items1")});
            this.cmbType.Name = "cmbType";
            this.cmbType.SelectedIndexChanged += new System.EventHandler(this.cmbType_SelectedIndexChanged);
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
            // tabControl1
            // 
            this.tabControl1.AccessibleDescription = null;
            this.tabControl1.AccessibleName = null;
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.BackgroundImage = null;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabSpatialIndex);
            this.tabControl1.Controls.Add(this.tabAdditionalFields);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Font = null;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.AccessibleDescription = null;
            this.tabPage1.AccessibleName = null;
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.BackgroundImage = null;
            this.tabPage1.Controls.Add(this.radioButton3);
            this.tabPage1.Controls.Add(this.btnGetImageSpace);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.txtImageSpace);
            this.tabPage1.Controls.Add(this.radioButton1);
            this.tabPage1.Controls.Add(this.cmbType);
            this.tabPage1.Controls.Add(this.radioButton2);
            this.tabPage1.Font = null;
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AccessibleDescription = null;
            this.radioButton3.AccessibleName = null;
            resources.ApplyResources(this.radioButton3, "radioButton3");
            this.radioButton3.BackgroundImage = null;
            this.radioButton3.Checked = true;
            this.radioButton3.Font = null;
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.TabStop = true;
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // tabSpatialIndex
            // 
            this.tabSpatialIndex.AccessibleDescription = null;
            this.tabSpatialIndex.AccessibleName = null;
            resources.ApplyResources(this.tabSpatialIndex, "tabSpatialIndex");
            this.tabSpatialIndex.BackgroundImage = null;
            this.tabSpatialIndex.Controls.Add(this.spatialIndexControl);
            this.tabSpatialIndex.Font = null;
            this.tabSpatialIndex.Name = "tabSpatialIndex";
            this.tabSpatialIndex.UseVisualStyleBackColor = true;
            // 
            // spatialIndexControl
            // 
            this.spatialIndexControl.AccessibleDescription = null;
            this.spatialIndexControl.AccessibleName = null;
            resources.ApplyResources(this.spatialIndexControl, "spatialIndexControl");
            this.spatialIndexControl.BackgroundImage = null;
            point11.M = 0;
            point11.X = 0;
            point11.Y = 0;
            point11.Z = 0;
            envelope3.Center = point11;
            point12.M = 0;
            point12.X = 0;
            point12.Y = 0;
            point12.Z = 0;
            envelope3.LowerLeft = point12;
            point13.M = 0;
            point13.X = 0;
            point13.Y = 0;
            point13.Z = 0;
            envelope3.LowerRight = point13;
            envelope3.maxx = 0;
            envelope3.maxy = 0;
            envelope3.minx = 0;
            envelope3.miny = 0;
            point14.M = 0;
            point14.X = 0;
            point14.Y = 0;
            point14.Z = 0;
            envelope3.UpperLeft = point14;
            point15.M = 0;
            point15.X = 0;
            point15.Y = 0;
            point15.Z = 0;
            envelope3.UpperRight = point15;
            this.spatialIndexControl.Extent = envelope3;
            this.spatialIndexControl.Font = null;
            this.spatialIndexControl.IndexTypeIsEditable = true;
            this.spatialIndexControl.Levels = 0;
            this.spatialIndexControl.MSIndex = null;
            this.spatialIndexControl.Name = "spatialIndexControl";
            gViewSpatialIndexDef2.Levels = 0;
            gViewSpatialIndexDef2.MaxPerNode = 200;
            point16.M = 0;
            point16.X = 0;
            point16.Y = 0;
            point16.Z = 0;
            envelope4.Center = point16;
            point17.M = 0;
            point17.X = 0;
            point17.Y = 0;
            point17.Z = 0;
            envelope4.LowerLeft = point17;
            point18.M = 0;
            point18.X = 0;
            point18.Y = 0;
            point18.Z = 0;
            envelope4.LowerRight = point18;
            envelope4.maxx = 0;
            envelope4.maxy = 0;
            envelope4.minx = 0;
            envelope4.miny = 0;
            point19.M = 0;
            point19.X = 0;
            point19.Y = 0;
            point19.Z = 0;
            envelope4.UpperLeft = point19;
            point20.M = 0;
            point20.X = 0;
            point20.Y = 0;
            point20.Z = 0;
            envelope4.UpperRight = point20;
            gViewSpatialIndexDef2.SpatialIndexBounds = envelope4;
            gViewSpatialIndexDef2.SpatialReference = null;
            gViewSpatialIndexDef2.SplitRatio = 0.55;
            this.spatialIndexControl.SpatialIndexDef = gViewSpatialIndexDef2;
            this.spatialIndexControl.Type = gView.Framework.UI.Controls.SpatialIndexControl.IndexType.gView;
            // 
            // tabAdditionalFields
            // 
            this.tabAdditionalFields.AccessibleDescription = null;
            this.tabAdditionalFields.AccessibleName = null;
            resources.ApplyResources(this.tabAdditionalFields, "tabAdditionalFields");
            this.tabAdditionalFields.BackgroundImage = null;
            this.tabAdditionalFields.Controls.Add(this.additionalFieldsControl1);
            this.tabAdditionalFields.Controls.Add(this.panel4);
            this.tabAdditionalFields.Font = null;
            this.tabAdditionalFields.Name = "tabAdditionalFields";
            this.tabAdditionalFields.UseVisualStyleBackColor = true;
            // 
            // additionalFieldsControl1
            // 
            this.additionalFieldsControl1.AccessibleDescription = null;
            this.additionalFieldsControl1.AccessibleName = null;
            resources.ApplyResources(this.additionalFieldsControl1, "additionalFieldsControl1");
            this.additionalFieldsControl1.BackgroundImage = null;
            this.additionalFieldsControl1.Font = null;
            this.additionalFieldsControl1.Name = "additionalFieldsControl1";
            // 
            // panel4
            // 
            this.panel4.AccessibleDescription = null;
            this.panel4.AccessibleName = null;
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.BackgroundImage = null;
            this.panel4.Font = null;
            this.panel4.Name = "panel4";
            // 
            // tabPage2
            // 
            this.tabPage2.AccessibleDescription = null;
            this.tabPage2.AccessibleName = null;
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.BackgroundImage = null;
            this.tabPage2.Font = null;
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // folderBrowserDialog1
            // 
            resources.ApplyResources(this.folderBrowserDialog1, "folderBrowserDialog1");
            // 
            // panel1
            // 
            this.panel1.AccessibleDescription = null;
            this.panel1.AccessibleName = null;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackgroundImage = null;
            this.panel1.Controls.Add(this.txtName);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Font = null;
            this.panel1.Name = "panel1";
            // 
            // FormNewDataset
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
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = null;
            this.Name = "FormNewDataset";
            this.Shown += new System.EventHandler(this.FormNewDataset_Shown);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabSpatialIndex.ResumeLayout(false);
            this.tabAdditionalFields.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnGetImageSpace;
        private System.Windows.Forms.TextBox txtImageSpace;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.TabPage tabSpatialIndex;
        private gView.Framework.UI.Controls.SpatialIndexControl spatialIndexControl;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabPage tabAdditionalFields;
        private gView.Framework.UI.Controls.AdditionalFieldsControl additionalFieldsControl1;
        private System.Windows.Forms.Panel panel4;
    }
}