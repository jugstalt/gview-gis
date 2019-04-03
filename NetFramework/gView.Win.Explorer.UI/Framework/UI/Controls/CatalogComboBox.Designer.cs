namespace gView.Framework.UI.Controls
{
    partial class CatalogComboBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CatalogComboBox));
            this.cmbCatalog = new System.Windows.Forms.ComboBox();
            this.imageList1 = new System.Windows.Forms.ImageList();
            this.SuspendLayout();
            // 
            // cmbCatalog
            // 
            this.cmbCatalog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbCatalog.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbCatalog.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCatalog.FormattingEnabled = true;
            this.cmbCatalog.ItemHeight = 16;
            this.cmbCatalog.Location = new System.Drawing.Point(0, 0);
            this.cmbCatalog.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbCatalog.Name = "cmbCatalog";
            this.cmbCatalog.Size = new System.Drawing.Size(428, 22);
            this.cmbCatalog.TabIndex = 0;
            this.cmbCatalog.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cmbCatalog_DrawItem);
            this.cmbCatalog.SelectedIndexChanged += new System.EventHandler(this.cmbCatalog_SelectedIndexChanged);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "computer_16.gif");
            this.imageList1.Images.SetKeyName(1, "folder-closed_16.gif");
            this.imageList1.Images.SetKeyName(2, "folder-open_16.gif");
            this.imageList1.Images.SetKeyName(3, "documents_16.gif");
            this.imageList1.Images.SetKeyName(4, "disc.png");
            this.imageList1.Images.SetKeyName(5, "mappeddrive.png");
            this.imageList1.Images.SetKeyName(6, "harddisc.png");
            this.imageList1.Images.SetKeyName(7, "floppy.png");
            // 
            // CatalogComboBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmbCatalog);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "CatalogComboBox";
            this.Size = new System.Drawing.Size(428, 38);
            this.Load += new System.EventHandler(this.CatalogComboBox_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbCatalog;
        internal System.Windows.Forms.ImageList imageList1;
    }
}
