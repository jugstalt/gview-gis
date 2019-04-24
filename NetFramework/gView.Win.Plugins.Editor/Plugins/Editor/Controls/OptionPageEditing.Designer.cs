namespace gView.Plugins.Editor.Controls
{
    partial class OptionPageEditing
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
            this.panelPage = new System.Windows.Forms.Panel();
            this.dgEditLayers = new System.Windows.Forms.DataGridView();
            this.colLayer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colInsert = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colUpdate = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDelete = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.panelPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgEditLayers)).BeginInit();
            this.SuspendLayout();
            // 
            // panelPage
            // 
            this.panelPage.Controls.Add(this.dgEditLayers);
            this.panelPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPage.Location = new System.Drawing.Point(0, 0);
            this.panelPage.Name = "panelPage";
            this.panelPage.Size = new System.Drawing.Size(370, 408);
            this.panelPage.TabIndex = 0;
            // 
            // dgEditLayers
            // 
            this.dgEditLayers.AllowUserToAddRows = false;
            this.dgEditLayers.AllowUserToDeleteRows = false;
            this.dgEditLayers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgEditLayers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLayer,
            this.colInsert,
            this.colUpdate,
            this.colDelete});
            this.dgEditLayers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgEditLayers.Location = new System.Drawing.Point(0, 0);
            this.dgEditLayers.Name = "dgEditLayers";
            this.dgEditLayers.RowHeadersVisible = false;
            this.dgEditLayers.Size = new System.Drawing.Size(370, 408);
            this.dgEditLayers.TabIndex = 0;
            // 
            // colLayer
            // 
            this.colLayer.HeaderText = "Layer";
            this.colLayer.Name = "colLayer";
            this.colLayer.ReadOnly = true;
            this.colLayer.Width = 200;
            // 
            // colInsert
            // 
            this.colInsert.HeaderText = "INSERT";
            this.colInsert.Name = "colInsert";
            this.colInsert.Width = 50;
            // 
            // colUpdate
            // 
            this.colUpdate.HeaderText = "UPDATE";
            this.colUpdate.Name = "colUpdate";
            this.colUpdate.Width = 55;
            // 
            // colDelete
            // 
            this.colDelete.HeaderText = "DELETE";
            this.colDelete.Name = "colDelete";
            this.colDelete.Width = 50;
            // 
            // OptionPageEditing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelPage);
            this.Name = "OptionPageEditing";
            this.Size = new System.Drawing.Size(370, 408);
            this.panelPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgEditLayers)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Panel panelPage;
        private System.Windows.Forms.DataGridView dgEditLayers;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLayer;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colInsert;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colUpdate;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colDelete;

    }
}
