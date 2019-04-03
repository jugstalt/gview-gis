namespace gView.Plugins.MapTools.Controls
{
    partial class IdentifyResultControl
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
            this.gridFeatureProperties = new System.Windows.Forms.PropertyGrid();
            this.cmbFeatures = new System.Windows.Forms.ComboBox();
            this.txtLocation = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // gridFeatureProperties
            // 
            this.gridFeatureProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gridFeatureProperties.Enabled = false;
            this.gridFeatureProperties.Location = new System.Drawing.Point(3, 31);
            this.gridFeatureProperties.Name = "gridFeatureProperties";
            this.gridFeatureProperties.Size = new System.Drawing.Size(262, 462);
            this.gridFeatureProperties.TabIndex = 0;
            // 
            // cmbFeatures
            // 
            this.cmbFeatures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbFeatures.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFeatures.FormattingEnabled = true;
            this.cmbFeatures.Location = new System.Drawing.Point(3, 4);
            this.cmbFeatures.Name = "cmbFeatures";
            this.cmbFeatures.Size = new System.Drawing.Size(262, 21);
            this.cmbFeatures.TabIndex = 1;
            this.cmbFeatures.SelectedIndexChanged += new System.EventHandler(this.cmbFeatures_SelectedIndexChanged);
            // 
            // txtLocation
            // 
            this.txtLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLocation.Enabled = false;
            this.txtLocation.Location = new System.Drawing.Point(3, 499);
            this.txtLocation.Name = "txtLocation";
            this.txtLocation.Size = new System.Drawing.Size(262, 20);
            this.txtLocation.TabIndex = 5;
            // 
            // IdentifyResultControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtLocation);
            this.Controls.Add(this.cmbFeatures);
            this.Controls.Add(this.gridFeatureProperties);
            this.Name = "IdentifyResultControl";
            this.Size = new System.Drawing.Size(268, 522);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid gridFeatureProperties;
        private System.Windows.Forms.ComboBox cmbFeatures;
        private System.Windows.Forms.TextBox txtLocation;
    }
}
