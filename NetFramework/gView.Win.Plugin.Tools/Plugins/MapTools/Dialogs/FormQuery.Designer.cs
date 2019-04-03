namespace gView.Plugins.MapTools.Dialogs
{
    partial class FormQuery
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormQuery));
            this.progressDisk = new gView.Framework.UI.Controls.ProgressDisk();
            this.btnQuery = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblMsg2 = new System.Windows.Forms.Label();
            this.lblMsg1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnStop = new System.Windows.Forms.Button();
            this.panelStandard = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbField = new System.Windows.Forms.ComboBox();
            this.btnDisplayField = new System.Windows.Forms.RadioButton();
            this.btnField = new System.Windows.Forms.RadioButton();
            this.btnAllFields = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.chkWildcards = new System.Windows.Forms.CheckBox();
            this.cmbQueryText = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panelCustom = new System.Windows.Forms.Panel();
            this.panelCustomFields = new System.Windows.Forms.Panel();
            this.lblQueryName = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panelStandard.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panelCustom.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressDisk
            // 
            this.progressDisk.BlockSize = gView.Framework.UI.Controls.BlockSize.Large;
            resources.ApplyResources(this.progressDisk, "progressDisk");
            this.progressDisk.Name = "progressDisk";
            this.progressDisk.SquareSize = 100;
            // 
            // btnQuery
            // 
            resources.ApplyResources(this.btnQuery, "btnQuery");
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblMsg2);
            this.panel1.Controls.Add(this.lblMsg1);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.progressDisk);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // lblMsg2
            // 
            resources.ApplyResources(this.lblMsg2, "lblMsg2");
            this.lblMsg2.Name = "lblMsg2";
            // 
            // lblMsg1
            // 
            resources.ApplyResources(this.lblMsg1, "lblMsg1");
            this.lblMsg1.Name = "lblMsg1";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnStop);
            this.panel2.Controls.Add(this.btnQuery);
            this.panel2.Controls.Add(this.btnCancel);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // btnStop
            // 
            resources.ApplyResources(this.btnStop, "btnStop");
            this.btnStop.Name = "btnStop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // panelStandard
            // 
            this.panelStandard.Controls.Add(this.groupBox1);
            this.panelStandard.Controls.Add(this.panel5);
            this.panelStandard.Controls.Add(this.panel4);
            this.panelStandard.Controls.Add(this.panel3);
            resources.ApplyResources(this.panelStandard, "panelStandard");
            this.panelStandard.Name = "panelStandard";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbField);
            this.groupBox1.Controls.Add(this.btnDisplayField);
            this.groupBox1.Controls.Add(this.btnField);
            this.groupBox1.Controls.Add(this.btnAllFields);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.chkWildcards);
            this.groupBox1.Controls.Add(this.cmbQueryText);
            this.groupBox1.Controls.Add(this.label1);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // cmbField
            // 
            this.cmbField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbField, "cmbField");
            this.cmbField.FormattingEnabled = true;
            this.cmbField.Name = "cmbField";
            this.cmbField.Sorted = true;
            this.cmbField.DropDown += new System.EventHandler(this.cmbField_DropDown);
            // 
            // btnDisplayField
            // 
            resources.ApplyResources(this.btnDisplayField, "btnDisplayField");
            this.btnDisplayField.Name = "btnDisplayField";
            this.btnDisplayField.UseVisualStyleBackColor = true;
            this.btnDisplayField.CheckedChanged += new System.EventHandler(this.btnSearch_CheckedChanged);
            // 
            // btnField
            // 
            resources.ApplyResources(this.btnField, "btnField");
            this.btnField.Name = "btnField";
            this.btnField.UseVisualStyleBackColor = true;
            this.btnField.CheckedChanged += new System.EventHandler(this.btnSearch_CheckedChanged);
            // 
            // btnAllFields
            // 
            resources.ApplyResources(this.btnAllFields, "btnAllFields");
            this.btnAllFields.Checked = true;
            this.btnAllFields.Name = "btnAllFields";
            this.btnAllFields.TabStop = true;
            this.btnAllFields.UseVisualStyleBackColor = true;
            this.btnAllFields.CheckedChanged += new System.EventHandler(this.btnSearch_CheckedChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // chkWildcards
            // 
            resources.ApplyResources(this.chkWildcards, "chkWildcards");
            this.chkWildcards.Checked = true;
            this.chkWildcards.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWildcards.Name = "chkWildcards";
            this.chkWildcards.UseVisualStyleBackColor = true;
            // 
            // cmbQueryText
            // 
            this.cmbQueryText.FormattingEnabled = true;
            resources.ApplyResources(this.cmbQueryText, "cmbQueryText");
            this.cmbQueryText.Name = "cmbQueryText";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // panel5
            // 
            resources.ApplyResources(this.panel5, "panel5");
            this.panel5.Name = "panel5";
            // 
            // panel4
            // 
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // panelCustom
            // 
            this.panelCustom.Controls.Add(this.panelCustomFields);
            this.panelCustom.Controls.Add(this.lblQueryName);
            resources.ApplyResources(this.panelCustom, "panelCustom");
            this.panelCustom.Name = "panelCustom";
            // 
            // panelCustomFields
            // 
            resources.ApplyResources(this.panelCustomFields, "panelCustomFields");
            this.panelCustomFields.Name = "panelCustomFields";
            // 
            // lblQueryName
            // 
            resources.ApplyResources(this.lblQueryName, "lblQueryName");
            this.lblQueryName.Name = "lblQueryName";
            // 
            // FormQuery
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.panelCustom);
            this.Controls.Add(this.panelStandard);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormQuery";
            this.ShowInTaskbar = false;
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panelStandard.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panelCustom.ResumeLayout(false);
            this.panelCustom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private gView.Framework.UI.Controls.ProgressDisk progressDisk;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panelStandard;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbField;
        private System.Windows.Forms.RadioButton btnDisplayField;
        private System.Windows.Forms.RadioButton btnField;
        private System.Windows.Forms.RadioButton btnAllFields;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkWildcards;
        private System.Windows.Forms.ComboBox cmbQueryText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lblMsg2;
        private System.Windows.Forms.Label lblMsg1;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Panel panelCustom;
        private System.Windows.Forms.Label lblQueryName;
        private System.Windows.Forms.Panel panelCustomFields;
    }
}