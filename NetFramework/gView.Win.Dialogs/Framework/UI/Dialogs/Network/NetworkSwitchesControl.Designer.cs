namespace gView.Framework.UI.Dialogs.Network
{
    partial class NetworkSwitchesControl
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
            this.gridFcs = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.NodeType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridFcs)).BeginInit();
            this.SuspendLayout();
            // 
            // gridFcs
            // 
            this.gridFcs.AllowUserToAddRows = false;
            this.gridFcs.AllowUserToDeleteRows = false;
            this.gridFcs.AllowUserToResizeRows = false;
            this.gridFcs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gridFcs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridFcs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column4,
            this.Column2,
            this.Column3,
            this.NodeType});
            this.gridFcs.Location = new System.Drawing.Point(3, 62);
            this.gridFcs.MultiSelect = false;
            this.gridFcs.Name = "gridFcs";
            this.gridFcs.RowHeadersVisible = false;
            this.gridFcs.Size = new System.Drawing.Size(444, 203);
            this.gridFcs.TabIndex = 1;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "FCID";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Visible = false;
            this.Column1.Width = 60;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Is Switch";
            this.Column4.Name = "Column4";
            this.Column4.Width = 70;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "FeatureClass";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 200;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "FieldName (Switch State)";
            this.Column3.Name = "Column3";
            this.Column3.Width = 150;
            // 
            // NodeType
            // 
            this.NodeType.HeaderText = "Node Type";
            this.NodeType.Name = "NodeType";
            // 
            // NetworkSwitchesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridFcs);
            this.Name = "NetworkSwitchesControl";
            this.Size = new System.Drawing.Size(450, 268);
            ((System.ComponentModel.ISupportInitialize)(this.gridFcs)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView gridFcs;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewComboBoxColumn Column3;
        private System.Windows.Forms.DataGridViewComboBoxColumn NodeType;
    }
}
