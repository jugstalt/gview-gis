namespace gView.Framework.UI.Dialogs
{
    partial class FormFavoritesList
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFavoritesList));
            this.lstFavorites = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.mnStripFavorite = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.mnStripFavorite.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstFavorites
            // 
            this.lstFavorites.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstFavorites.Location = new System.Drawing.Point(0, 0);
            this.lstFavorites.Name = "lstFavorites";
            this.lstFavorites.Size = new System.Drawing.Size(275, 540);
            this.lstFavorites.SmallImageList = this.imageList1;
            this.lstFavorites.TabIndex = 0;
            this.lstFavorites.UseCompatibleStateImageBehavior = false;
            this.lstFavorites.View = System.Windows.Forms.View.List;
            this.lstFavorites.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstFavorites_MouseDoubleClick);
            this.lstFavorites.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstFavorites_MouseDown);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "folder_go.png");
            this.imageList1.Images.SetKeyName(1, "folder_heart.png");
            // 
            // mnStripFavorite
            // 
            this.mnStripFavorite.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnRemove});
            this.mnStripFavorite.Name = "mnStripRemove";
            this.mnStripFavorite.Size = new System.Drawing.Size(153, 48);
            // 
            // mnRemove
            // 
            this.mnRemove.Name = "mnRemove";
            this.mnRemove.Size = new System.Drawing.Size(152, 22);
            this.mnRemove.Text = "Remove";
            this.mnRemove.Click += new System.EventHandler(this.mnRemove_Click);
            // 
            // FormFavoritesList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(275, 540);
            this.Controls.Add(this.lstFavorites);
            this.Name = "FormFavoritesList";
            this.Text = "FormFavoritesList";
            this.Load += new System.EventHandler(this.FormFavoritesList_Load);
            this.mnStripFavorite.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lstFavorites;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip mnStripFavorite;
        private System.Windows.Forms.ToolStripMenuItem mnRemove;
    }
}