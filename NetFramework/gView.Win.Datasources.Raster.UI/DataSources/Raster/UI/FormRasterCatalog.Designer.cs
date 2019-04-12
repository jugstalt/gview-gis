namespace gView.DataSources.Raster.UI
{
    partial class FormRasterCatalog
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnGetCatalog = new System.Windows.Forms.Button();
            this.lstFiles = new System.Windows.Forms.CheckedListBox();
            this.btnGetFiles = new System.Windows.Forms.Button();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Catalog:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(65, 10);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(290, 20);
            this.textBox1.TabIndex = 1;
            // 
            // btnGetCatalog
            // 
            this.btnGetCatalog.Location = new System.Drawing.Point(361, 10);
            this.btnGetCatalog.Name = "btnGetCatalog";
            this.btnGetCatalog.Size = new System.Drawing.Size(29, 20);
            this.btnGetCatalog.TabIndex = 2;
            this.btnGetCatalog.Text = "...";
            this.btnGetCatalog.UseVisualStyleBackColor = true;
            // 
            // lstFiles
            // 
            this.lstFiles.FormattingEnabled = true;
            this.lstFiles.Location = new System.Drawing.Point(16, 46);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.Size = new System.Drawing.Size(374, 319);
            this.lstFiles.TabIndex = 3;
            // 
            // btnGetFiles
            // 
            this.btnGetFiles.Location = new System.Drawing.Point(292, 372);
            this.btnGetFiles.Name = "btnGetFiles";
            this.btnGetFiles.Size = new System.Drawing.Size(97, 23);
            this.btnGetFiles.TabIndex = 4;
            this.btnGetFiles.Text = "Get Files ...";
            this.btnGetFiles.UseVisualStyleBackColor = true;
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(233, 424);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(156, 35);
            this.btnCreate.TabIndex = 5;
            this.btnCreate.Text = "Create/Append Catalog";
            this.btnCreate.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(16, 424);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(126, 35);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // FormRasterCatalog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 473);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.btnGetFiles);
            this.Controls.Add(this.lstFiles);
            this.Controls.Add(this.btnGetCatalog);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormRasterCatalog";
            this.Text = "Create Raster Catalog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnGetCatalog;
        private System.Windows.Forms.CheckedListBox lstFiles;
        private System.Windows.Forms.Button btnGetFiles;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;
    }
}