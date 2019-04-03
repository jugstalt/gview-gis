namespace gView.Framework.UI.Dialogs
{
    partial class FormCatalogTree
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.catalogTreeControl1 = new gView.Framework.UI.Controls.CatalogTreeControl();
            this.SuspendLayout();
            // 
            // catalogTreeControl1
            // 
            this.catalogTreeControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.catalogTreeControl1.ExplorerApplication = null;
            this.catalogTreeControl1.Filter = null;
            this.catalogTreeControl1.Location = new System.Drawing.Point(0, 0);
            this.catalogTreeControl1.Name = "catalogTreeControl1";
            this.catalogTreeControl1.Size = new System.Drawing.Size(253, 388);
            this.catalogTreeControl1.TabIndex = 0;
            this.catalogTreeControl1.NodeSelected += new gView.Framework.UI.Controls.CatalogTreeControl.NodeClickedEvent(this.catalogTreeControl1_NodeSelected);
            this.catalogTreeControl1.NodeDeleted += new gView.Framework.UI.Controls.CatalogTreeControl.NodeDeletedEvent(this.catalogTreeControl1_NodeDeleted);
            this.catalogTreeControl1.NodeRenamed += new gView.Framework.UI.Controls.CatalogTreeControl.NodeRenamedEvent(this.catalogTreeControl1_NodeRenamed);
            // 
            // FormCatalogTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(253, 388);
            this.Controls.Add(this.catalogTreeControl1);
            this.Name = "FormCatalogTree";
            this.Text = "Explorer Tree";
            this.Load += new System.EventHandler(this.FormCatalogTree_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private gView.Framework.UI.Controls.CatalogTreeControl catalogTreeControl1;

    }
}