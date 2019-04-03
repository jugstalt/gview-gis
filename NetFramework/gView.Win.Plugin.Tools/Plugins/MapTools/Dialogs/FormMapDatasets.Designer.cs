namespace gView.Plugins.MapTools.Dialogs
{
    partial class FormMapDatasets
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMapDatasets));
            this.tvDatasets = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.datasetInfoControl1 = new gView.Framework.UI.Controls.DatasetInfoControl();
            this.btnRemoveDataset = new System.Windows.Forms.Button();
            this.btnRemoveUnusedDatasets = new System.Windows.Forms.Button();
            this.btnCompressMap = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tvDatasets
            // 
            this.tvDatasets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvDatasets.ImageIndex = 0;
            this.tvDatasets.ImageList = this.imageList1;
            this.tvDatasets.Location = new System.Drawing.Point(1, 1);
            this.tvDatasets.Name = "tvDatasets";
            this.tvDatasets.SelectedImageIndex = 0;
            this.tvDatasets.Size = new System.Drawing.Size(225, 539);
            this.tvDatasets.TabIndex = 0;
            this.tvDatasets.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvDatasets_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "layers.png");
            this.imageList1.Images.SetKeyName(1, "dataset.png");
            // 
            // datasetInfoControl1
            // 
            this.datasetInfoControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.datasetInfoControl1.Dataset = null;
            this.datasetInfoControl1.Location = new System.Drawing.Point(229, 2);
            this.datasetInfoControl1.Name = "datasetInfoControl1";
            this.datasetInfoControl1.Size = new System.Drawing.Size(518, 488);
            this.datasetInfoControl1.TabIndex = 1;
            this.datasetInfoControl1.Visible = false;
            // 
            // btnRemoveDataset
            // 
            this.btnRemoveDataset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemoveDataset.Enabled = false;
            this.btnRemoveDataset.Location = new System.Drawing.Point(257, 496);
            this.btnRemoveDataset.Name = "btnRemoveDataset";
            this.btnRemoveDataset.Size = new System.Drawing.Size(111, 23);
            this.btnRemoveDataset.TabIndex = 2;
            this.btnRemoveDataset.Text = "Remove Dataset";
            this.btnRemoveDataset.UseVisualStyleBackColor = true;
            this.btnRemoveDataset.Click += new System.EventHandler(this.btnRemoveDataset_Click);
            // 
            // btnRemoveUnusedDatasets
            // 
            this.btnRemoveUnusedDatasets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemoveUnusedDatasets.Enabled = false;
            this.btnRemoveUnusedDatasets.Location = new System.Drawing.Point(374, 496);
            this.btnRemoveUnusedDatasets.Name = "btnRemoveUnusedDatasets";
            this.btnRemoveUnusedDatasets.Size = new System.Drawing.Size(141, 23);
            this.btnRemoveUnusedDatasets.TabIndex = 3;
            this.btnRemoveUnusedDatasets.Text = "Remove Unused Dataset";
            this.btnRemoveUnusedDatasets.UseVisualStyleBackColor = true;
            this.btnRemoveUnusedDatasets.Click += new System.EventHandler(this.btnRemoveUnusedDatasets_Click);
            // 
            // btnCompressMap
            // 
            this.btnCompressMap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCompressMap.Location = new System.Drawing.Point(521, 496);
            this.btnCompressMap.Name = "btnCompressMap";
            this.btnCompressMap.Size = new System.Drawing.Size(211, 23);
            this.btnCompressMap.TabIndex = 4;
            this.btnCompressMap.Text = "Compress/Clean (recomended)";
            this.btnCompressMap.UseVisualStyleBackColor = true;
            this.btnCompressMap.Click += new System.EventHandler(this.btnCompressMap_Click);
            // 
            // FormMapDatasets
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(752, 541);
            this.Controls.Add(this.btnCompressMap);
            this.Controls.Add(this.btnRemoveUnusedDatasets);
            this.Controls.Add(this.btnRemoveDataset);
            this.Controls.Add(this.datasetInfoControl1);
            this.Controls.Add(this.tvDatasets);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormMapDatasets";
            this.Text = "Map Datasets";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView tvDatasets;
        private System.Windows.Forms.ImageList imageList1;
        private Framework.UI.Controls.DatasetInfoControl datasetInfoControl1;
        private System.Windows.Forms.Button btnRemoveDataset;
        private System.Windows.Forms.Button btnRemoveUnusedDatasets;
        private System.Windows.Forms.Button btnCompressMap;
    }
}