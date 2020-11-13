namespace gView.DataSources.TileCache.UI
{
    partial class FormLocalCacheProperties
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLocalCacheProperties));
            this.chkUseLocalCaching = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.btnGetFolder = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lstCaches = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnEraseCache = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkUseLocalCaching
            // 
            this.chkUseLocalCaching.AutoSize = true;
            this.chkUseLocalCaching.Location = new System.Drawing.Point(15, 30);
            this.chkUseLocalCaching.Name = "chkUseLocalCaching";
            this.chkUseLocalCaching.Size = new System.Drawing.Size(205, 17);
            this.chkUseLocalCaching.TabIndex = 0;
            this.chkUseLocalCaching.Text = "Use local caching of downloaded tiles";
            this.chkUseLocalCaching.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Local Cache Folder:";
            // 
            // txtFolder
            // 
            this.txtFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFolder.Location = new System.Drawing.Point(120, 56);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(389, 20);
            this.txtFolder.TabIndex = 2;
            // 
            // btnGetFolder
            // 
            this.btnGetFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetFolder.BackgroundImage = global::gView.Win.DataSources.TileCache.UI.Properties.Resources.folder_open_16;
            this.btnGetFolder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnGetFolder.Location = new System.Drawing.Point(516, 54);
            this.btnGetFolder.Name = "btnGetFolder";
            this.btnGetFolder.Size = new System.Drawing.Size(37, 23);
            this.btnGetFolder.TabIndex = 3;
            this.btnGetFolder.UseVisualStyleBackColor = true;
            this.btnGetFolder.Click += new System.EventHandler(this.btnGetFolder_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(393, 308);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(160, 27);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "Close and Save Settings";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(239, 308);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(148, 27);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lstCaches
            // 
            this.lstCaches.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCaches.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.columnHeader2,
            this.columnHeader3});
            this.lstCaches.FullRowSelect = true;
            this.lstCaches.Location = new System.Drawing.Point(15, 93);
            this.lstCaches.MultiSelect = false;
            this.lstCaches.Name = "lstCaches";
            this.lstCaches.Size = new System.Drawing.Size(495, 197);
            this.lstCaches.TabIndex = 6;
            this.lstCaches.UseCompatibleStateImageBehavior = false;
            this.lstCaches.View = System.Windows.Forms.View.Details;
            this.lstCaches.SelectedIndexChanged += new System.EventHandler(this.lstCaches_SelectedIndexChanged);
            // 
            // colName
            // 
            this.colName.Text = "Cache";
            this.colName.Width = 164;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Files";
            this.columnHeader2.Width = 133;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Size (MB)";
            this.columnHeader3.Width = 159;
            // 
            // btnEraseCache
            // 
            this.btnEraseCache.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            //this.btnEraseCache.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnEraseCache.BackgroundImage")));
            //this.btnEraseCache.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnEraseCache.Enabled = false;
            this.btnEraseCache.Location = new System.Drawing.Point(516, 120);
            this.btnEraseCache.Name = "btnEraseCache";
            this.btnEraseCache.Size = new System.Drawing.Size(37, 34);
            this.btnEraseCache.TabIndex = 7;
            this.btnEraseCache.UseVisualStyleBackColor = true;
            this.btnEraseCache.Click += new System.EventHandler(this.btnEraseCache_Click);
            this.btnEraseCache.Text = "X";
            // 
            // FormLocalCacheProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 346);
            this.Controls.Add(this.btnEraseCache);
            this.Controls.Add(this.lstCaches);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnGetFolder);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkUseLocalCaching);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormLocalCacheProperties";
            this.Text = "Local Caching Properties";
            this.Load += new System.EventHandler(this.FormLocalCacheProperties_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkUseLocalCaching;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnGetFolder;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ListView lstCaches;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button btnEraseCache;
    }
}