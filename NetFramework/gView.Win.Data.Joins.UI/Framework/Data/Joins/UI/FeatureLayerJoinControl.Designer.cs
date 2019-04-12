namespace gView.Framework.Data.Joins.UI
{
    partial class FeatureLayerJoinControl
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
            this.gbJoinedFeaturelayer = new System.Windows.Forms.GroupBox();
            this.cmbJoinedFeatureLayerJoinField = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.cmbJoinedLayer = new System.Windows.Forms.ComboBox();
            this.gbJoinedFeaturelayer.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbJoinedFeaturelayer
            // 
            this.gbJoinedFeaturelayer.Controls.Add(this.cmbJoinedFeatureLayerJoinField);
            this.gbJoinedFeaturelayer.Controls.Add(this.label10);
            this.gbJoinedFeaturelayer.Controls.Add(this.label9);
            this.gbJoinedFeaturelayer.Controls.Add(this.cmbJoinedLayer);
            this.gbJoinedFeaturelayer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbJoinedFeaturelayer.Location = new System.Drawing.Point(0, 0);
            this.gbJoinedFeaturelayer.Name = "gbJoinedFeaturelayer";
            this.gbJoinedFeaturelayer.Size = new System.Drawing.Size(411, 162);
            this.gbJoinedFeaturelayer.TabIndex = 14;
            this.gbJoinedFeaturelayer.TabStop = false;
            this.gbJoinedFeaturelayer.Text = "Joined Feature Layer";
            // 
            // cmbJoinedFeatureLayerJoinField
            // 
            this.cmbJoinedFeatureLayerJoinField.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbJoinedFeatureLayerJoinField.FormattingEnabled = true;
            this.cmbJoinedFeatureLayerJoinField.Location = new System.Drawing.Point(139, 54);
            this.cmbJoinedFeatureLayerJoinField.Name = "cmbJoinedFeatureLayerJoinField";
            this.cmbJoinedFeatureLayerJoinField.Size = new System.Drawing.Size(265, 21);
            this.cmbJoinedFeatureLayerJoinField.TabIndex = 21;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(33, 58);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(100, 13);
            this.label10.TabIndex = 20;
            this.label10.Text = "Feature Layer Field:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(15, 27);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(36, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Layer:";
            // 
            // cmbJoinedLayer
            // 
            this.cmbJoinedLayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbJoinedLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbJoinedLayer.FormattingEnabled = true;
            this.cmbJoinedLayer.Location = new System.Drawing.Point(58, 23);
            this.cmbJoinedLayer.Name = "cmbJoinedLayer";
            this.cmbJoinedLayer.Size = new System.Drawing.Size(346, 21);
            this.cmbJoinedLayer.TabIndex = 17;
            this.cmbJoinedLayer.SelectedIndexChanged += new System.EventHandler(this.cmbJoinedLayer_SelectedIndexChanged);
            // 
            // FeatureLayerJoinControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbJoinedFeaturelayer);
            this.Name = "FeatureLayerJoinControl";
            this.Size = new System.Drawing.Size(411, 162);
            this.gbJoinedFeaturelayer.ResumeLayout(false);
            this.gbJoinedFeaturelayer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbJoinedFeaturelayer;
        private System.Windows.Forms.ComboBox cmbJoinedFeatureLayerJoinField;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cmbJoinedLayer;
    }
}
