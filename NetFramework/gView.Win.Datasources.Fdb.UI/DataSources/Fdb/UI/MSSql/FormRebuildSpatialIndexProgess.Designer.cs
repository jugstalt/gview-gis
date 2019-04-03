namespace gView.DataSources.Fdb.UI.MSSql
{
    partial class FormRebuildSpatialIndexProgess
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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.progressBar3 = new System.Windows.Forms.ProgressBar();
            this.label3 = new System.Windows.Forms.Label();
            this.plabel2 = new System.Windows.Forms.Label();
            this.plabel3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Update Spatial Index Definition:";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(16, 30);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(407, 17);
            this.progressBar1.TabIndex = 1;
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(16, 95);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(407, 17);
            this.progressBar2.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(145, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Calculate new feature nodes:";
            // 
            // progressBar3
            // 
            this.progressBar3.Location = new System.Drawing.Point(16, 158);
            this.progressBar3.Name = "progressBar3";
            this.progressBar3.Size = new System.Drawing.Size(407, 17);
            this.progressBar3.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 141);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(136, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Update new feature nodes:";
            // 
            // plabel2
            // 
            this.plabel2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.plabel2.AutoSize = true;
            this.plabel2.Location = new System.Drawing.Point(164, 78);
            this.plabel2.Name = "plabel2";
            this.plabel2.Size = new System.Drawing.Size(13, 13);
            this.plabel2.TabIndex = 7;
            this.plabel2.Text = "0";
            // 
            // plabel3
            // 
            this.plabel3.AutoSize = true;
            this.plabel3.Location = new System.Drawing.Point(155, 141);
            this.plabel3.Name = "plabel3";
            this.plabel3.Size = new System.Drawing.Size(13, 13);
            this.plabel3.TabIndex = 8;
            this.plabel3.Text = "0";
            // 
            // FormRebuildSpatialIndexProgess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 210);
            this.Controls.Add(this.plabel3);
            this.Controls.Add(this.plabel2);
            this.Controls.Add(this.progressBar3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormRebuildSpatialIndexProgess";
            this.Text = "Rebuild Spatial Index Progess";
            this.Load += new System.EventHandler(this.FormRebuildSpatialIndexProgess_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormRebuildSpatialIndexProgess_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar progressBar3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label plabel2;
        private System.Windows.Forms.Label plabel3;
    }
}