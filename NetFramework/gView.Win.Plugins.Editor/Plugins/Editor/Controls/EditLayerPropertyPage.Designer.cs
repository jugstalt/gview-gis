namespace gView.Plugins.Editor.Controls
{
    partial class EditLayerPropertyPage
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
            this.dgFields = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.colEditable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colFieldName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFieldAlias = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsRequired = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDefaultValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDomain = new System.Windows.Forms.DataGridViewButtonColumn();
            this.panelPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgFields)).BeginInit();
            this.SuspendLayout();
            // 
            // panelPage
            // 
            this.panelPage.Controls.Add(this.dgFields);
            this.panelPage.Controls.Add(this.panel1);
            this.panelPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPage.Location = new System.Drawing.Point(0, 0);
            this.panelPage.Name = "panelPage";
            this.panelPage.Size = new System.Drawing.Size(475, 346);
            this.panelPage.TabIndex = 0;
            // 
            // dgFields
            // 
            this.dgFields.AllowUserToAddRows = false;
            this.dgFields.AllowUserToDeleteRows = false;
            this.dgFields.AllowUserToResizeRows = false;
            this.dgFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgFields.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colEditable,
            this.colFieldName,
            this.colFieldAlias,
            this.colIsRequired,
            this.colDefaultValue,
            this.colDomain});
            this.dgFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgFields.Location = new System.Drawing.Point(0, 0);
            this.dgFields.Name = "dgFields";
            this.dgFields.RowHeadersVisible = false;
            this.dgFields.Size = new System.Drawing.Size(475, 285);
            this.dgFields.TabIndex = 1;
            this.dgFields.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgFields_CellClick);
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 285);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(475, 61);
            this.panel1.TabIndex = 0;
            // 
            // colEditable
            // 
            this.colEditable.HeaderText = "Editable";
            this.colEditable.Name = "colEditable";
            this.colEditable.Width = 50;
            // 
            // colFieldName
            // 
            this.colFieldName.HeaderText = "Field Name";
            this.colFieldName.Name = "colFieldName";
            this.colFieldName.ReadOnly = true;
            this.colFieldName.Width = 130;
            // 
            // colFieldAlias
            // 
            this.colFieldAlias.HeaderText = "Field Alias";
            this.colFieldAlias.Name = "colFieldAlias";
            this.colFieldAlias.ReadOnly = true;
            this.colFieldAlias.Visible = false;
            // 
            // colIsRequired
            // 
            this.colIsRequired.HeaderText = "IsRequired";
            this.colIsRequired.Name = "colIsRequired";
            this.colIsRequired.Width = 60;
            // 
            // colDefaultValue
            // 
            this.colDefaultValue.HeaderText = "Default Value";
            this.colDefaultValue.Name = "colDefaultValue";
            // 
            // colDomain
            // 
            this.colDomain.HeaderText = "Domain";
            this.colDomain.Name = "colDomain";
            this.colDomain.Width = 120;
            // 
            // EditLayerPropertyPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelPage);
            this.Name = "EditLayerPropertyPage";
            this.Size = new System.Drawing.Size(475, 346);
            this.panelPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgFields)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelPage;
        private System.Windows.Forms.DataGridView dgFields;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colEditable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFieldName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFieldAlias;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsRequired;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDefaultValue;
        private System.Windows.Forms.DataGridViewButtonColumn colDomain;
    }
}
