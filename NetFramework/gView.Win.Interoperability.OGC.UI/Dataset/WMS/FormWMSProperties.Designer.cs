namespace gView.Interoperability.OGC.UI.Dataset.WMS
{
    partial class FormWMSProperties
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWMSProperties));
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkUseSLD_BODY = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cmbInfoFormat = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cmbGetMapFormat = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbCoordSystem = new System.Windows.Forms.ComboBox();
            this.panel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AccessibleDescription = null;
            this.panel1.AccessibleName = null;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackgroundImage = null;
            this.panel1.Controls.Add(this.chkUseSLD_BODY);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Font = null;
            this.panel1.Name = "panel1";
            // 
            // chkUseSLD_BODY
            // 
            this.chkUseSLD_BODY.AccessibleDescription = null;
            this.chkUseSLD_BODY.AccessibleName = null;
            resources.ApplyResources(this.chkUseSLD_BODY, "chkUseSLD_BODY");
            this.chkUseSLD_BODY.BackgroundImage = null;
            this.chkUseSLD_BODY.Font = null;
            this.chkUseSLD_BODY.Name = "chkUseSLD_BODY";
            this.chkUseSLD_BODY.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.AccessibleDescription = null;
            this.groupBox3.AccessibleName = null;
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.BackgroundImage = null;
            this.groupBox3.Controls.Add(this.cmbInfoFormat);
            this.groupBox3.Font = null;
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // cmbInfoFormat
            // 
            this.cmbInfoFormat.AccessibleDescription = null;
            this.cmbInfoFormat.AccessibleName = null;
            resources.ApplyResources(this.cmbInfoFormat, "cmbInfoFormat");
            this.cmbInfoFormat.BackgroundImage = null;
            this.cmbInfoFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbInfoFormat.Font = null;
            this.cmbInfoFormat.FormattingEnabled = true;
            this.cmbInfoFormat.Name = "cmbInfoFormat";
            // 
            // groupBox2
            // 
            this.groupBox2.AccessibleDescription = null;
            this.groupBox2.AccessibleName = null;
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.BackgroundImage = null;
            this.groupBox2.Controls.Add(this.cmbGetMapFormat);
            this.groupBox2.Font = null;
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // cmbGetMapFormat
            // 
            this.cmbGetMapFormat.AccessibleDescription = null;
            this.cmbGetMapFormat.AccessibleName = null;
            resources.ApplyResources(this.cmbGetMapFormat, "cmbGetMapFormat");
            this.cmbGetMapFormat.BackgroundImage = null;
            this.cmbGetMapFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGetMapFormat.Font = null;
            this.cmbGetMapFormat.FormattingEnabled = true;
            this.cmbGetMapFormat.Name = "cmbGetMapFormat";
            // 
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = null;
            this.groupBox1.AccessibleName = null;
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.BackgroundImage = null;
            this.groupBox1.Controls.Add(this.cmbCoordSystem);
            this.groupBox1.Font = null;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // cmbCoordSystem
            // 
            this.cmbCoordSystem.AccessibleDescription = null;
            this.cmbCoordSystem.AccessibleName = null;
            resources.ApplyResources(this.cmbCoordSystem, "cmbCoordSystem");
            this.cmbCoordSystem.BackgroundImage = null;
            this.cmbCoordSystem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCoordSystem.Font = null;
            this.cmbCoordSystem.FormattingEnabled = true;
            this.cmbCoordSystem.Name = "cmbCoordSystem";
            // 
            // FormWMSProperties
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.panel1);
            this.Font = null;
            this.Icon = null;
            this.Name = "FormWMSProperties";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cmbCoordSystem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cmbInfoFormat;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox cmbGetMapFormat;
        private System.Windows.Forms.CheckBox chkUseSLD_BODY;
    }
}