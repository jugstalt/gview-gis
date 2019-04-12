namespace gView.DataSources.OGR.UI
{
    partial class FormNewOgrDataset
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnTestConnecton = new System.Windows.Forms.Button();
            this.btnConnect2Folder = new System.Windows.Forms.Button();
            this.btnConnect2File = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Connectionstring:";
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConnectionString.Location = new System.Drawing.Point(12, 37);
            this.txtConnectionString.Multiline = true;
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new System.Drawing.Size(513, 126);
            this.txtConnectionString.TabIndex = 1;
            this.txtConnectionString.TextChanged += new System.EventHandler(this.txtConnectionString_TextChanged);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(450, 249);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(15, 249);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnTestConnecton
            // 
            this.btnTestConnecton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTestConnecton.Location = new System.Drawing.Point(299, 249);
            this.btnTestConnecton.Name = "btnTestConnecton";
            this.btnTestConnecton.Size = new System.Drawing.Size(145, 23);
            this.btnTestConnecton.TabIndex = 4;
            this.btnTestConnecton.Text = "Test Connection...";
            this.btnTestConnecton.UseVisualStyleBackColor = true;
            this.btnTestConnecton.Click += new System.EventHandler(this.btnTestConnecton_Click);
            // 
            // btnConnect2Folder
            // 
            this.btnConnect2Folder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect2Folder.Image = global::gView.Win.DataSources.GDAL.UI.Properties.Resources.img_23;
            this.btnConnect2Folder.Location = new System.Drawing.Point(409, 169);
            this.btnConnect2Folder.Name = "btnConnect2Folder";
            this.btnConnect2Folder.Size = new System.Drawing.Size(116, 57);
            this.btnConnect2Folder.TabIndex = 6;
            this.btnConnect2Folder.Text = "Connect to folder...";
            this.btnConnect2Folder.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnConnect2Folder.UseVisualStyleBackColor = true;
            this.btnConnect2Folder.Click += new System.EventHandler(this.btnConnect2Folder_Click);
            // 
            // btnConnect2File
            // 
            this.btnConnect2File.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect2File.Image = global::gView.Win.DataSources.GDAL.UI.Properties.Resources.img_26_20_19;
            this.btnConnect2File.Location = new System.Drawing.Point(299, 169);
            this.btnConnect2File.Name = "btnConnect2File";
            this.btnConnect2File.Size = new System.Drawing.Size(104, 57);
            this.btnConnect2File.TabIndex = 5;
            this.btnConnect2File.Text = "Connect to file...";
            this.btnConnect2File.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnConnect2File.UseVisualStyleBackColor = true;
            this.btnConnect2File.Click += new System.EventHandler(this.btnConnect2File_Click);
            // 
            // FormNewOgrDataset
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(537, 284);
            this.Controls.Add(this.btnConnect2Folder);
            this.Controls.Add(this.btnConnect2File);
            this.Controls.Add(this.btnTestConnecton);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtConnectionString);
            this.Controls.Add(this.label1);
            this.Name = "FormNewOgrDataset";
            this.Text = "New OGR Dataset";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnTestConnecton;
        private System.Windows.Forms.Button btnConnect2File;
        private System.Windows.Forms.Button btnConnect2Folder;
    }
}