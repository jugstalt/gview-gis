namespace gView.Interoperability.OGC.UI.Dataset.WMS
{
    partial class Metadata_WMS
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

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cmbInfoFormat = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cmbGetMapFormat = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbCoordSystem = new System.Windows.Forms.ComboBox();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cmbInfoFormat);
            this.groupBox3.Location = new System.Drawing.Point(206, 71);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(172, 50);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Info Format";
            // 
            // cmbInfoFormat
            // 
            this.cmbInfoFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbInfoFormat.FormattingEnabled = true;
            this.cmbInfoFormat.Location = new System.Drawing.Point(11, 19);
            this.cmbInfoFormat.Name = "cmbInfoFormat";
            this.cmbInfoFormat.Size = new System.Drawing.Size(152, 21);
            this.cmbInfoFormat.TabIndex = 2;
            this.cmbInfoFormat.SelectedIndexChanged += new System.EventHandler(this.cmbInfoFormat_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cmbGetMapFormat);
            this.groupBox2.Location = new System.Drawing.Point(12, 71);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(172, 50);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Image Format";
            // 
            // cmbGetMapFormat
            // 
            this.cmbGetMapFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGetMapFormat.FormattingEnabled = true;
            this.cmbGetMapFormat.Location = new System.Drawing.Point(11, 19);
            this.cmbGetMapFormat.Name = "cmbGetMapFormat";
            this.cmbGetMapFormat.Size = new System.Drawing.Size(152, 21);
            this.cmbGetMapFormat.TabIndex = 2;
            this.cmbGetMapFormat.SelectedIndexChanged += new System.EventHandler(this.cmbGetMapFormat_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbCoordSystem);
            this.groupBox1.Location = new System.Drawing.Point(12, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(366, 50);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Coordinate System";
            // 
            // cmbCoordSystem
            // 
            this.cmbCoordSystem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCoordSystem.FormattingEnabled = true;
            this.cmbCoordSystem.Location = new System.Drawing.Point(11, 19);
            this.cmbCoordSystem.Name = "cmbCoordSystem";
            this.cmbCoordSystem.Size = new System.Drawing.Size(349, 21);
            this.cmbCoordSystem.TabIndex = 2;
            this.cmbCoordSystem.SelectedIndexChanged += new System.EventHandler(this.cmbCoordSystem_SelectedIndexChanged);
            // 
            // Metadata_WMS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Metadata_WMS";
            this.Size = new System.Drawing.Size(395, 344);
            this.Load += new System.EventHandler(this.Metadata_WMS_Load);
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cmbInfoFormat;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox cmbGetMapFormat;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbCoordSystem;
    }
}
