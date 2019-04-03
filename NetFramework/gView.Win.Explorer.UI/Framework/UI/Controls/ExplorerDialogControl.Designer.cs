namespace gView.Framework.UI.Controls
{
    partial class ExplorerDialogControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExplorerDialogControl));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbFilters = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtElement = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.label3 = new System.Windows.Forms.Label();
            this.catalogComboBox1 = new gView.Framework.UI.Controls.CatalogComboBox();
            this.contentsList1 = new gView.Framework.UI.Controls.ContentsList();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.btnFavorites = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolAddToFavorites = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.cmbFilters);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.txtElement);
            this.panel1.Name = "panel1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // cmbFilters
            // 
            resources.ApplyResources(this.cmbFilters, "cmbFilters");
            this.cmbFilters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilters.FormattingEnabled = true;
            this.cmbFilters.Name = "cmbFilters";
            this.cmbFilters.SelectedIndexChanged += new System.EventHandler(this.cmbFilters_SelectedIndexChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // txtElement
            // 
            resources.ApplyResources(this.txtElement, "txtElement");
            this.txtElement.Name = "txtElement";
            this.txtElement.TextChanged += new System.EventHandler(this.txtElement_TextChanged);
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Controls.Add(this.toolStrip1);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.catalogComboBox1);
            this.panel3.Name = "panel3";
            // 
            // toolStrip1
            // 
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripSplitButton1,
            this.btnFavorites});
            this.toolStrip1.Name = "toolStrip1";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // catalogComboBox1
            // 
            resources.ApplyResources(this.catalogComboBox1, "catalogComboBox1");
            this.catalogComboBox1.Name = "catalogComboBox1";
            this.catalogComboBox1.SelectedItemChanged += new gView.Framework.UI.Controls.CatalogComboBox.SelectedItemChangedEvent(this.catalogComboBox1_SelectedItemChanged);
            // 
            // contentsList1
            // 
            resources.ApplyResources(this.contentsList1, "contentsList1");
            this.contentsList1.AllowContextMenu = true;
            this.contentsList1.ExplorerObject = null;
            this.contentsList1.Filter = null;
            this.contentsList1.IsOpenDialog = true;
            this.contentsList1.MultiSelection = true;
            this.contentsList1.Name = "contentsList1";
            this.contentsList1.View = System.Windows.Forms.View.Details;
            // 
            // toolStripButton1
            // 
            resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // toolStripSplitButton1
            // 
            resources.ApplyResources(this.toolStripSplitButton1, "toolStripSplitButton1");
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2});
            this.toolStripSplitButton1.Image = global::gView.Win.Explorer.UI.Properties.Resources.ExplorerStyle;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            // 
            // toolStripMenuItem1
            // 
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            this.toolStripMenuItem1.Image = global::gView.Win.Explorer.UI.Properties.Resources.ExplorerStyle_list;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.btnList_Click);
            // 
            // toolStripMenuItem2
            // 
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.Image = global::gView.Win.Explorer.UI.Properties.Resources.ExplorerStyle_details;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // btnFavorites
            // 
            resources.ApplyResources(this.btnFavorites, "btnFavorites");
            this.btnFavorites.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFavorites.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolAddToFavorites});
            this.btnFavorites.Image = global::gView.Win.Explorer.UI.Properties.Resources.folder_heart;
            this.btnFavorites.Name = "btnFavorites";
            // 
            // toolAddToFavorites
            // 
            resources.ApplyResources(this.toolAddToFavorites, "toolAddToFavorites");
            this.toolAddToFavorites.Image = global::gView.Win.Explorer.UI.Properties.Resources.folder_heart;
            this.toolAddToFavorites.Name = "toolAddToFavorites";
            this.toolAddToFavorites.Click += new System.EventHandler(this.toolAddToFavorites_Click);
            // 
            // ExplorerDialogControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.contentsList1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Name = "ExplorerDialogControl";
            this.Load += new System.EventHandler(this.ExplorerDialogControl_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cmbFilters;
        private System.Windows.Forms.TextBox txtElement;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private ContentsList contentsList1;
        private CatalogComboBox catalogComboBox1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripDropDownButton btnFavorites;
        private System.Windows.Forms.ToolStripMenuItem toolAddToFavorites;
    }
}
