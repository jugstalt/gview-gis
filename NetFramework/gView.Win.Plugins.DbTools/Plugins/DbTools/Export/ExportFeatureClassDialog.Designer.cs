namespace gView.Plugins.DbTools.Export
{
    partial class ExportFeatureClassDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportFeatureClassDialog));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.cmbExport = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.txtTargetClass = new System.Windows.Forms.TextBox();
            this.txtDatasetLocation = new System.Windows.Forms.TextBox();
            this.txtDatasetName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.gvFields = new System.Windows.Forms.DataGridView();
            this.panelStep1 = new System.Windows.Forms.Panel();
            this.panelStep2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnBack = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvFields)).BeginInit();
            this.panelStep1.SuspendLayout();
            this.panelStep2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
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
            // btnNext
            // 
            this.btnNext.AccessibleDescription = null;
            this.btnNext.AccessibleName = null;
            resources.ApplyResources(this.btnNext, "btnNext");
            this.btnNext.BackgroundImage = null;
            this.btnNext.Font = null;
            this.btnNext.Name = "btnNext";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // cmbExport
            // 
            this.cmbExport.AccessibleDescription = null;
            this.cmbExport.AccessibleName = null;
            resources.ApplyResources(this.cmbExport, "cmbExport");
            this.cmbExport.BackgroundImage = null;
            this.cmbExport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExport.Font = null;
            this.cmbExport.FormattingEnabled = true;
            this.cmbExport.Name = "cmbExport";
            this.cmbExport.SelectedIndexChanged += new System.EventHandler(this.cmbExport_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = null;
            this.groupBox1.AccessibleName = null;
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.BackgroundImage = null;
            this.groupBox1.Controls.Add(this.cmbExport);
            this.groupBox1.Font = null;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.AccessibleDescription = null;
            this.groupBox2.AccessibleName = null;
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.BackgroundImage = null;
            this.groupBox2.Controls.Add(this.btnSelect);
            this.groupBox2.Controls.Add(this.txtTargetClass);
            this.groupBox2.Controls.Add(this.txtDatasetLocation);
            this.groupBox2.Controls.Add(this.txtDatasetName);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Font = null;
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // btnSelect
            // 
            this.btnSelect.AccessibleDescription = null;
            this.btnSelect.AccessibleName = null;
            resources.ApplyResources(this.btnSelect, "btnSelect");
            this.btnSelect.BackgroundImage = null;
            this.btnSelect.Font = null;
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // txtTargetClass
            // 
            this.txtTargetClass.AccessibleDescription = null;
            this.txtTargetClass.AccessibleName = null;
            resources.ApplyResources(this.txtTargetClass, "txtTargetClass");
            this.txtTargetClass.BackgroundImage = null;
            this.txtTargetClass.Font = null;
            this.txtTargetClass.Name = "txtTargetClass";
            // 
            // txtDatasetLocation
            // 
            this.txtDatasetLocation.AccessibleDescription = null;
            this.txtDatasetLocation.AccessibleName = null;
            resources.ApplyResources(this.txtDatasetLocation, "txtDatasetLocation");
            this.txtDatasetLocation.BackgroundImage = null;
            this.txtDatasetLocation.Font = null;
            this.txtDatasetLocation.Name = "txtDatasetLocation";
            // 
            // txtDatasetName
            // 
            this.txtDatasetName.AccessibleDescription = null;
            this.txtDatasetName.AccessibleName = null;
            resources.ApplyResources(this.txtDatasetName, "txtDatasetName");
            this.txtDatasetName.BackgroundImage = null;
            this.txtDatasetName.Font = null;
            this.txtDatasetName.Name = "txtDatasetName";
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Font = null;
            this.label3.Name = "label3";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Font = null;
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
            // groupBox3
            // 
            this.groupBox3.AccessibleDescription = null;
            this.groupBox3.AccessibleName = null;
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.BackgroundImage = null;
            this.groupBox3.Controls.Add(this.gvFields);
            this.groupBox3.Font = null;
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // gvFields
            // 
            this.gvFields.AccessibleDescription = null;
            this.gvFields.AccessibleName = null;
            this.gvFields.AllowUserToAddRows = false;
            this.gvFields.AllowUserToDeleteRows = false;
            this.gvFields.AllowUserToResizeRows = false;
            resources.ApplyResources(this.gvFields, "gvFields");
            this.gvFields.BackgroundImage = null;
            this.gvFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvFields.Font = null;
            this.gvFields.Name = "gvFields";
            this.gvFields.ShowEditingIcon = false;
            // 
            // panelStep1
            // 
            this.panelStep1.AccessibleDescription = null;
            this.panelStep1.AccessibleName = null;
            resources.ApplyResources(this.panelStep1, "panelStep1");
            this.panelStep1.BackgroundImage = null;
            this.panelStep1.Controls.Add(this.groupBox1);
            this.panelStep1.Controls.Add(this.groupBox2);
            this.panelStep1.Font = null;
            this.panelStep1.Name = "panelStep1";
            // 
            // panelStep2
            // 
            this.panelStep2.AccessibleDescription = null;
            this.panelStep2.AccessibleName = null;
            resources.ApplyResources(this.panelStep2, "panelStep2");
            this.panelStep2.BackgroundImage = null;
            this.panelStep2.Controls.Add(this.groupBox3);
            this.panelStep2.Font = null;
            this.panelStep2.Name = "panelStep2";
            // 
            // panel3
            // 
            this.panel3.AccessibleDescription = null;
            this.panel3.AccessibleName = null;
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.BackgroundImage = null;
            this.panel3.Font = null;
            this.panel3.Name = "panel3";
            // 
            // panel4
            // 
            this.panel4.AccessibleDescription = null;
            this.panel4.AccessibleName = null;
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.BackgroundImage = null;
            this.panel4.Controls.Add(this.btnBack);
            this.panel4.Controls.Add(this.btnNext);
            this.panel4.Controls.Add(this.btnCancel);
            this.panel4.Font = null;
            this.panel4.Name = "panel4";
            // 
            // btnBack
            // 
            this.btnBack.AccessibleDescription = null;
            this.btnBack.AccessibleName = null;
            resources.ApplyResources(this.btnBack, "btnBack");
            this.btnBack.BackgroundImage = null;
            this.btnBack.Font = null;
            this.btnBack.Name = "btnBack";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // ExportFeatureClassDialog
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.panelStep2);
            this.Controls.Add(this.panelStep1);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "ExportFeatureClassDialog";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvFields)).EndInit();
            this.panelStep1.ResumeLayout(false);
            this.panelStep2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.ComboBox cmbExport;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.TextBox txtTargetClass;
        private System.Windows.Forms.TextBox txtDatasetLocation;
        private System.Windows.Forms.TextBox txtDatasetName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DataGridView gvFields;
        private System.Windows.Forms.Panel panelStep1;
        private System.Windows.Forms.Panel panelStep2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnBack;
    }
}