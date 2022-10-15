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
            gView.Framework.Geometry.Envelope envelope5 = new gView.Framework.Geometry.Envelope();
            gView.Framework.Geometry.Point point21 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point22 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point23 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point24 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point25 = new gView.Framework.Geometry.Point();
            gView.Framework.Data.gViewSpatialIndexDef gViewSpatialIndexDef3 = new gView.Framework.Data.gViewSpatialIndexDef();
            gView.Framework.Geometry.Envelope envelope6 = new gView.Framework.Geometry.Envelope();
            gView.Framework.Geometry.Point point26 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point27 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point28 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point29 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point30 = new gView.Framework.Geometry.Point();
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
            this.tabAdditionalFields = new System.Windows.Forms.TabPage();
            this.panel4 = new System.Windows.Forms.Panel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.spatialIndexControl = new gView.Framework.UI.Controls.SpatialIndexControl();
            this.additionalFieldsControl1 = new gView.Framework.UI.Controls.AdditionalFieldsControl();
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
            resources.ApplyResources(this.btnGetImageSpace, "btnGetImageSpace");
            this.btnGetImageSpace.Name = "btnGetImageSpace";
            this.btnGetImageSpace.UseVisualStyleBackColor = true;
            this.btnGetImageSpace.Click += new System.EventHandler(this.btnGetImageSpace_Click);
            // 
            // txtImageSpace
            // 
            resources.ApplyResources(this.txtImageSpace, "txtImageSpace");
            this.txtImageSpace.Name = "txtImageSpace";
            // 
            // radioButton2
            // 
            resources.ApplyResources(this.radioButton2, "radioButton2");
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            resources.ApplyResources(this.radioButton1, "radioButton1");
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // txtName
            // 
            resources.ApplyResources(this.txtName, "txtName");
            this.txtName.Name = "txtName";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cmbType
            // 
            this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            resources.GetString("cmbType.Items"),
            resources.GetString("cmbType.Items1")});
            resources.ApplyResources(this.cmbType, "cmbType");
            this.cmbType.Name = "cmbType";
            this.cmbType.SelectedIndexChanged += new System.EventHandler(this.cmbType_SelectedIndexChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.btnCancel);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnOK);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabSpatialIndex);
            this.tabControl1.Controls.Add(this.tabAdditionalFields);
            this.tabControl1.Controls.Add(this.tabPage2);
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.radioButton3);
            this.tabPage1.Controls.Add(this.btnGetImageSpace);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.txtImageSpace);
            this.tabPage1.Controls.Add(this.radioButton1);
            this.tabPage1.Controls.Add(this.cmbType);
            this.tabPage1.Controls.Add(this.radioButton2);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            resources.ApplyResources(this.radioButton3, "radioButton3");
            this.radioButton3.Checked = true;
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.TabStop = true;
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // tabSpatialIndex
            // 
            this.tabSpatialIndex.Controls.Add(this.spatialIndexControl);
            resources.ApplyResources(this.tabSpatialIndex, "tabSpatialIndex");
            this.tabSpatialIndex.Name = "tabSpatialIndex";
            this.tabSpatialIndex.UseVisualStyleBackColor = true;
            // 
            // tabAdditionalFields
            // 
            this.tabAdditionalFields.Controls.Add(this.additionalFieldsControl1);
            this.tabAdditionalFields.Controls.Add(this.panel4);
            resources.ApplyResources(this.tabAdditionalFields, "tabAdditionalFields");
            this.tabAdditionalFields.Name = "tabAdditionalFields";
            this.tabAdditionalFields.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // tabPage2
            // 
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtName);
            this.panel1.Controls.Add(this.label2);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // spatialIndexControl
            // 
            point21.M = 0D;
            point21.Srs = null;
            point21.X = 0D;
            point21.Y = 0D;
            point21.Z = 0D;
            envelope5.Center = point21;
            point22.M = 0D;
            point22.Srs = null;
            point22.X = 0D;
            point22.Y = 0D;
            point22.Z = 0D;
            envelope5.LowerLeft = point22;
            point23.M = 0D;
            point23.Srs = null;
            point23.X = 0D;
            point23.Y = 0D;
            point23.Z = 0D;
            envelope5.LowerRight = point23;
            envelope5.maxx = 0D;
            envelope5.maxy = 0D;
            envelope5.minx = 0D;
            envelope5.miny = 0D;
            envelope5.Srs = null;
            point24.M = 0D;
            point24.Srs = null;
            point24.X = 0D;
            point24.Y = 0D;
            point24.Z = 0D;
            envelope5.UpperLeft = point24;
            point25.M = 0D;
            point25.Srs = null;
            point25.X = 0D;
            point25.Y = 0D;
            point25.Z = 0D;
            envelope5.UpperRight = point25;
            this.spatialIndexControl.Extent = envelope5;
            this.spatialIndexControl.IndexTypeIsEditable = true;
            this.spatialIndexControl.Levels = 0;
            resources.ApplyResources(this.spatialIndexControl, "spatialIndexControl");
            this.spatialIndexControl.MSIndex = null;
            this.spatialIndexControl.Name = "spatialIndexControl";
            gViewSpatialIndexDef3.Levels = 0;
            gViewSpatialIndexDef3.MaxPerNode = 200;
            point26.M = 0D;
            point26.Srs = null;
            point26.X = 0D;
            point26.Y = 0D;
            point26.Z = 0D;
            envelope6.Center = point26;
            point27.M = 0D;
            point27.Srs = null;
            point27.X = 0D;
            point27.Y = 0D;
            point27.Z = 0D;
            envelope6.LowerLeft = point27;
            point28.M = 0D;
            point28.Srs = null;
            point28.X = 0D;
            point28.Y = 0D;
            point28.Z = 0D;
            envelope6.LowerRight = point28;
            envelope6.maxx = 0D;
            envelope6.maxy = 0D;
            envelope6.minx = 0D;
            envelope6.miny = 0D;
            envelope6.Srs = null;
            point29.M = 0D;
            point29.Srs = null;
            point29.X = 0D;
            point29.Y = 0D;
            point29.Z = 0D;
            envelope6.UpperLeft = point29;
            point30.M = 0D;
            point30.Srs = null;
            point30.X = 0D;
            point30.Y = 0D;
            point30.Z = 0D;
            envelope6.UpperRight = point30;
            gViewSpatialIndexDef3.SpatialIndexBounds = envelope6;
            gViewSpatialIndexDef3.SpatialReference = null;
            gViewSpatialIndexDef3.SplitRatio = 0.55D;
            this.spatialIndexControl.SpatialIndexDef = gViewSpatialIndexDef3;
            this.spatialIndexControl.Type = gView.Framework.UI.Controls.SpatialIndexControl.IndexType.gView;
            // 
            // additionalFieldsControl1
            // 
            resources.ApplyResources(this.additionalFieldsControl1, "additionalFieldsControl1");
            this.additionalFieldsControl1.Name = "additionalFieldsControl1";
            // 
            // FormNewDataset
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
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