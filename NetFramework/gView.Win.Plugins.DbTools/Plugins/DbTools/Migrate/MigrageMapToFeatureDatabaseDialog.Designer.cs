namespace gView.Plugins.DbTools.Migrate
{
    partial class MigrateMapToFeatureDatabaseDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MigrateMapToFeatureDatabaseDialog));
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbExport = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.txtDatasetLocation = new System.Windows.Forms.TextBox();
            this.txtDatasetName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtMap = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbExport);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // cmbExport
            // 
            this.cmbExport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExport.FormattingEnabled = true;
            resources.ApplyResources(this.cmbExport, "cmbExport");
            this.cmbExport.Name = "cmbExport";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSelect);
            this.groupBox2.Controls.Add(this.txtDatasetLocation);
            this.groupBox2.Controls.Add(this.txtDatasetName);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // btnSelect
            // 
            resources.ApplyResources(this.btnSelect, "btnSelect");
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // txtDatasetLocation
            // 
            resources.ApplyResources(this.txtDatasetLocation, "txtDatasetLocation");
            this.txtDatasetLocation.Name = "txtDatasetLocation";
            // 
            // txtDatasetName
            // 
            resources.ApplyResources(this.txtDatasetName, "txtDatasetName");
            this.txtDatasetName.Name = "txtDatasetName";
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
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtMap);
            this.groupBox3.Controls.Add(this.label3);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // txtMap
            // 
            resources.ApplyResources(this.txtMap, "txtMap");
            this.txtMap.Name = "txtMap";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnStart);
            this.panel4.Controls.Add(this.btnCancel);
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // btnStart
            // 
            this.btnStart.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.btnStart, "btnStart");
            this.btnStart.Name = "btnStart";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // MigrateMapToFeatureDatabaseDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MigrateMapToFeatureDatabaseDialog";
            this.Shown += new System.EventHandler(this.MigrateMapToFeatureDatabaseDialog_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbExport;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.TextBox txtDatasetLocation;
        private System.Windows.Forms.TextBox txtDatasetName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtMap;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnCancel;

    }
}